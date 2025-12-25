using MySql.Data.MySqlClient;

namespace FunWebsiteThing.SQL
{
    public static class Stats
    {
        // Updates various stat entries based on which stat needs to be incremented
        public static async Task UpdateStat(string stat)
        {
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "";
                    if (stat == "logins" || stat == "Logins")
                    {
                        query = "UPDATE stats SET count = count + 1 WHERE stat = 'logins'";
                    }
                    else if (stat == "registrations" || stat == "Registrations")
                    {
                        query = "UPDATE stats SET count = count + 1 WHERE stat = 'registrations'";
                    }
                    else if (stat == "errors" || stat == "Errors")
                    {
                        query = "UPDATE stats SET count = count + 1 WHERE stat = 'errors'";
                    }
                    using (var cmd = new MySqlCommand(query, con))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (MySqlException e)
            {
                Logger.Write("SQL.Stats: An error occured in UpdateStat: " + e.Message + "\nSQL.Stats: Error Code: " + e.ErrorCode, "ERROR");
            }
        }

        // Resets all stat entries back to 0
        public static async Task ResetStats()
        {
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "UPDATE stats SET count = 0 WHERE stat = 'logins'";
                    string query2 = "UPDATE stats SET count = 0 WHERE stat = 'registrations'";
                    string query3 = "UPDATE stats SET count = 0 WHERE stat = 'errors'";

                    using (var cmd = new MySqlCommand(query, con))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }

                    using (var cmd = new MySqlCommand(query2, con))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }

                    using (var cmd = new MySqlCommand(query3, con))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (MySqlException e)
            {
                Logger.Write("SQL.Stats: An error occured in ResetStats: " + e.Message + "\nSQL.Stats: Error Code: " + e.ErrorCode, "ERROR");
            }
        }

        // Get the stats table as an int array
        public static async Task<int[]> GetStats()
        {
            int logins = 0, registrations = 0, errors = 0;
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string logquery = "SELECT count FROM stats WHERE stat = 'logins'";
                    using (var cmd = new MySqlCommand(logquery, con))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                logins = reader.GetInt32(0);
                            }
                            else
                            {
                                logins = 0;
                            }
                        }
                    }
                    string regquery = "SELECT count FROM stats WHERE stat = 'registrations'";
                    using (var cmd = new MySqlCommand(regquery, con))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                registrations = reader.GetInt32(0);
                            }
                            else
                            {
                                registrations = 0;
                            }
                        }
                    }
                    string errquery = "SELECT count FROM stats WHERE stat = 'errors'";
                    using (var cmd = new MySqlCommand(errquery, con))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                errors = reader.GetInt32(0);
                            }
                            else
                            {
                                errors = 0;
                            }
                        }
                    }
                    return new int[] { logins, registrations, errors };
                }
            }
            catch (MySqlException e)
            {
                Logger.Write("SQL.Stats: An error occured in GetStats " + e.Message + "\nSQL.Stats: Error Code: " + e.ErrorCode, "ERROR");
                return new int[] { 0, 0, 0 };
            }
        }
    }
}
