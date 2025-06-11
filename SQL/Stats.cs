using MySql.Data.MySqlClient;
using System.Globalization;

namespace FunWebsiteThing.SQL
{
    public static class Stats
    {
        public static void Init()
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
                con.Close();
            }
        }

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

        public static async Task ResetStats()
        {
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "UPDATE stats SET count = 0 WHERE stat = 'logins'";
                    string query2 = "UPDATE stats SET count = 0 WHERE stat = 'registrations'";

                    using (var cmd = new MySqlCommand(query, con))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }

                    using (var cmd = new MySqlCommand(query2, con))
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

        public static async Task<int[]> GetStats()
        {
            int logins = 0, registrations = 0;
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "SELECT count FROM stats WHERE stat = 'logins'";
                    using (var cmd = new MySqlCommand(query, con))
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
                    string query2 = "SELECT count FROM stats WHERE stat = 'registrations'";
                    using (var cmd = new MySqlCommand(query, con))
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
                    return new int[] { logins, registrations };
                }
            }
            catch (MySqlException e)
            {
                Logger.Write("SQL.Stats: An error occured in GetStats " + e.Message + "\nSQL.Stats: Error Code: " + e.ErrorCode, "ERROR");
                return new int[] { 0, 0 };
            }
        }
    }
}
