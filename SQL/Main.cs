using Microsoft.Data.Sqlite;

namespace FunWebsiteThing.SQL
{
    public class Main
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

        public static string GetConnectionString()
        {
            return ConnectionString;
        }
    }
}
