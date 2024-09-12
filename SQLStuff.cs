using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Http;
using System.Xml.Linq;

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
                    con.Close();
                }
            }
            using (var con = Connect())
            {
                con.Open();
                string command = "CREATE TABLE IF NOT EXISTS accounts (id INTEGER PRIMARY KEY AUTOINCREMENT, email TEXT NOT NULL UNIQUE, username TEXT NOT NULL, password TEXT NOT NULL)";
                using (var cmd = new SqliteCommand(command, con))
                {
                    cmd.ExecuteNonQuery();
                }
                string com = "CREATE TABLE IF NOT EXISTS comments (id INTEGER PRIMARY KEY AUTOINCREMENT, userid INTEGER NOT NULL, comment NVARCHAR(255), date DATETIME DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY (userid) REFERENCES accounts(id))";
                using (var cmd = new SqliteCommand(com, con))
                {
                    cmd.ExecuteNonQuery();
                }
                string isadminalive = "SELECT COUNT(*) FROM accounts WHERE id = -1999";
                using (var c = new SqliteCommand(isadminalive, con))
                {
                    int count = Convert.ToInt32(c.ExecuteScalar());
                    if (count == 0)
                    {
                        string pass = BCrypt.Net.BCrypt.HashPassword("test");
                        string createadmin = "INSERT INTO accounts (id, email, username, password) VALUES (-1999, \"admin@email.com\", \"admin\", @pass)";
                        using (var cmd = new SqliteCommand(createadmin, con))
                        {
                            cmd.Parameters.AddWithValue("@pass", pass);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                string youcallanonymously = "SELECT COUNT(*) FROM accounts WHERE id = -1";
                using (var c = new SqliteCommand(youcallanonymously, con))
                {
                    int count = Convert.ToInt32(c.ExecuteScalar());
                    if (count == 0)
                    {
                        string pass = BCrypt.Net.BCrypt.HashPassword("tSFSDAKFSDJKGFISDJTR89324JR283JI213HE812H3E8D1H2IKASKFHDASKDFHKASHDKASHDKAH1231241251241231;;'===---+++SDA");
                        string createanonymous = "INSERT INTO accounts (id, email, username, password) VALUES (-1, \"anonymous@localhost.com\", \"Anonymous\", @pass)";
                        using (var cmd = new SqliteCommand(createanonymous, con))
                        {
                            cmd.Parameters.AddWithValue("@pass", pass);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                con.Close();
            }
        }

        public void AddComment(string comment, string username = "Anonymous")
        {
            int userid = 0;
            int anonymousid = -1;
            if (comment == null)
            {
                comment = "";
            }
            using (var con = Connect())
            {
                con.Open();
                string query = "SELECT id FROM accounts WHERE username = @username";
                using (var cmd = new SqliteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    var result = cmd.ExecuteScalar();
                    if (result == null)
                    {
                        userid = anonymousid;
                    }
                    else
                    {
                        userid = Convert.ToInt32(result);
                    }
                }
                string q = "INSERT INTO comments (userid, comment) VALUES (@userid, @comment)";
                Console.WriteLine(q);
                using (var cmd = new SqliteCommand(q, con))
                {
                    cmd.Parameters.AddWithValue("@userid", userid);
                    cmd.Parameters.AddWithValue("@comment", comment);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public (string[], string[], string[]) GrabComments()
        {
            List<string> usernames = new List<string>();
            List<string> comments = new List<string>();
            List<string> dates = new List<string>();
            using (var con = Connect())
            {
                con.Open();
                string query = @"SELECT a.username, c.comment, c.date FROM comments c JOIN accounts a ON c.userid = a.id ORDER BY c.date DESC;";
                using (var cmd = new SqliteCommand(query, con))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            usernames.Add(reader.GetString(0)); 
                            comments.Add(reader.GetString(1));
                            dates.Add(reader.GetString(2)); 
                        }
                    }
                }
                return (usernames.ToArray(), comments.ToArray(), dates.ToArray());
            }
        }

        public bool DoesUserExist(string username)
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
                    else
                    {
                        return true;
                    }
                }
            }
        }

        public bool UpdateInfo(string username, int option, string input)
        {
            using (var con = Connect())
            {
                con.Open();
                string query = @"SELECT * FROM accounts WHERE username = @username";
                using (var cmd = new SqliteCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    if (count != 0)
                    {
                        switch (option)
                        {
                            case 0: // password
                                string updatepassword = "UPDATE accounts SET password = @password WHERE username = @username";
                                using (var c = new SqliteCommand(updatepassword, con))
                                {
                                    string pass = BCrypt.Net.BCrypt.HashPassword(input);
                                    c.Parameters.AddWithValue("@username", username);
                                    c.Parameters.AddWithValue("@password", pass);
                                    c.ExecuteNonQuery();
                                }
                                return true;
                                break;
                            case 1: // email
                                string updateemail = "UPDATE accounts SET email = @email WHERE username = @username";
                                using (var c = new SqliteCommand(updateemail, con))
                                {
                                    c.Parameters.AddWithValue("@username", username);
                                    c.Parameters.AddWithValue("@email", input);
                                    c.ExecuteNonQuery();
                                }
                                return true;
                                break;
                            case 2: // username
                                string updateusername = "UPDATE accounts SET username = @newusername WHERE username = @username";
                                using (var c = new SqliteCommand(updateusername, con))
                                {
                                    c.Parameters.AddWithValue("@username", username);
                                    c.Parameters.AddWithValue("@newusername", input);
                                    c.ExecuteNonQuery();
                                }
                                return true;
                                break;
                            default: // shouldn't happen
                                return false;
                                break;
                        }
                    }
                    return false;
                }
            }
        }
    }
}
