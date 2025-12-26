using MySql.Data.MySqlClient;

namespace FunWebsiteThing.SQL
{
    public static class Admin
    {
        // Grabs the entire table of accounts and returns an array of all 6 columns
        public static string[]?[]? GrabAccountsTable()
        {
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "SELECT a.*, s.question FROM accounts a LEFT JOIN securityquestion s ON a.id = s.id";
                    using (var cmd = new MySqlCommand(query, con))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            List<string[]> rows = new List<string[]>(); // create a List of string arrays called rows
                            while (reader.Read())
                            {
                                #pragma warning disable CS8601 
                                string[] row = new string[9];
                                row[0] = reader.GetInt32(0).ToString(); // id
                                row[1] = reader.GetString(1); // email
                                row[2] = reader.GetString(2); // username
                                row[3] = reader.GetString(3); // password
                                row[4] = reader.IsDBNull(4) ? "" : reader.GetInt32(4).ToString(); // sessionid
                                row[5] = reader.IsDBNull(5) ? null : reader.GetBoolean(5).ToString(); // is admin?
                                row[6] = reader.IsDBNull(6) ? DateTime.Now.ToString() : reader.GetDateTime(6).ToString(); // join date
                                row[7] = reader.IsDBNull(7) ? "No security question set!" : reader.GetString(7); // security question
        
                                rows.Add(row);
                                #pragma warning restore CS8601
                            }
                            return rows.ToArray(); // convert the List to an array and return it
                        }
                    }
                }
            }
            catch (MySqlException e)
            {
                Logger.Write("SQL.Admin: An error occurred in GrabAccountsTable: " + e.Message + "\nSQL.Admin: Error Code: " + e.ErrorCode, "ERROR");
                return null;
            }
        }

        // checks if user is admin
        public static bool IsAdmin(int? userid)
        {
            if (Accounts.DoesUserExist(userid))
            {
                try
                {
                    using (var con = Main.Connect())
                    {
                        con.Open();
                        string query = "SELECT isadmin FROM accounts WHERE id = @userid";
                        using (var cmd = new MySqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@userid", userid);
                            var result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value) 
                            {
                                return Convert.ToInt32(result) == 1;
                            }
                        }
                    }
                }
                catch (MySqlException e)
                {
                    Logger.Write("SQL.Admin: An error occured in IsAdmin " + e.Message + "\nSQL.Admin: Error Code: " + e.ErrorCode, "ERROR");
                    return false;
                }
            }
            return false;
        }

        // Deletes a user from the accounts table
        public static async Task DeleteUser(int? userid)
        {
            if (Accounts.DoesUserExist(userid) && (userid != -1 && userid != 1))
            {
                try
                {
                    using (var con = Main.Connect())
                    {
                        con.Open();
                        string query = "DELETE FROM accounts WHERE id = @userid";
                        using (var cmd = new MySqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@userid", userid);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                catch (MySqlException e)
                {
                    Logger.Write("SQL.Admin: An error occured in DeleteUser: " + e.Message + "\nSQL.Admin: Error Code: " + e.ErrorCode, "ERROR");
                }
            }
        }

        // Makes user an admin
        public static async Task AdminUser(int? userid)
        {
            if (Accounts.DoesUserExist(userid) && (userid != -1 && userid != 1))
            {
                try
                {
                    using (var con = Main.Connect())
                    {
                        con.Open();
                        string query = "UPDATE accounts SET isadmin = @isadmin WHERE id = @userid";
                        using (var cmd = new MySqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@userid", userid);
                            cmd.Parameters.AddWithValue("@isadmin", IsAdmin(userid) ? 0 : 1);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                catch (MySqlException e)
                {
                    Logger.Write("SQL.Admin: An error occured in AdminUser: " + e.Message + "\nSQL.Admin: Error Code: " + e.ErrorCode, "ERROR");
                }
            }
        }

        // Bans IP from Accessing the Website
        public static async Task BanIP(string ip, string? reason, DateTime? expire)
        {
            if (ip != "" || ip != null)
            {
                if (expire == null)
                    expire = DateTime.Now;
                if (reason == "" || reason == null)
                    reason = "You have been banned.";

                try
                {
                    using (var con = Main.Connect())
                    {
                        con.Open();
                        string query = "INSERT INTO bans (ip, reason, expire) VALUES (@ip, @reason, @expire)";
                        using (var cmd = new MySqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@ip", ip);
                            cmd.Parameters.AddWithValue("@reason", reason);
                            cmd.Parameters.AddWithValue("@expire", expire);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                    Logger.Write("Ban added for IP Address " + ip);
                }
                catch (MySqlException e)
                {
                    Logger.Write("SQL.Admin: An error occured in BanIP: " + e.Message + "\nSQL.Admin: Error Code: " + e.ErrorCode, "ERROR");
                }
            }
        }

        // Checks if the user is IP Banned
        public static (bool, string?, string?, DateTime?) IsUserIPBanned(string ip)
        {
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "SELECT reason, expire FROM bans WHERE ip = @ip";
                    using (var cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ip", ip);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string reason = reader.GetString(0);
                                DateTime expire = reader.GetDateTime(1);
                                return (true, ip, reason, expire);
                            }
                            return (false, null, null, null);
                        }
                    }
                }
            }
            catch (MySqlException e)
            {
                Logger.Write("SQL.Admin: An error occured in IsUserIPBanned " + e.Message + "\nSQL.Admin: Error Code: " + e.ErrorCode, "ERROR");
                return (false, null, null, null);
            }
        }

        // Checks if the user is IP Banned, but as a simple true (yes) or false (no).
        public static bool IsUserIPBannedSimple(string ip)
        {
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "SELECT reason, expire FROM bans WHERE ip = @ip";
                    using (var cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ip", ip);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return true;
                            }
                            return false;
                        }
                    }
                }
            }
            catch (MySqlException e)
            {
                Logger.Write("SQL.Admin: An error occured in IsUserIPBannedS " + e.Message + "\nSQL.Admin: Error Code: " + e.ErrorCode, "ERROR");
                return false;
            }
        }

        // Bans a user by their UserID
        public static async Task BanUser(int? id, string? reason, DateTime? expire)
        {
            if (id != 1 && id != -1 && id != null && SQL.Accounts.DoesUserExist(id))
            {
                if (expire == null)
                    expire = DateTime.Now;
                if (reason == "" || reason == null)
                    reason = "You have been banned.";

                try
                {
                    using (var con = Main.Connect())
                    {
                        con.Open();
                        string query = "UPDATE accountbans SET banned = @banned, reason = @reason, expire = @expire WHERE id = @id";
                        using (var cmd = new MySqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.Parameters.AddWithValue("@banned", true);
                            cmd.Parameters.AddWithValue("@reason", reason);
                            cmd.Parameters.AddWithValue("@expire", expire);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                    Logger.Write("Ban added for Account ID " + id);
                }
                catch (MySqlException e)
                {
                    Logger.Write("SQL.Admin: An error occured in BanUser: " + e.Message + "\nSQL.Admin: Error Code: " + e.ErrorCode, "ERROR");
                }
            }
        }

        // Is a user banned from the website
        public static (bool, int?, string?, DateTime?) IsUserBanned(int? id)
        {
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "SELECT banned, reason, expire FROM accountbans WHERE id = @id";
                    using (var cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bool banned = reader.GetBoolean(0);
                                string reason = reader.GetString(1);
                                DateTime expire = reader.GetDateTime(2);
                                return (banned, id, reason, expire);
                            }
                            return (false, null, null, null);
                        }
                    }
                }
            }
            catch (MySqlException e)
            {
                Logger.Write("SQL.Admin: An error occured in IsUserBanned " + e.Message + "\nSQL.Admin: Error Code: " + e.ErrorCode, "ERROR");
                return (false, null, null, null);
            }
        }

        // Is the user banned from the website, but a simple true (yes) or false (no).
        public static bool IsUserBannedSimple(int id)
        {
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "SELECT banned, reason, expire FROM accountbans WHERE id = @id";
                    using (var cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bool banned = reader.GetBoolean(0);
                                return banned;
                            }
                            return false;
                        }
                    }
                }
            }
            catch (MySqlException e)
            {
                Logger.Write("SQL.Admin: An error occured in IsUserBannedS " + e.Message + "\nSQL.Admin: Error Code: " + e.ErrorCode, "ERROR");
                return false;
            }
        }

        // Unbans a user by UserID
        public static async Task UnbanUser(int? id)
        {
            (bool b, int? i, string? r, DateTime? ex) = IsUserBanned(id);
            if (b == true)
            {
                try
                {
                    using (var con = Main.Connect())
                    {
                        con.Open();
                        string query = "UPDATE accountbans SET expire = @expire, banned = @banned WHERE id = @id";
                        using (var cmd = new MySqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.Parameters.AddWithValue("@banned", false);
                            cmd.Parameters.AddWithValue("@expire", DateTime.Now);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                    Logger.Write("Unbanned Account ID " + id);
                }
                catch (MySqlException e)
                {
                    Logger.Write("SQL.Admin: An error occured in UnbanUser: " + e.Message + "\nSQL.Admin: Error Code: " + e.ErrorCode, "ERROR");
                }
            }
        }

        // Unbans an IP based on IP address
        public static async Task UnbanIP(string ip)
        {
            (bool b, string? i, string? r, DateTime? ex) = IsUserIPBanned(ip);
            if (b == true)
            {
                try
                {
                    using (var con = Main.Connect())
                    {
                        con.Open();
                        string query = "UPDATE bans SET expire = @expire WHERE ip = @ip";
                        using (var cmd = new MySqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@ip", ip);
                            cmd.Parameters.AddWithValue("@expire", DateTime.Now);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                    Logger.Write("Ban removed for IP Address " + ip);
                }
                catch (MySqlException e)
                {
                    Logger.Write("SQL.Admin: An error occured in UnbanIP: " + e.Message + "\nSQL.Admin: Error Code: " + e.ErrorCode, "ERROR");
                }
            }
        }
    }
}
