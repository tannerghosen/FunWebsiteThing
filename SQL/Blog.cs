using Microsoft.Data.Sqlite;
using System.Xml.Linq;

namespace FunWebsiteThing.SQL
{
    public class Blog
    {

        public static void Init()
        {
            using (var con = Main.Connect())
            {
                con.Open();

                string blog = "CREATE TABLE IF NOT EXISTS blog (id INTEGER PRIMARY KEY AUTOINCREMENT, title TEXT NOT NULL, message TEXT NOT NULL, date DATETIME DEFAULT CURRENT_TIMESTAMP)";
                using (var cmd = new SqliteCommand(blog, con))
                {
                    cmd.ExecuteNonQuery();
                }

                con.Close();
            }
        }
        public static async Task AddBlogPost(string title, string body)
        {
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "INSERT INTO blog (title, message) VALUES (@title, @body)";
                    using (var cmd = new SqliteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@title", title);
                        cmd.Parameters.AddWithValue("@body", body);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                Logger.Write("Added blog post with title " + title);
            }
            catch (SqliteException e)
            {
                Logger.Write("SQL.Blog: An error occured in AddBlogPost: " + e.Message + "\nSQL.Blog: Error Code: " + e.SqliteErrorCode, "ERROR");
            }
        }

        public static async Task UpdateBlogPost(string title, string body, int? blogid)
        {
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "UPDATE blog SET title = @title, body = @body WHERE id = @id";
                    using (var cmd = new SqliteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@title", title);
                        cmd.Parameters.AddWithValue("@body", body);
                        cmd.Parameters.AddWithValue("@id", blogid);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                Logger.Write("Updated blog post " + blogid);
            }
            catch (SqliteException e)
            {
                Logger.Write("SQL.Blog: An error occured in UpdateBlogPost: " + e.Message + "\nSQL.Blog: Error Code: " + e.SqliteErrorCode, "ERROR");
            }
        }

        public static async Task DeleteBlogPost(int? blogid)
        {
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "DELETE FROM blog WHERE id = @id";
                    using (var cmd = new SqliteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", blogid);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                Logger.Write("Deleted blog post " + blogid);
            }
            catch (SqliteException e)
            {
                Logger.Write("SQL.Blog: An error occured in DeleteBlogPost: " + e.Message + "\nSQL.Blog: Error Code: " + e.SqliteErrorCode, "ERROR");
            }
        }

        public static (string?, string?) GetBlogPost(int? blogid)
        {
            string title = "", body = "";
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "SELECT (title, body) FROM blog WHERE id = @id";
                    using (var cmd = new SqliteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", blogid);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader != null)
                            {
                                while (reader.Read())
                                {
                                    title = reader.GetString(0);
                                    body = reader.GetString(1);
                                }
                            }
                        }
                        return (title, body);
                    }
                }
            }
            catch (SqliteException e)
            {
                Logger.Write("SQL.Blog: An error occured in GetBlogPost: " + e.Message + "\nSQL.Blog: Error Code: " + e.SqliteErrorCode, "ERROR");
                return (null, null);
            }
        }
    }
}
