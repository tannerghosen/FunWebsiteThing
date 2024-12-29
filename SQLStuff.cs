﻿using Microsoft.Data.Sqlite;
/*
 * SQLite Error Codes for Results:
 * https://www.sqlite.org/rescode.html
 */

namespace FunWebsiteThing
{
    // This class is for anything involving SQLite in this project, which includes so far: login/registration, account settings and comments.
    public class SQLStuff
    {
        // Connects to our database
        public SqliteConnection Connect()
        {
            string connect = "Data Source=database.db";
            return new SqliteConnection(connect);
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
                string command = "CREATE TABLE IF NOT EXISTS accounts (id INTEGER PRIMARY KEY AUTOINCREMENT, email TEXT NOT NULL UNIQUE, username TEXT NOT NULL UNIQUE, password TEXT NOT NULL, sessionid INTEGER, sessiontoken TEXT, isadmin BOOLEAN DEFAULT FALSE)";
                using (var cmd = new SqliteCommand(command, con))
                {
                    cmd.ExecuteNonQuery();
                }
                string com = "CREATE TABLE IF NOT EXISTS comments (id INTEGER PRIMARY KEY AUTOINCREMENT, commentsid INTEGER NOT NULL, userid INTEGER NOT NULL, comment NVARCHAR(255), date DATETIME DEFAULT CURRENT_TIMESTAMP, FOREIGN KEY (userid) REFERENCES accounts(id))";
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
                        string createadmin = "INSERT INTO accounts (id, email, username, password, isadmin) VALUES (-1999, \"admin@email.com\", \"admin\", @pass, TRUE)";
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
                        string createanonymous = "INSERT INTO accounts (id, email, username, password) VALUES (-1, \"anonymous@email.com\", \"Anonymous\", @pass)";
                        using (var cmd = new SqliteCommand(createanonymous, con))
                        {
                            cmd.Parameters.AddWithValue("@pass", pass);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                /*string commenttrigger = "DROP TRIGGER IF EXISTS commentdate;CREATE TRIGGER commentdate BEFORE INSERT ON comments FOR EACH ROW BEGIN UPDATE comments SET date = DATETIME('now', 'utc', '-8 hours') WHERE commentsid = NEW.commentsid; END;";
                using (var c = new SqliteCommand(commenttrigger, con))
                {
                    c.ExecuteNonQuery();
                }
                con.Close();*/
            }
        }

