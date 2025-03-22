﻿using MySql.Data.MySqlClient;

namespace FunWebsiteThing.SQL
{
    public class Main
    {
        public static string ConnectionString = "";
        // Connects to our database
        public static MySqlConnection Connect()
        {
            string connect = ConnectionString;
            return new MySqlConnection(connect);
        }
        // Creates database file and tables if they don't exist
        public static void Init()
        {
            string cs = Settings.GetSettings()[0];
            SetConnectionString(cs);
            Blog.Init();
            Comments.Init();
            Accounts.Init();
            Files.Init();
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
