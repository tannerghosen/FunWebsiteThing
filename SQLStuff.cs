using Microsoft.Data.Sqlite;
namespace LearningASPNETAndRazor
{
    // This class is injected into our Login.cshtml and Register.cshtmls
    public class SQLStuff
    {
        // Connects to our database
        public SqliteConnection Connect()
        {
            string connect = "Data Source=database.db";
            return new SqliteConnection(connect);
        }
        
        // Registers an account by first running a SQL statement to see if it the account exists. If it does, don't do anything.
        // If it doesn't, run another SQL statement that inserts it into the table, alongside generating a salt to hash our password.
        public bool Register(string email, string username, string password)
        {
            using (var con = Connect())
            {
                con.Open();
                string query = "SELECT COUNT(*) FROM accounts WHERE email = @email OR username = @username";
                Console.WriteLine(query);
                using (var cmd = new SqliteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@email", email);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    if (count > 0)
                    {
                        return false;
                    }
                }
                query = "INSERT INTO accounts (email, username, password) VALUES (@email, @username, @password)";
                Console.WriteLine(query);
                using (var cmd = new SqliteCommand(query, con))
                {
                    string salt = BCrypt.Net.BCrypt.GenerateSalt(12);
                    string hashpass = BCrypt.Net.BCrypt.HashPassword(password, salt);
                    Console.WriteLine(hashpass);
                    cmd.Parameters.AddWithValue("@email", email);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", hashpass);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
        }
        
        // "Logs us into" an account by running a SQL statement to see if the username is valid first. If it isn't, do nothing.
        // If it is, then we run another SQL statement that compares the hashed password with the password given using BCrypt.Verify
        // If it matches, we "log in", if not, we return the password is invalid.
        public bool Login(string username, string password)
        {
            using (var con = Connect())
            {
                con.Open();
                string query = "SELECT COUNT(*) FROM accounts WHERE username = @username";
                Console.WriteLine(query);
                using (var cmd = new SqliteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    if (count == 0)
                    {
                        return false;
                    }
                }
                query = "SELECT password FROM accounts WHERE username = @username";
                Console.WriteLine(query);
                using (var cmd = new SqliteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    string hash = cmd.ExecuteScalar().ToString();
                    Console.WriteLine(hash + " " + password);
                    if (!BCrypt.Net.BCrypt.Verify(password, hash)) // invalid password
                    {
                        return false;
                    }
                    else // valid password
                    {
                        return true;
                    }
                }
            }
        }

        // Creates database file and accounts table if the file doesn't exist
        public void Init()
        {
            if (!File.Exists("database.db"))
            {
                using (var con = new SqliteConnection($"Data Source=database.db"))
                {
                    con.Open();
                    string command = "CREATE TABLE IF NOT EXISTS accounts (id INTEGER PRIMARY KEY AUTOINCREMENT, email TEXT NOT NULL, username TEXT NOT NULL, password TEXT NOT NULL)";
                    using (var cmd = new SqliteCommand(command, con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    /*string createaccount = "INSERT INTO accounts (email, username, password) VALUES (\"test@testing.com\", \"admin\", \"test\")";
                    using (var cmd = new SqliteCommand(createaccount, con))
                    {
                        cmd.ExecuteNonQuery();
                    }*/
                }
            }
        }

        // to do
        public bool UpdateInfo(int option, string input)
        {
            using (var con = Connect())
            {
                con.Open();
            }
            return false;
        }
    }
}
