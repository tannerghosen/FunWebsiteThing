using MySql.Data.MySqlClient;

namespace FunWebsiteThing.SQL
{
    public static class Main
    {
        public static string ConnectionString = "";
        // Connects to our database
        public static MySqlConnection Connect()
        {
            string connect = ConnectionString;
            return new MySqlConnection(connect);
        }
        // Creates database and tables if they don't exist
        public static void Init(string sqlconstr)
        {
            SetConnectionString(sqlconstr);
            if (TryConnectionString() == true)
            {
                Tables.Accounts(); // accounts table
                Tables.SecurityQuestion(); // securityquestion tables
                Tables.AccountBans(); // account bans table
                Triggers.Accounts(); // accounts trigger
                Triggers.UnbanOnExpire(); // unban on expire event
                Misc.CreateDefaultAccounts(); // create accounts
                Tables.Blog(); // blog table
                Tables.Comments(); // comments table
                Tables.Stats(); // stats table
                Tables.Bans(); // bans table
                Triggers.UnbanOnExpireIP(); // unban on expire IP event
            }
            else
            {
                Logger.Write("SQL: MySQL Error occured that isn't recoverable from. Possible causes include:\n* Invalid / Incorrect connection string in environment variables.\n* MySQL is not running.\n* You're connecting to a Remote MySQL server that's not accessible for various reasons (unreachable, not running, no internet locally to connect to it, etc.).", "CRITICAL ERROR!");
                Environment.Exit(0);
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
        public static bool TryConnectionString()
        {
            try
            {
                using (var con = Connect())
                {
                    con.Open();
                    con.Close();
                    return true;
                }
            }
            catch (MySqlException e)
            {
                Logger.Write("SQL: Something is wrong with MySQL.\nError provided: " + e.Message + "\nSQL: Error Code: " + e.ErrorCode, "ERROR");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Write("SQL: Something is wrong with MySQL that caused a regular exception.\nError provided: " + ex.Message);
                return false;
            }
        }
    }
}