        // Registers an account by first running a SQL statement to see if it the account exists. If it does, don't do anything.
        // If it doesn't, run another SQL statement that inserts it into the table, alongside generating a salt to hash our password.
        // (first bool is did operation succeed, second bool is did an error occur. the first bool will never be true if the second one is true.)
        public (bool, bool) Register(string email, string username, string password, int sessionid = 0, string stoken = "")
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
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
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
                        cmd.ExecuteNonQuery();
                        return (true, false);
                    }
                }
            }
            catch (SqliteException e)
            {
                Console.WriteLine("SQLStuff: An error occured in Register: " + e.Message + "\nSQLStuff: Error Code: " + e.SqliteErrorCode);
                return (false, true);
            }
        }

        // Logs us into an account by running a SQL statement to see if the username is valid first. If it isn't, return false.
        // If it is, then we run another SQL statement that compares the hashed password with the password given using BCrypt.Verify
        // If it matches, we return true so Login.cshtml.cs can handle setting the session up. If not, we return false.
        // (first bool is did operation succeed, second bool is did an error occur. the first bool will never be true if the second one is true.)
        public (bool, bool) Login(string username, string password, int sessionid = 0, string stoken = "")
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
                            return (false, false);
                        }
                    }
                    query = "SELECT password FROM accounts WHERE username = @username";
                    using (var cmd = new SqliteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        string hash = cmd.ExecuteScalar().ToString();
                        if (!BCrypt.Net.BCrypt.Verify(password, hash)) // invalid password
                        {
                            return (false, false);
                        }
                        else // valid password
                        {
                            query = "SELECT sessionid FROM accounts WHERE username = @username";
                            using (var c = new SqliteCommand(query, con))
                            {
                                c.Parameters.AddWithValue("@username", username);
                                var result = c.ExecuteScalar();
                                int id = (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : -999999999; // Default the id to 0 if it's null or DBNull
                                if (sessionid != id || id == -999999999)
                                {
                                    query = "UPDATE accounts SET sessionid = @sid WHERE username = @username";
                                    using (var cm = new SqliteCommand(query, con))
                                    {
                                        cm.Parameters.AddWithValue("@sid", sessionid);
                                        cm.Parameters.AddWithValue("@username", username);
                                        cm.ExecuteNonQuery();
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
                Console.WriteLine("SQLStuff: An error occured in Login: " + e.Message + "\nSQLStuff: Error Code: " + e.SqliteErrorCode);
                return (false, true);
            }
        }

        // Adds a comment to a specified comment section
        public void AddComment(string comment, string username = "Anonymous", int commentsection = 0)
        {
            int userid = 0;
            int anonymousid = -1;
            if (comment == null || comment == "")
            {
                comment = "";
            }
            try
            {
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
                    string q = "INSERT INTO comments (userid, commentsid, comment, date) VALUES (@userid, @commentsection, @comment, DATETIME('now', 'utc', '-8 hours'))";
                    using (var cmd = new SqliteCommand(q, con))
                    {
                        cmd.Parameters.AddWithValue("@userid", userid);
                        cmd.Parameters.AddWithValue("@comment", comment);
                        cmd.Parameters.AddWithValue("@commentsection", commentsection);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqliteException e)
            {
                Console.WriteLine("SQLStuff: An error occured in AddComment: " + e.Message + "\nSQLStuff: Error Code: " + e.SqliteErrorCode);
            }
        }

        // Grabs usernames, comments, and dates from database and returns them as arrays
        public string[][] GrabComments(int section = 0)
        {
            List<string> usernames = new List<string>();
            List<string> comments = new List<string>();
            List<string> dates = new List<string>();
            try
            {
                using (var con = Connect())
                {
                    con.Open();
                    string query = @"SELECT a.username, c.comment, c.date FROM comments c JOIN accounts a ON c.userid = a.id WHERE c.commentsid = @section ORDER BY c.date DESC;";
                    using (var cmd = new SqliteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@section", section);
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
                    return new string[][] { usernames.ToArray(), comments.ToArray(), dates.ToArray() };
                }
            }
            catch (SqliteException e)
            {
                Console.WriteLine("SQLStuff: An error occured in GrabComments: " + e.Message + "\nSQLStuff: Error Code: " +e.SqliteErrorCode);
                return null;
            }
        }

        // Used to ensure the user does actually exist before we get too far in with various methods
        public bool DoesUserExist(string username)
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
                Console.WriteLine("SQLStuff: An error occured in DoesUserExist (string username parameter variant): " + e.Message + "\nSQLStuff: Error Code: " + e.SqliteErrorCode);
                return false;
            }
        }

        // Same as above but userid variant, in case this is more preferable in the future
        public bool DoesUserExist(int? userid)
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
                Console.WriteLine("SQLStuff: An error occured in DoesUserExist (int? userid parameter variant): " + e.Message + "\nSQLStuff: Error Code: " + e.SqliteErrorCode);
                return false;
            }
        }

        // Get User ID by Username
        public int GetUserID(string username)
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
                        int id = Convert.ToInt32(cmd.ExecuteScalar());
                        return id;
                    }
                }
            }
            catch (SqliteException e)
            {
                Console.WriteLine("SQLStuff: An error occured in DoesUserExist: " + e.Message + "\nSQLStuff: Error Code: " + e.SqliteErrorCode);
                return -1;
            }
        }

        // Does Session ID Match
        public bool DoesSIDMatch(int? userid, int? sid)
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
                    Console.WriteLine("SQLStuff: An error occured in DoesSIDMatch: " + e.Message + "\nSQLStuff: Error Code: " + e.SqliteErrorCode);
                    return false;
                }
            }
            return false;
        }

        // Does Session Token Match
        public bool DoesSTokenMatch(int? userid, string? stoken)
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
                    Console.WriteLine("SQLStuff: An error occured in DoesSTokenMatch: " + e.Message + "\nSQLStuff: Error Code: " + e.SqliteErrorCode);
                    return false;
                }
            }
            return false;
        }

        // Updates various settings of a specified user (by username)'s account.
        // (first bool is did operation succeed, second bool is did an error occur. the first bool will never be true if the second one is true.)
        public (bool, bool) UpdateInfo(int? userid, int option, string input, int? sessionid = 0)
        {
            if (DoesSIDMatch(userid, sessionid))
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
                                    c.ExecuteNonQuery();
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
                                        c.ExecuteNonQuery();
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
                                        c.ExecuteNonQuery();
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
                    Console.WriteLine("SQLStuff: An error occured in UpdateInfo: " + e.Message + "\nSQLStuff: Error Code: " + e.SqliteErrorCode);
                    return (false, true); // error happened due to sql issue, so yea let's say an error occured
                }
            }
            else
            {
                return (false, true);
            }
        }

        // Grabs the entire table of accounts and returns an array of all 6 columns
        public string[][] GrabAccountsTable()
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
                            List<string[]> rows = new List<string[]>();
                            while (reader.Read())
                            {
                                string[] row = new string[7];
                                row[0] = reader.GetInt32(0).ToString(); // id
                                row[1] = reader.GetString(1); // email
                                row[2] = reader.GetString(2); // username
                                row[3] = reader.GetString(3); // password
                                row[4] = reader.IsDBNull(4) ? "" : reader.GetInt32(4).ToString(); // sessionid
                                row[5] = reader.IsDBNull(5) ? "" : reader.GetString(5); // sessiontoken
                                row[6] = reader.IsDBNull(6) ? null : reader.GetBoolean(6).ToString();
                                rows.Add(row);
                            }
                            return rows.ToArray();
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                Console.WriteLine("SQLStuff: An error occurred in GrabAccountsTable: " + e.Message + "\nSQLStuff: Error Code: " + e.SqliteErrorCode);
                return null;
            }
        }
    }
}
