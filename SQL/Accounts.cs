﻿using Microsoft.Data.Sqlite;

namespace FunWebsiteThing.SQL
{
    public class Accounts
    {
        public static void Init()
        {
            using (var con = Main.Connect())
            {
                con.Open();

                // Accounts Table
                string accounts = "CREATE TABLE IF NOT EXISTS accounts (id INTEGER PRIMARY KEY AUTOINCREMENT, email TEXT NOT NULL UNIQUE, username TEXT NOT NULL UNIQUE, password TEXT NOT NULL, sessionid INTEGER, sessiontoken TEXT, isadmin BOOLEAN DEFAULT 0)";
                using (var cmd = new SqliteCommand(accounts, con))
                {
                    cmd.ExecuteNonQuery();
                }

                // Security Questions Table
                string securityquestion = "CREATE TABLE IF NOT EXISTS securityquestion (id INTEGER PRIMARY KEY, question TEXT NOT NULL, answer TEXT NOT NULL, FOREIGN KEY (id) REFERENCES accounts(id))";
                using (var cmd = new SqliteCommand(securityquestion, con))
                {
                    cmd.ExecuteNonQuery();
                }

                // Delete Account Trigger
                string deleteaccounttrigger = "CREATE TRIGGER IF NOT EXISTS deleteaccounttrigger AFTER DELETE ON accounts FOR EACH ROW BEGIN DELETE FROM securityquestion WHERE id = OLD.id; UPDATE comments SET userid = -1 WHERE userid = OLD.id; END";
                using (var cmd = new SqliteCommand(deleteaccounttrigger, con))
                {
                    cmd.ExecuteNonQuery();
                }

                // Admin Account
                string doesadminexist = "SELECT COUNT(*) FROM accounts WHERE id = 0";
                using (var c = new SqliteCommand(doesadminexist, con))
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
                string doesanonymousexist = "SELECT COUNT(*) FROM accounts WHERE id = -1";
                using (var c = new SqliteCommand(doesanonymousexist, con))
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
            }
        }
        // Registers an account by first running a SQL statement to see if it the account exists. If it does, don't do anything.
        // If it doesn't, run another SQL statement that inserts it into the table, alongside generating a salt to hash our password.
        // (first bool is did operation succeed, second bool is did an error occur. the first bool will never be true if the second one is true.)
        public static async Task<(bool, bool)> Register(string email, string username, string password, int sessionid = 0, string stoken = "")
        {
            try
            {
                using (var con = Main.Connect())
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
                Logger.Write("SQL.Accounts: An error occured in Register: " + e.Message + "\nSQL.Accounts: Error Code: " + e.SqliteErrorCode, "ERROR");
                return (false, true);
            }
        }

