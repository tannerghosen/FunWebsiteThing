using MySqlConnector;
using System.Text.RegularExpressions;
using TannersWebsiteTemplate.Helpers;
using TannersWebsiteTemplate.Interfaces.SQL;

namespace TannersWebsiteTemplate.SQL
{
    public class Accounts : IAccounts
    {
        private static Regex EmailRegex = Regexes.GetEmailRegex();
        private static Regex UsernameRegex = Regexes.GetUsernameRegex();
        // Registers an account by first running a SQL statement to see if it the account exists. If it does, don't do anything.
        // If it doesn't, run another SQL statement that inserts it into the table, alongside generating a salt to hash our password.
        // (first bool is did operation succeed, second bool is did an error occur. the first bool will never be true if the second one is true.)
        public static async Task<(bool, bool)> Register(string email, string username, string password, string sessionid = "", bool external = false)
        {
            // Ensure it meets regex before we even consider registering
            if (!EmailRegex.IsMatch(email) || !UsernameRegex.IsMatch(username))
                return (false, false);

            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "SELECT COUNT(*) FROM accounts WHERE email = @email OR username = @username";
                    using (var cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@email", email);
                        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        if (count > 0)
                        {
                            return (false, false);
                        }
                    }
                    query = "INSERT INTO accounts (email, username, password, sessionid, isexternal) VALUES (@email, @username, @password, @sid, @external)";
                    using (var cmd = new MySqlCommand(query, con))
                    {
                        string salt = BCrypt.Net.BCrypt.GenerateSalt(12);
                        string hashpass = BCrypt.Net.BCrypt.HashPassword(password, salt);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", hashpass);
                        cmd.Parameters.AddWithValue("@sid", sessionid);
                        cmd.Parameters.AddWithValue("@external", external);
                        await cmd.ExecuteNonQueryAsync();
                        return (true, false);
                    }
                }
            }
            catch (MySqlException e)
            {
                await Logger.Write("SQL.Accounts: An error occured in Register: " + e.Message + "\nSQL.Accounts: Error Code: " + e.ErrorCode, "ERROR");
                return (false, true);
            }
        }

        // Logs us into an account by running a SQL statement to see if the username is valid first. If it isn't, return false.
        // If it is, then we run another SQL statement that compares the hashed password with the password given using BCrypt.Verify
        // If it matches, we return true so Login.cshtml.cs can handle setting the session up. If not, we return false.
        // (first bool is did operation succeed, second bool is did an error occur. the first bool will never be true if the second one is true.)
        public static async Task<(bool, bool)> Login(string username, string password, string sessionid = "")
        {
            if (username == await GetUsername(-1))
            {
                return (false, false);
            }
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "SELECT COUNT(*) FROM accounts WHERE username = @username";
                    using (var cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        if (count == 0)
                        {
                            return (false, false);
                        }
                    }
                    // verify the password
                    query = "SELECT password FROM accounts WHERE username = @username";
                    using (var cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        var res = await cmd.ExecuteScalarAsync();
                        string hashedpassword = res.ToString();
                        // if password doesn't match password in database
                        if (!BCrypt.Net.BCrypt.Verify(password, hashedpassword))
                        {
                            // reject login
                            return (false, false);
                        }
                        else
                        {
                            // continue login process
                            // updating session id if required
                            query = "SELECT sessionid FROM accounts WHERE username = @username";
                            using (var c = new MySqlCommand(query, con))
                            {
                                c.Parameters.AddWithValue("@username", username);
                                var result = await c.ExecuteScalarAsync();
                                string id = (result != null && result != DBNull.Value) ? result.ToString() : ""; // Default the id to -1 if it's null or DBNull
                                if (sessionid != id || id == "")
                                {
                                    query = "UPDATE accounts SET sessionid = @sid WHERE username = @username";
                                    using (var cm = new MySqlCommand(query, con))
                                    {
                                        cm.Parameters.AddWithValue("@sid", sessionid);
                                        cm.Parameters.AddWithValue("@username", username);
                                        await cm.ExecuteNonQueryAsync();
                                    }
                                }
                            }
                            // login
                            return (true, false);
                        }
                    }
                }
            }
            catch (MySqlException e)
            {
                await Logger.Write("SQL.Accounts: An error occured in Login: " + e.Message + "\nSQL.Accounts: Error Code: " + e.ErrorCode, "ERROR");
                return (false, true);
            }
        }

        // Updates various settings of a specified user (by username)'s account.
        // (first bool is did operation succeed, second bool is did an error occur. the first bool will never be true if the second one is true.)
        public static async Task<(bool, bool)> UpdateInfo(int? userid, int option, string input, string? sessionid = "", bool adminupdate = false)
        {
            if ((await DoesSIDMatch(userid, sessionid) || adminupdate == true) && userid != -1)
            {
                try
                {
                    using (var con = Main.Connect())
                    {
                        con.Open();
                        string query = "SELECT COUNT(*) FROM accounts WHERE id = @id";
                        using (var cmd = new MySqlCommand(query, con))
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
                                using (var c = new MySqlCommand(updatepassword, con))
                                {
                                    string pass = BCrypt.Net.BCrypt.HashPassword(input);
                                    c.Parameters.AddWithValue("@id", userid);
                                    c.Parameters.AddWithValue("@password", pass);
                                    await c.ExecuteNonQueryAsync();
                                }
                                return (true, false);
                            case 1: // email
                                if (EmailRegex.IsMatch(input))
                                {
                                    string updateemail = "UPDATE accounts SET email = @email WHERE id = @id";
                                    using (var c = new MySqlCommand(updateemail, con))
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
                                }
                                return (false, false);
                            case 2: // username
                                if (UsernameRegex.IsMatch(input))
                                {
                                    string updateusername = "UPDATE accounts SET username = @newusername WHERE id = @id";
                                    using (var c = new MySqlCommand(updateusername, con))
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
                                }
                                return (false, false);
                            default:
                                return (false, false);
                        }
                    }
                }
                catch (MySqlException e)
                {
                    await Logger.Write("SQL.Accounts: An error occured in UpdateInfo: " + e.Message + "\nSQL.Accounts: Error Code: " + e.ErrorCode, "ERROR");
                    return (false, true); 
                }
            }
            else
            {
                return (false, false);
            }
        }

        // Used to ensure the user does actually exist before we get too far in with various methods
        public static async Task<bool> DoesUserExist(string value, string search = "username")
        {
            try
            {
                using (var con = Main.Connect())
                {
                    if (search == "username")
                    {
                        con.Open();
                        string query = "SELECT COUNT(*) FROM accounts WHERE username = @username";
                        using (var cmd = new MySqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@username", value);
                            int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
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
                    else
                    {
                        con.Open();
                        string query = "SELECT COUNT(*) FROM accounts WHERE email = @email";
                        using (var cmd = new MySqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@email", value);
                            int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
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
            }
            catch (MySqlException e)
            {
                await Logger.Write("SQL.Accounts: An error occured in DoesUserExist (string value, string search parameters variant): " + e.Message + "\nSQL.Accounts: Error Code: " + e.ErrorCode, "ERROR");
                return false;
            }
        }

        // Same as above but userid variant, in case this is more preferable in the future
        public static async Task<bool> DoesUserExist(int? userid)
        {
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "SELECT COUNT(*) FROM accounts WHERE id = @userid";
                    using (var cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@userid", userid);
                        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
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
            catch (MySqlException e)
            {
                await Logger.Write("SQL.Accounts: An error occured in DoesUserExist (int? userid parameter variant): " + e.Message + "\nSQL.Accounts: Error Code: " + e.ErrorCode, "ERROR");
                return false;
            }
        }

        // Get User ID by Username
        public static async Task<int> GetUserID(string username)
        {
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "SELECT id FROM accounts WHERE username = @username";
                    using (var cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        int id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        return id;
                    }
                }
            }
            catch (MySqlException e)
            {
                await Logger.Write("SQL.Accounts: An error occured in GetUserID: " + e.Message + "\nSQL.Accounts: Error Code: " + e.ErrorCode, "ERROR");
                return 0;
            }
        }

        // Get Username by UserID
        public static async Task<string?> GetUsername(int userid)
        {
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "SELECT username FROM accounts WHERE id = @userid";
                    using (var cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@userid", userid);
                        var result = await cmd.ExecuteScalarAsync();
                        if (result == null || result == DBNull.Value) result = "";
                        return result.ToString();
                    }
                }
            }
            catch (MySqlException e)
            {
                await Logger.Write("SQL.Accounts: An error occured in GetUsername (int userid variant): " + e.Message + "\nSQL.Accounts: Error Code: " + e.ErrorCode, "ERROR");
                return null;
            }
        }

        //  Get Username by Email
        public static async Task<string?> GetUsername(string email)
        {
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "SELECT username FROM accounts WHERE email = @email";
                    using (var cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        var result = await cmd.ExecuteScalarAsync();
                        if (result == null || result == DBNull.Value) result = "";
                        return result.ToString();
                    }
                }
            }
            catch (MySqlException e)
            {
                await Logger.Write("SQL.Accounts: An error occured in GetUsername (string email variant): " + e.Message + "\nSQL.Accounts: Error Code: " + e.ErrorCode, "ERROR");
                return null;
            }
        }

        // Get Join Date by UserID
        public static async Task<DateTime?> GetJoinDate(int userid)
        {
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "SELECT joined FROM accounts WHERE id = @userid";
                    using (var cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@userid", userid);
                        var result = await cmd.ExecuteScalarAsync();
                        return (DateTime?)result;
                    }
                }
            }
            catch (MySqlException e)
            {
                await Logger.Write("SQL.Accounts: An error occured in GetJoinDate: " + e.Message + "\nSQL.Accounts: Error Code: " + e.ErrorCode, "ERROR");
                return null;
            }
        }

        // Does Session ID Match
        public static async Task<bool> DoesSIDMatch(int? userid, string? sid)
        {
            bool usercheck = await DoesUserExist(userid);
            if (usercheck)
            {
                try
                {
                    using (var con = Main.Connect())
                    {
                        con.Open();
                        string query = "SELECT sessionid FROM accounts WHERE id = @userid";
                        using (var cmd = new MySqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@userid", userid);
                            var result = await cmd.ExecuteScalarAsync();
                            string id = result.ToString();
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
                catch (MySqlException e)
                {
                    await Logger.Write("SQL.Accounts: An error occured in DoesSIDMatch: " + e.Message + "\nSQL.Accounts: Error Code: " + e.ErrorCode, "ERROR");
                    return false;
                }
            }
            return false;
        }

        // Is the User an External Source one?
        public static async Task<bool> IsExternal(int? userid)
        {
            if (await Accounts.DoesUserExist(userid))
            {
                try
                {
                    using (var con = Main.Connect())
                    {
                        con.Open();
                        string query = "SELECT isexternal FROM accounts WHERE id = @userid";
                        using (var cmd = new MySqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@userid", userid);
                            var result = await cmd.ExecuteScalarAsync();
                            if (result != null && result != DBNull.Value)
                            {
                                return Convert.ToInt32(result) == 1;
                            }
                        }
                    }
                }
                catch (MySqlException e)
                {
                    await Logger.Write("SQL.Accounts: An error occured in IsExternal " + e.Message + "\nSQL.Accounts: Error Code: " + e.ErrorCode, "ERROR");
                    return false;
                }
            }
            return false;
        }
    }
}
