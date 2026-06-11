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
                // Create Tables, Triggers and Default Accounts
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
                //Indexes.AccountID();
                //Indexes.AccountUsername();
                //Indexes.AccountEmail();
                //Indexes.BlogPostID();
                //Indexes.CommentSID();
            }
            else
            {
                // Fatal Error happened, like in Program.cs we stop the program altogether here.
                Logger.Write("Fatal error with MySQL. Ending program.", "FATAL");
                Console.WriteLine("Fatal error with MySQL. Ending program.");
                Console.ReadKey();

                Environment.Exit(0);
            }
        }

        // Sets connection string
        public static void SetConnectionString(string cs)
        {
            ConnectionString = cs;
        }

        // Gets connection string
        public static string GetConnectionString()
        {
            return ConnectionString;
        }
        
        // Tests connection string
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
                Console.WriteLine("SQL: Something is wrong with MySQL.\nError provided: " + e.Message + "\nSQL: Error Code: " + e.ErrorCode);
                Logger.Write("SQL: Something is wrong with MySQL.\nError provided: " + e.Message + "\nSQL: Error Code: " + e.ErrorCode, "ERROR");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("SQL: Something is wrong with MySQL that caused a regular exception.\nError provided: " + ex.Message);
                Logger.Write("SQL: Something is wrong with MySQL that caused a regular exception.\nError provided: " + ex.Message);
                return false;
            }
        }
    }
}
