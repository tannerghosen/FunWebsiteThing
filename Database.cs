using MySql.Data.MySqlClient;
namespace LearningASPNETAndRazor
{
    // This class is injected into our Login.cshtml and Register.cshtmls
    public class Database
    {
        // Connects to our database
        public MySqlConnection Connect()
        {
            string connect = "Server=127.0.0.1;Port=3306;Database=test;User Id=root;";
            var con = new MySqlConnection(connect);
            con.Open();
            return con;
        }
        
        // Registers an account by first running a SQL statement to see if it the account exists. If it does, don't do anything.
        // If it doesn't, run another SQL statement that inserts it into the table, alongside generating a salt to hash our password.
        public string Register(string email, string username, string password)
        {
            MySqlConnection con = Connect();
            string query = "SELECT COUNT(*) FROM accounts WHERE email = @email OR username = @username";
            Console.WriteLine(query);
            using (var cmd = new MySqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("username", username);
                cmd.Parameters.AddWithValue("email", email);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count > 0)
                {
                    return "Email or username already exists!";
                }
            }
            query = "INSERT INTO accounts (email, username, password) VALUES (@email, @username, @password)";
            Console.WriteLine(query);
            using (var cmd = new MySqlCommand(query, con))
            {
                string salt = BCrypt.Net.BCrypt.GenerateSalt(12);
                string hashpass = BCrypt.Net.BCrypt.HashPassword(password, salt);
                Console.WriteLine(hashpass);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", hashpass);
                cmd.ExecuteNonQuery();
                return "Account Added: " + username;
            }
        }
        
        // "Logs us into" an account by running a SQL statement to see if the username is valid first. If it isn't, do nothing.
        // If it is, then we run another SQL statement that compares the hashed password with the password given using BCrypt.Verify
        // If it matches, we "log in", if not, we return the password is invalid.
        public string Login(string username, string password)
        {
            MySqlConnection con = Connect();
            string query = "SELECT COUNT(*) FROM accounts WHERE username = @username";
            Console.WriteLine(query);
            using (var cmd = new MySqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@username", username);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count == 0)
                {
                    return "Username doesn't exist!";
                }
            }
            query = "SELECT password FROM accounts WHERE username = @username";
            Console.WriteLine(query);
            using (var cmd = new MySqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@username", username);
                string hash = cmd.ExecuteScalar().ToString();
                Console.WriteLine(hash + " " + password);
                if (!BCrypt.Net.BCrypt.Verify(password, hash)) // invalid password
                {
                    return "Invalid password!";
                }
                else // valid password
                {
                    return "Logged into: " + username;
                }
            }
        }
    }
}