        // Logs us into an account by running a SQL statement to see if the username is valid first. If it isn't, return false.
        // If it is, then we run another SQL statement that compares the hashed password with the password given using BCrypt.Verify
        // If it matches, we return true so Login.cshtml.cs can handle setting the session up. If not, we return false.
        // (first bool is did operation succeed, second bool is did an error occur. the first bool will never be true if the second one is true.)
        public static async Task<(bool, bool)> Login(string username, string password, int sessionid = 0, string stoken = "")
        {
            if (username == "Anonymous") // Let's not allow people to use Anonymous as a username to login
            {
                return (false, false);
            }
            try
            {
                using (var con = Main.Connect())
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
            catch (SqliteException e)
            {
                Logger.Write("SQL.Accounts: An error occured in Login: " + e.Message + "\nSQL.Accounts: Error Code: " + e.SqliteErrorCode, "ERROR");
                return (false, true);
            }
        }

        // Updates various settings of a specified user (by username)'s account.
        // (first bool is did operation succeed, second bool is did an error occur. the first bool will never be true if the second one is true.)
        public static async Task<(bool, bool)> UpdateInfo(int? userid, int option, string input, int? sessionid = 0, bool adminupdate = false)
        {
            if ((DoesSIDMatch(userid, sessionid) || adminupdate == true) && userid != -1)
            {
                try
                {
                    using (var con = Main.Connect())
                    {
                        con.Open();
                        string query = "SELECT COUNT(*) FROM accounts WHERE id = @id";
                        using (var cmd = new SqliteCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@id", userid);
                            int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                            if (count == 0)
                            {
                                return (false, false);
                            }
                        }
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
                    Logger.Write("SQL.Accounts: An error occured in UpdateInfo: " + e.Message + "\nSQL.Accounts: Error Code: " + e.SqliteErrorCode, "ERROR");
                    return (false, true); // error happened due to sql issue, so yea let's say an error occured
                }
            }
            else
            {
                return (false, true);
            }
        }

        // Used to ensure the user does actually exist before we get too far in with various methods
        public static bool DoesUserExist(string username)
        {
            try
            {
                using (var con = Main.Connect())
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
                Logger.Write("SQL.Accounts: An error occured in DoesUserExist (string username parameter variant): " + e.Message + "\nSQL.Accounts: Error Code: " + e.SqliteErrorCode, "ERROR");
                return false;
            }
        }

        // Same as above but userid variant, in case this is more preferable in the future
        public static bool DoesUserExist(int? userid)
        {
            try
            {
                using (var con = Main.Connect())
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
                Logger.Write("SQL.Accounts: An error occured in DoesUserExist (int? userid parameter variant): " + e.Message + "\nSQL.Accounts: Error Code: " + e.SqliteErrorCode, "ERROR");
                return false;
            }
        }

        // Get User ID by Username
        public static int GetUserID(string username)
        {
            try
            {
                using (var con = Main.Connect())
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
                Logger.Write("SQL.Accounts: An error occured in GetUserID: " + e.Message + "\nSQL.Accounts: Error Code: " + e.SqliteErrorCode, "ERROR");
                return -1;
            }
        }

        // Get Username by UserID
        public static string? GetUsername(int userid)
        {
            try
            {
                using (var con = Main.Connect())
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
                Logger.Write("SQL.Accounts: An error occured in GetUserID: " + e.Message + "\nSQL.Accounts: Error Code: " + e.SqliteErrorCode, "ERROR");
                return null;
            }
        }

        //  Get Email by Username
        public static string? GetUsername(string email)
        {
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "SELECT username FROM accounts WHERE email = @email";
                    using (var cmd = new SqliteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        return cmd.ExecuteScalar()?.ToString();
                    }
                }
            }
            catch (SqliteException e)
            {
                Logger.Write("SQL.Accounts: An error occured in GetEmail: " + e.Message + "\nSQL.Accounts: Error Code: " + e.SqliteErrorCode, "ERROR");
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
                    using (var con = Main.Connect())
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
                    Logger.Write("SQL.Accounts: An error occured in DoesSIDMatch: " + e.Message + "\nSQL.Accounts: Error Code: " + e.SqliteErrorCode, "ERROR");
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
                    using (var con = Main.Connect())
                    {
                        con.Open();
                        string query = "SELECT sessiontoken FROM accounts WHERE id = @userid";
                        using (var cmd = new SqliteCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@userid", userid);
                            var token = Convert.ToString(cmd.ExecuteScalar());
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
                    Logger.Write("SQL.Accounts: An error occured in DoesSTokenMatch: " + e.Message + "\nSQL.Accounts: Error Code: " + e.SqliteErrorCode, "ERROR");
                    return false;
                }
            }
            return false;
        }
        public static (string?, string?) GetSecurityQuestion(int? userid)
        {
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "SELECT question, answer FROM securityquestion WHERE id = @id";
                    using (var cmd = new SqliteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", userid);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string question = reader.GetString(0); 
                                string answer = reader.GetString(1);
                                return (question, answer);
                            }
                        }
                    }
                }
            }
            catch (SqliteException e)
            {
                Logger.Write("SQL.Accounts: An error occured in SecurityQuestion: " + e.Message + "\nSQL.Accounts: Error Code: " + e.SqliteErrorCode, "ERROR");
                return (null, null);
            }
            return (null, null);
        }
        public static async Task<(bool, bool)> CreateSecurityQuestion(int? userid, string? question, string? answer)
        {
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string prequery = "SELECT COUNT(*) FROM securityquestion WHERE id = @id";
                    using (var cmd = new SqliteCommand(prequery, con))
                    {
                        cmd.Parameters.AddWithValue("@id", userid);
                        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        if (count > 0)
                        {
                            return (false, false);
                        }
                    }
                    string query = "INSERT INTO securityquestion (id, question, answer) VALUES (@id, @q, @a)";
                    using (var cmd = new SqliteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", userid);
                        cmd.Parameters.AddWithValue("@q", question);
                        cmd.Parameters.AddWithValue("@a", answer);
                        await cmd.ExecuteNonQueryAsync();
                        return (true, false);
                    }
                }
            }
            catch (SqliteException e)
            {
                Logger.Write("SQL.Accounts: An error occured in CreateSecurityQuestion: " + e.Message + "\nSQL.Accounts: Error Code: " + e.SqliteErrorCode, "ERROR");
                return (false, true);
            }
        }
        public static async Task<(bool, bool)> UpdateSecurityQuestion(int? userid, string? question = null, string? answer = null)
        {
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string prequery = "SELECT COUNT(*) FROM securityquestion WHERE id = @id";
                    using (var cmd = new SqliteCommand(prequery, con))
                    {
                        cmd.Parameters.AddWithValue("@id", userid);
                        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        if (count == 0)
                        {
                            return (false, false);
                        }
                    }
                    (string? q, string? a) = GetSecurityQuestion(userid);
                    question = question == null ? q : question;
                    answer = answer == null ? a : answer;
                    string query = "UPDATE securityquestion SET question = @q, answer = @a WHERE id = @id";
                    using (var cmd = new SqliteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", userid);
                        cmd.Parameters.AddWithValue("@q", question);
                        cmd.Parameters.AddWithValue("@a", answer);
                        await cmd.ExecuteNonQueryAsync();
                        return (true, false);
                    }
                }
            }
            catch (SqliteException e)
            {
                Logger.Write("SQL.Accounts: An error occured in CreateSecurityQuestion: " + e.Message + "\nSQL.Accounts: Error Code: " + e.SqliteErrorCode, "ERROR");
                return (false, true);
            }
        }
    }
}
