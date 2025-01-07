using Microsoft.Data.Sqlite;
/*
 * SQLite Error Codes for Results:
 * https://www.sqlite.org/rescode.html
 */

namespace FunWebsiteThing
{
    // This class is for anything involving SQL in this project
    public static class SQLStuff
    {
        public static string ConnectionString = "";
        // Connects to our database
        public static SqliteConnection Connect()
        {
            string connect = ConnectionString;
            return new SqliteConnection(connect);
        }

        // Creates database file and tables if they don't exist
        public static void Init()
        {
            // Make database in case it doesn't exist
            if (!File.Exists("database.db"))
            {
                using (var con = new SqliteConnection(ConnectionString))
                {
                    Logger.Write("Creating database.db file with following connection string: " + ConnectionString + " .");
                    con.Open();
                    con.Close();
                }
            }

            using (var con = Connect())
            {
                con.Open();

                // Blog Table
                string blog = "CREATE TABLE IF NOT EXISTS blog (id INTEGER PRIMARY KEY AUTOINCREMENT, title TEXT NOT NULL, message TEXT NOT NULL, date DATETIME DEFAULT CURRENT_TIMESTAMP)";
                using (var cmd = new SqliteCommand(blog, con))
                {
                    cmd.ExecuteNonQuery();
                }

                // Comments Table
                string comments = "CREATE TABLE IF NOT EXISTS comments (id INTEGER PRIMARY KEY AUTOINCREMENT, commentsid INTEGER NOT NULL, userid INTEGER NOT NULL, comment NVARCHAR(255), date DATETIME DEFAULT CURRENT_TIMESTAMP)";
                using (var cmd = new SqliteCommand(comments, con))
                {
                    cmd.ExecuteNonQuery();
                }

                // Accounts Table
                string accounts = "CREATE TABLE IF NOT EXISTS accounts (id INTEGER PRIMARY KEY AUTOINCREMENT, email TEXT NOT NULL UNIQUE, username TEXT NOT NULL UNIQUE, password TEXT NOT NULL, sessionid INTEGER, sessiontoken TEXT, isadmin BOOLEAN DEFAULT 0)";
                using (var cmd = new SqliteCommand(accounts, con))
                {
                    cmd.ExecuteNonQuery();
                }

                // Security Questions Table
                string securityquestions = "CREATE TABLE IF NOT EXISTS securityquestions (id INTEGER PRIMARY KEY, question TEXT NOT NULL, answer TEXT NOT NULL, FOREIGN KEY (id) REFERENCES accounts(id))";
                using (var cmd = new SqliteCommand(securityquestions, con))
                {
                    cmd.ExecuteNonQuery();
                }

                // Admin Account
                string isadminalive = "SELECT COUNT(*) FROM accounts WHERE id = 0";
                using (var c = new SqliteCommand(isadminalive, con))
                {
                    int count = Convert.ToInt32(c.ExecuteScalar());
                    if (count == 0)
                    {
                        string pass = BCrypt.Net.BCrypt.HashPassword("test");
                        string createadmin = "INSERT INTO accounts (id, email, username, password, isadmin) VALUES (0, 'admin@email.com', 'admin', @pass, 1)";
                        using (var cmd = new SqliteCommand(createadmin, con))
                        {
                            cmd.Parameters.AddWithValue("@pass", pass);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // Anonymous Account
                string youcallanonymously = "SELECT COUNT(*) FROM accounts WHERE id = -1";
                using (var c = new SqliteCommand(youcallanonymously, con))
                {
                    int count = Convert.ToInt32(c.ExecuteScalar());
                    if (count == 0)
                    {
                        string pass = BCrypt.Net.BCrypt.HashPassword("tSFSDAKFSDJKGFISDJTR89324JR283JI213HE812H3E8D1H2IKASKFHDASKDFHKASHDKASHDKAH1231241251241231;;'===---+++SDA");
                        string createanonymous = "INSERT INTO accounts (id, email, username, password) VALUES (-1, 'anonymous@email.com', 'Anonymous', @pass)";
                        using (var cmd = new SqliteCommand(createanonymous, con))
                        {
                            cmd.Parameters.AddWithValue("@pass", pass);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // Delete Account Trigger
                string deleteaccounttrigger = "CREATE TRIGGER IF NOT EXISTS deleteaccounttrigger AFTER DELETE ON accounts FOR EACH ROW BEGIN DELETE FROM securityquestions WHERE id = OLD.id; UPDATE comments SET userid = -1 WHERE userid = OLD.id; END";
                using (var cmd = new SqliteCommand(deleteaccounttrigger, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Registers an account by first running a SQL statement to see if it the account exists. If it does, don't do anything.
        // If it doesn't, run another SQL statement that inserts it into the table, alongside generating a salt to hash our password.
        // (first bool is did operation succeed, second bool is did an error occur. the first bool will never be true if the second one is true.)
        public static async Task<(bool, bool)> Register(string email, string username, string password, int sessionid = 0, string stoken = "")
        {
            try
            {
                using (var con = Connect())
                {
                    con.Open();
                    string query = "SELECT COUNT(*) FROM accounts WHERE email = @email OR username = @username";
                    using (var cmd = new SqliteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@email", email);
                        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        if (count > 0)
                        {
                            return (false, false);
                        }
                    }
                    query = "INSERT INTO accounts (email, username, password, sessionid) VALUES (@email, @username, @password, @sid)";
                    using (var cmd = new SqliteCommand(query, con))
                    {
                        string salt = BCrypt.Net.BCrypt.GenerateSalt(12);
                        string hashpass = BCrypt.Net.BCrypt.HashPassword(password, salt);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", hashpass);
                        cmd.Parameters.AddWithValue("@sid", sessionid);
                        await cmd.ExecuteNonQueryAsync();
                        return (true, false);
                    }
                }
            }
            catch (SqliteException e)
            {
                Logger.Write("SQLStuff: An error occured in Register: " + e.Message + "\nSQLStuff: Error Code: " + e.SqliteErrorCode, "ERROR");
                return (false, true);
            }
        }

        // Logs us into an account by running a SQL statement to see if the username is valid first. If it isn't, return false.
        // If it is, then we run another SQL statement that compares the hashed password with the password given using BCrypt.Verify
        // If it matches, we return true so Login.cshtml.cs can handle setting the session up. If not, we return false.
        // (first bool is did operation succeed, second bool is did an error occur. the first bool will never be true if the second one is true.)
        public static async Task <(bool, bool)> Login(string username, string password, int sessionid = 0, string stoken = "")
        {
            try
            {
                using (var con = Connect())
                {
                    con.Open();
                    string query = "SELECT COUNT(*) FROM accounts WHERE username = @username";
                    using (var cmd = new SqliteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        if (count == 0)
                        {
                            return (false, false);
                        }
                    }
                    query = "SELECT password FROM accounts WHERE username = @username";
                    using (var cmd = new SqliteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        var res = await cmd.ExecuteScalarAsync();
                        string hashedpassword = res.ToString();
                        if (!BCrypt.Net.BCrypt.Verify(password, hashedpassword)) // invalid password
                        {
                            return (false, false);
                        }
                        else // valid password
                        {
                            query = "SELECT sessionid FROM accounts WHERE username = @username";
                            using (var c = new SqliteCommand(query, con))
                            {
                                c.Parameters.AddWithValue("@username", username);
                                var result = await c.ExecuteScalarAsync();
                                int id = (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : -1; // Default the id to -1 if it's null or DBNull
                                if (sessionid != id || id == -1)
                                {
                                    query = "UPDATE accounts SET sessionid = @sid WHERE username = @username";
                                    using (var cm = new SqliteCommand(query, con))
                                    {
                                        cm.Parameters.AddWithValue("@sid", sessionid);
                                        cm.Parameters.AddWithValue("@username", username);
                                        await cm.ExecuteNonQueryAsync();
                                    }
                                    return (true, false);
                                }
                                else
                                {
                                    return (true, false);
                                }
                            }
                        }
                    }
                }
            }
            catch(SqliteException e)
            {
                Logger.Write("SQLStuff: An error occured in Login: " + e.Message + "\nSQLStuff: Error Code: " + e.SqliteErrorCode, "ERROR");
                return (false, true);
            }
        }

        // Adds a comment to a specified comment section
        public static async Task AddComment(string comment, string username = "Anonymous", int commentsection = 0)
        {
            int userid, anonymousid = -1;
            if (comment == null)
            {
                comment = "";
            }
            if (!DoesUserExist(username) || (username == "" || username == null || username == "Anonymous"))
            {
                userid = anonymousid;
            }
            else
            {
                userid = GetUserID(username);
            }
            try
            {
                using (var con = Connect())
                {
                    con.Open();
                    string q = "INSERT INTO comments (userid, commentsid, comment, date) VALUES (@userid, @commentsection, @comment, DATETIME('now', 'utc', '-8 hours'))";
                    using (var cmd = new SqliteCommand(q, con))
                    {
                        cmd.Parameters.AddWithValue("@userid", userid);
                        cmd.Parameters.AddWithValue("@comment", comment);
                        cmd.Parameters.AddWithValue("@commentsection", commentsection);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    Logger.Write("Comment added by " + username + " to comment section id " + commentsection);
                }
            }
            catch (SqliteException e)
            {
                Logger.Write("SQLStuff: An error occured in AddComment: " + e.Message + "\nSQLStuff: Error Code: " + e.SqliteErrorCode, "ERROR");
            }
        }

        // Grabs usernames, comments, and dates from database and returns them as arrays
        public static string[][] GrabComments(int section = 0)
        {
            List<string> usernames = new List<string>();
            List<string> comments = new List<string>();
            List<string> dates = new List<string>();
            List<string> ids = new List<string>();
            try
            {
                using (var con = Connect())
                {
                    con.Open();
                    // SELECT account username, comments comment, comments data FROM comments JOIN accounts on comments userid = accounts id WHERE commentsid = section ORDER BY comments date DESC
                    string query = @"SELECT a.username, c.comment, c.date, c.id FROM comments c JOIN accounts a ON a.id = c.userid WHERE c.commentsid = @section ORDER BY c.date DESC";
                    using (var cmd = new SqliteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@section", section);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader != null)
                            {
                                while (reader.Read())
                                {
                                    usernames.Add(reader.GetString(0));
                                    comments.Add(reader.GetString(1));
                                    dates.Add(reader.GetString(2));
                                    ids.Add(reader.GetString(3));
                                }
                            }
                        }
                    }
                    return new string[][] { usernames.ToArray(), comments.ToArray(), dates.ToArray(), ids.ToArray() };
                }
            }
            catch (SqliteException e)
            {
                Logger.Write("SQLStuff: An error occured in GrabComments: " + e.Message + "\nSQLStuff: Error Code: " + e.SqliteErrorCode, "ERROR");
                return null;
            }
        }

        public static async Task DeleteComment(int? commentid)
        {
            try
            {
                using (var con = Connect())
                {
                    con.Open();
                    string query = "DELETE FROM comments WHERE id = @id";
                    using (var cmd = new SqliteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", commentid);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                Logger.Write("Deleted comment with id " + commentid);
            }
            catch (SqliteException e)
            {
                Logger.Write("SQLStuff: An error occured in DeleteComment: " + e.Message + "\nSQLStuff: Error Code: " + e.SqliteErrorCode, "ERROR");
            }
        }

        // Used to ensure the user does actually exist before we get too far in with various methods
        public static bool DoesUserExist(string username)
        {
            try
            {
                using (var con = Connect())
                {
                    con.Open();
                    string query = "SELECT COUNT(*) FROM accounts WHERE username = @username";
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
            catch (SqliteException e)
            {
                Logger.Write("SQLStuff: An error occured in DoesUserExist (string username parameter variant): " + e.Message + "\nSQLStuff: Error Code: " + e.SqliteErrorCode, "ERROR");
                return false;
            }
        }

        // Same as above but userid variant, in case this is more preferable in the future
        public static bool DoesUserExist(int? userid)
        {
            try
            {
                using (var con = Connect())
                {
                    con.Open();
                    string query = "SELECT COUNT(*) FROM accounts WHERE id = @userid";
                    using (var cmd = new SqliteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@userid", userid);
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
            catch (SqliteException e)
            {
                Logger.Write("SQLStuff: An error occured in DoesUserExist (int? userid parameter variant): " + e.Message + "\nSQLStuff: Error Code: " + e.SqliteErrorCode, "ERROR");
                return false;
            }
        }

        // Get User ID by Username
        public static int GetUserID(string username)
        {
            try
            {
                using (var con = Connect())
                {
                    con.Open();
                    string query = "SELECT id FROM accounts WHERE username = @username";
                    using (var cmd = new SqliteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        int id = Convert.ToInt32(cmd.ExecuteScalar());
                        return id;
                    }
                }
            }
            catch (SqliteException e)
            {
                Logger.Write("SQLStuff: An error occured in GetUserID: " + e.Message + "\nSQLStuff: Error Code: " + e.SqliteErrorCode, "ERROR");
                return -1;
            }
        }

        // Get Username by UserID
        public static string? GetUsername(int userid)
        {
            try
            {
                using (var con = Connect())
                {
                    con.Open();
                    string query = "SELECT username FROM accounts WHERE id = @userid";
                    using (var cmd = new SqliteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@userid", userid);
                        return cmd.ExecuteScalar()?.ToString();
                    }
                }
            }
            catch (SqliteException e)
            {
                Logger.Write("SQLStuff: An error occured in GetUserID: " + e.Message + "\nSQLStuff: Error Code: " + e.SqliteErrorCode, "ERROR");
                return null;
            }
        }

        // Does Session ID Match
        public static bool DoesSIDMatch(int? userid, int? sid)
        {
            bool usercheck = DoesUserExist(userid);
            if (usercheck)
            {
                try
                {
                    using (var con = Connect())
                    {
                        con.Open();
                        string query = "SELECT sessionid FROM accounts WHERE id = @userid";
                        using (var cmd = new SqliteCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@userid", userid);
                            int id = Convert.ToInt32(cmd.ExecuteScalar());
                            if (id != sid)
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
                catch (SqliteException e)
                {
                    Logger.Write("SQLStuff: An error occured in DoesSIDMatch: " + e.Message + "\nSQLStuff: Error Code: " + e.SqliteErrorCode, "ERROR");
                    return false;
                }
            }
            return false;
        }

        // Does Session Token Match
        public static bool DoesSTokenMatch(int? userid, string? stoken)
        {
            bool usercheck = DoesUserExist(userid);
            if (usercheck)
            {
                try
                {
                    using (var con = Connect())
                    {
                        con.Open();
                        string query = "SELECT sessiontoken FROM accounts WHERE id = @userid";
                        using (var cmd = new SqliteCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@userid", userid);
                            string token = Convert.ToString(cmd.ExecuteScalar());
                            if (token != stoken)
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
                catch (SqliteException e)
                {
                    Logger.Write("SQLStuff: An error occured in DoesSTokenMatch: " + e.Message + "\nSQLStuff: Error Code: " + e.SqliteErrorCode, "ERROR");
                    return false;
                }
            }
            return false;
        }

        // Updates various settings of a specified user (by username)'s account.
        // (first bool is did operation succeed, second bool is did an error occur. the first bool will never be true if the second one is true.)
        public static async Task<(bool, bool)> UpdateInfo(int? userid, int option, string input, int? sessionid = 0, bool adminupdate = false)
        {
            if ((DoesSIDMatch(userid, sessionid) || adminupdate == true) && (userid != -1 && userid != 0))
            {
                try
                {
                    using (var con = Connect())
                    {
                        con.Open();
                        switch (option)
                        {
                            case 0: // password
                                string updatepassword = "UPDATE accounts SET password = @password WHERE id = @id";
                                using (var c = new SqliteCommand(updatepassword, con))
                                {
                                    string pass = BCrypt.Net.BCrypt.HashPassword(input);
                                    c.Parameters.AddWithValue("@id", userid);
                                    c.Parameters.AddWithValue("@password", pass);
                                    await c.ExecuteNonQueryAsync();
                                }
                                return (true, false);
                            case 1: // email
                                string updateemail = "UPDATE accounts SET email = @email WHERE id = @id";
                                using (var c = new SqliteCommand(updateemail, con))
                                {
                                    c.Parameters.AddWithValue("@id", userid);
                                    c.Parameters.AddWithValue("@email", input);
                                    try
                                    {
                                        await c.ExecuteNonQueryAsync();
                                    }
                                    catch
                                    {
                                        return (false, false); // dup email
                                    }
                                }
                                return (true, false);
                            case 2: // username
                                string updateusername = "UPDATE accounts SET username = @newusername WHERE id = @id";
                                using (var c = new SqliteCommand(updateusername, con))
                                {
                                    c.Parameters.AddWithValue("@id", userid);
                                    c.Parameters.AddWithValue("@newusername", input);
                                    try
                                    {
                                       await c.ExecuteNonQueryAsync();
                                    }
                                    catch
                                    {
                                        return (false, false); // dup username
                                    }
                                }
                                return (true, false);
                            default:
                                return (false, false);
                        }
                    }
                }
                catch (SqliteException e)
                {
                    Logger.Write("SQLStuff: An error occured in UpdateInfo: " + e.Message + "\nSQLStuff: Error Code: " + e.SqliteErrorCode, "ERROR");
                    return (false, true); // error happened due to sql issue, so yea let's say an error occured
                }
            }
            else
            {
                return (false, true);
            }
        }

        // Grabs the entire table of accounts and returns an array of all 6 columns
        public static string[]?[]? GrabAccountsTable()
        {
            try
            {
                using (var con = Connect())
                {
                    con.Open();
                    string query = "SELECT * FROM accounts";
                    using (var cmd = new SqliteCommand(query, con))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            List<string[]> rows = new List<string[]>(); // create a List of string arrays called rows
                            while (reader.Read())
                            {
                                string[] row = new string[7]; // create a string array called row where all 7 columns are stored in
                                row[0] = reader.GetInt32(0).ToString(); // id
                                row[1] = reader.GetString(1); // email
                                row[2] = reader.GetString(2); // username
                                row[3] = reader.GetString(3); // password
                                row[4] = reader.IsDBNull(4) ? "" : reader.GetInt32(4).ToString(); // sessionid
                                row[5] = reader.IsDBNull(5) ? "" : reader.GetString(5); // sessiontoken
                                row[6] = reader.IsDBNull(6) ? null : reader.GetBoolean(6).ToString(); // is admin?
                                rows.Add(row); // add row to the rows List
                            }
                            return rows.ToArray(); // convert the List to an array and return it
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                Logger.Write("SQLStuff: An error occurred in GrabAccountsTable: " + e.Message + "\nSQLStuff: Error Code: " + e.SqliteErrorCode, "ERROR");
                return null;
            }
        }

        // checks if user is admin
        public static bool IsAdmin(int? userid)
        {
            bool usercheck = DoesUserExist(userid);
            if (usercheck)
            {
                try
                {
                    using (var con = Connect())
                    {
                        con.Open();
                        string query = "SELECT isadmin FROM accounts WHERE id = @userid";
                        using (var cmd = new SqliteCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@userid", userid);
                            var result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value) // if result isn't null or result isn't dbnull 
                            {
                                // isadmin is stored as a boolean (which defaults to 0 (false) or 1 (true) regardless of how inserted or stored)
                                return Convert.ToInt32(result) == 1;
                            }
                        }
                    }
                }
                catch (SqliteException e)
                {
                    Logger.Write("SQLStuff: An error occured in IsAdmin " + e.Message + "\nSQLStuff: Error Code: " + e.SqliteErrorCode, "ERROR");
                    return false;
                }
            }
            return false;
        }

        // Deletes a user from the accounts table
        public static async Task DeleteUser(int? userid)
        {
            bool usercheck = DoesUserExist(userid);
            if (usercheck && (userid != -1 && userid != 0))
            {
                try
                {
                    using (var con = Connect())
                    {
                        con.Open();
                        string query = "DELETE FROM accounts WHERE id = @userid";
                        using (var cmd = new SqliteCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@userid", userid);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                catch (SqliteException e)
                {
                    Logger.Write("SQLStuff: An error occured in DeleteUser: " + e.Message + "\nSQLStuff: Error Code: " + e.SqliteErrorCode, "ERROR");
                }
            }
        }

        // Makes user an admin
        public static async Task AdminUser(int? userid)
        {
            bool usercheck = DoesUserExist(userid);
            if (usercheck && (userid != -1 && userid != 0))
            {
                try
                {
                    using (var con = Connect())
                    {
                        con.Open();
                        string query = "UPDATE accounts SET isadmin = @isadmin WHERE id = @userid";
                        using (var cmd = new SqliteCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@userid", userid);
                            cmd.Parameters.AddWithValue("@isadmin", IsAdmin(userid) ? 0 : 1);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                catch (SqliteException e)
                {
                    Logger.Write("SQLStuff: An error occured in AdminUser: " + e.Message + "\nSQLStuff: Error Code: " + e.SqliteErrorCode, "ERROR");
                }
            }
        }

        public static async Task AddBlogPost(string title, string body)
        {
        }

        public static async Task UpdateBlogPost(string title, string body, int? blogid)
        {
        }

        public static async Task DeleteBlogPost(int? blogid)
        {
        }

        public static void UpdateDatabasePassword(string password)
        {
            try
            {
                // Update the password in the connection strings
                string NewConnectionString = $"Data Source=database.db;Password={password}";

                // Update the password in the database
                using (var con = Connect())
                {
                    con.Open();

                    // Update the password in the accounts table
                    string query = "PRAGMA rekey = @newPassword";
                    using (var cmd = new SqliteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@newPassword", password);
                        cmd.ExecuteNonQuery();
                    }
                }
                ConnectionString = NewConnectionString;
                Settings.UpdateSettings("ConnectionString", ConnectionString);

                // Log the successful password update
                Logger.Write("Database password updated successfully.");
            }
            catch (SqliteException e)
            {
                Logger.Write("An error occurred while updating the database password: " + e.Message, "ERROR");
            }
        }
        public static void SetConnectionString(string cs)
        {
            ConnectionString = cs;
        }
    }
}
