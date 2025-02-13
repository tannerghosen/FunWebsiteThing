using MySql.Data.MySqlClient;

namespace FunWebsiteThing.SQL
{
    public class Admin
    {
        // Grabs the entire table of accounts and returns an array of all 6 columns
        public static string[]?[]? GrabAccountsTable()
        {
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "SELECT a.*, s.question, (SELECT COUNT(*) FROM comments WHERE userid = a.id) AS CommentCount FROM accounts a LEFT JOIN securityquestion s ON a.id = s.id";
                    using (var cmd = new MySqlCommand(query, con))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            List<string[]> rows = new List<string[]>(); // create a List of string arrays called rows
                            while (reader.Read())
                            {
                                #pragma warning disable CS8601 // Nullability of reference types in value doesn't match target type.
                                string[] row = new string[9]; // create a string array called row where all 7 columns are stored in
                                row[0] = reader.GetInt32(0).ToString(); // id
                                row[1] = reader.GetString(1); // email
                                row[2] = reader.GetString(2); // username
                                row[3] = reader.GetString(3); // password
                                row[4] = reader.IsDBNull(4) ? "" : reader.GetInt32(4).ToString(); // sessionid
                                row[5] = reader.IsDBNull(5) ? "" : reader.GetString(5); // sessiontoken
                                row[6] = reader.IsDBNull(6) ? null : reader.GetBoolean(6).ToString(); // is admin?
                                row[7] = reader.IsDBNull(7) ? "No security question set!" : reader.GetString(7); // security question
                                //row[8] = reader.IsDBNull(8) ? "0" : reader.GetInt32(8).ToString(); // comments count
                                rows.Add(row); // add row to the rows List
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
            bool usercheck = Accounts.DoesUserExist(userid);
            if (usercheck)
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
                            if (result != null && result != DBNull.Value) // if result isn't null or result isn't dbnull 
                            {
                                // isadmin is stored as a boolean (which defaults to 0 (false) or 1 (true) regardless of how inserted or stored)
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
            bool usercheck = Accounts.DoesUserExist(userid);
            if (usercheck && (userid != -1 && userid != 0))
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
            bool usercheck = Accounts.DoesUserExist(userid);
            if (usercheck && (userid != -1 && userid != 0))
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
    }
}
