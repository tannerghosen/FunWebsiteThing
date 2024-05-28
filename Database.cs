using MySql.Data.MySqlClient;
using BCrypt.Net;
using System.Data.SqlTypes;
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
        public string Register(string email, string username, string password)
        {;
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
