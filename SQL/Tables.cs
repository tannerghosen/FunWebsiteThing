using MySql.Data.MySqlClient;

namespace FunWebsiteThing.SQL
{
    // Creates all the tables in the database
    public static class Tables
    {
        public static void Accounts()
        {
            using (var con = Main.Connect())
            {
                con.Open();

                // Accounts Table
                string accounts = "CREATE TABLE IF NOT EXISTS accounts (id INT(11) PRIMARY KEY AUTO_INCREMENT, email VARCHAR(255) NOT NULL UNIQUE, username VARCHAR(50) NOT NULL UNIQUE, password TEXT NOT NULL, sessionid INT(11), isadmin TINYINT DEFAULT 0)";
                using (var cmd = new MySqlCommand(accounts, con))
                {
                    cmd.ExecuteNonQuery();
                }

                con.Close();
            }
        }

        public static void SecurityQuestion()
        {
            using (var con = Main.Connect())
            {
                con.Open();

                // Security Questions Table
                string securityquestion = "CREATE TABLE IF NOT EXISTS securityquestion (id INT(11) PRIMARY KEY, question VARCHAR(255) NOT NULL, answer VARCHAR(255) NOT NULL, FOREIGN KEY (id) REFERENCES accounts(id) ON DELETE CASCADE)";
                using (var cmd = new MySqlCommand(securityquestion, con))
                {
                    cmd.ExecuteNonQuery();
                }

                con.Close();
            }
        }
        public static void Blog()
        {
            using (var con = Main.Connect())
            {
                con.Open();

                string blog = "CREATE TABLE IF NOT EXISTS blog (id INT(11) PRIMARY KEY AUTO_INCREMENT, title VARCHAR(255) NOT NULL, message TEXT(65025) NOT NULL, date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP)";
                using (var cmd = new MySqlCommand(blog, con))
                {
                    cmd.ExecuteNonQuery();
                }

                con.Close();
            }
        }

        public static void Comments()
        {
            using (var con = Main.Connect())
            {
                con.Open();

                string comments = "CREATE TABLE IF NOT EXISTS comments (id INT(11) PRIMARY KEY AUTO_INCREMENT, commentsid INT(11) NOT NULL, userid INT(11) NOT NULL, comment VARCHAR(2550), date DATETIME DEFAULT CURRENT_TIMESTAMP)";
                using (var cmd = new MySqlCommand(comments, con))
                {
                    cmd.ExecuteNonQuery();
                }

                con.Close();
            }
        }

        public static void Stats()
        {
            using (var con = Main.Connect())
            {
                con.Open();

                string stats = "CREATE TABLE IF NOT EXISTS stats (stat VARCHAR(50) PRIMARY KEY, count INT(11) NOT NULL)";
                using (var cmd = new MySqlCommand(stats, con))
                {
                    cmd.ExecuteNonQuery();
                }

                string login = "SELECT COUNT(*) FROM stats WHERE stat = 'logins'";
                using (var c = new MySqlCommand(login, con))
                {
                    int count = Convert.ToInt32(c.ExecuteScalar());
                    if (count == 0)
                    {
                        string createlogin = "INSERT INTO stats (stat, count) VALUES ('logins', 0)";
                        using (var cmd = new MySqlCommand(createlogin, con))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                string registration = "SELECT COUNT(*) FROM stats WHERE stat = 'registrations'";
                using (var c = new MySqlCommand(registration, con))
                {
                    int count = Convert.ToInt32(c.ExecuteScalar());
                    if (count == 0)
                    {
                        string createregistrations = "INSERT INTO stats (stat, count) VALUES ('registrations', 0)";
                        using (var cmd = new MySqlCommand(createregistrations, con))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                string errors = "SELECT COUNT(*) FROM stats WHERE stat = 'errors'";
                using (var c = new MySqlCommand(errors, con))
                {
                    int count = Convert.ToInt32(c.ExecuteScalar());
                    if (count == 0)
                    {
                        string createerrors = "INSERT INTO stats (stat, count) VALUES ('errors', 0)";
                        using (var cmd = new MySqlCommand(createerrors, con))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                con.Close();
            }
        }

        public static void Bans()
        {
            using (var con = Main.Connect())
            {
                con.Open();

                string bans = "CREATE TABLE IF NOT EXISTS bans (id INT(11) PRIMARY KEY AUTO_INCREMENT, ip VARCHAR(39), reason VARCHAR(255) DEFAULT 'You have been banned.', expire DATETIME DEFAULT CURRENT_TIMESTAMP)";
                using (var cmd = new MySqlCommand(bans, con))
                {
                    cmd.ExecuteNonQuery();
                }

                con.Close();
            }
        }

        public static void AccountBans()
        {
            using (var con = Main.Connect())
            {
                con.Open();

                string bans = "CREATE TABLE IF NOT EXISTS accountbans (id INT(11) PRIMARY KEY, banned BOOL, reason VARCHAR(255) DEFAULT 'You have been banned.', expire DATETIME DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY (id) REFERENCES accounts(id) ON DELETE CASCADE)";
                using (var cmd = new MySqlCommand(bans, con))
                {
                    cmd.ExecuteNonQuery();
                }

                con.Close();
            }
        }
    }
}
