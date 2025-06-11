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
            Accounts.Init(); // accounts and securityquestion tables
            Blog.Init(); // blog table
            Comments.Init(); // comments table
            Files.Init(); // files and filesdescription tables
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
