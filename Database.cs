using MySql.Data.MySqlClient;

namespace LearningASPNETAndRazor
{
    public class Database
    {
        public MySqlConnection Connect()
        {
            string connect = "Server=127.0.0.1;Port=3306;Database=test;User Id=root;";
            var con = new MySqlConnection(connect);
            con.Open();
            return con;
        }
        public string Register(string username, string password)
        {
            MySqlConnection con = Connect();
            string query = "SELECT COUNT(*) FROM accounts WHERE username = @username";
            Console.WriteLine(query);
            using (var cmd = new MySqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("username", username);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count > 0)
                {
                    return "Username already exists!";
                }
            }
            query = "INSERT INTO accounts (username, password) VALUES (@username, @password)";
            Console.WriteLine(query);
            using (var cmd = new MySqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);
                cmd.ExecuteNonQuery();
                return "Account Added: " + username;
            }
        }

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
                string pass = cmd.ExecuteScalar().ToString();
                if (pass != password) // invalid password
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
