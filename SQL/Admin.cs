using Microsoft.Data.Sqlite;

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
            bool usercheck = Accounts.DoesUserExist(userid);
            if (usercheck)
            {
                try
                {
                    using (var con = Main.Connect())
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
            bool usercheck = Accounts.DoesUserExist(userid);
            if (usercheck && (userid != -1 && userid != 0))
            {
                try
                {
                    using (var con = Main.Connect())
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
            bool usercheck = Accounts.DoesUserExist(userid);
            if (usercheck && (userid != -1 && userid != 0))
            {
                try
                {
                    using (var con = Main.Connect())
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
    }
}
