using MySql.Data.MySqlClient;

namespace FunWebsiteThing.SQL
{
    public class Blog
    {

        public static void Init()
        {
            using (var con = Main.Connect())
            {
                con.Open();

                string blog = "CREATE TABLE IF NOT EXISTS blog (id INT(11) PRIMARY KEY AUTO_INCREMENT, title VARCHAR(255) NOT NULL, message VARCHAR(255) NOT NULL, date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP)";
                using (var cmd = new MySqlCommand(blog, con))
                {
                    cmd.ExecuteNonQuery();
                }

                con.Close();
            }
        }
        public static async Task AddBlogPost(string title, string message)
        {
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "INSERT INTO blog (title, message) VALUES (@title, @message)";
                    using (var cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@title", title);
                        cmd.Parameters.AddWithValue("@message", message);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                Logger.Write("Added blog post with title " + title);
            }
            catch (MySqlException e)
            {
                Logger.Write("SQL.Blog: An error occured in AddBlogPost: " + e.Message + "\nSQL.Blog: Error Code: " + e.ErrorCode, "ERROR");
            }
        }

        public static async Task UpdateBlogPost(string title, string message, int? blogid)
        {
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "UPDATE blog SET title = @title, message = @message WHERE id = @id";
                    using (var cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@title", title);
                        cmd.Parameters.AddWithValue("@message", message);
                        cmd.Parameters.AddWithValue("@id", blogid);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                Logger.Write("Updated blog post " + blogid);
            }
            catch (MySqlException e)
            {
                Logger.Write("SQL.Blog: An error occured in UpdateBlogPost: " + e.Message + "\nSQL.Blog: Error Code: " + e.ErrorCode, "ERROR");
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
                    using (var cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", blogid);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                Logger.Write("Deleted blog post " + blogid);
            }
            catch (MySqlException e)
            {
                Logger.Write("SQL.Blog: An error occured in DeleteBlogPost: " + e.Message + "\nSQL.Blog: Error Code: " + e.ErrorCode, "ERROR");
            }
        }

        // to do - get date of blog post, maybe change return from tuple to array
        public static (string? title, string? message) GetBlogPost(int? blogid)
        {
            string? title = null, message = null; 
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "SELECT title, message FROM blog WHERE id = @id";
                    using (var cmd = new MySqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", blogid);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) 
                            {
                                title = reader.GetString(0);
                                message = reader.GetString(1);
                            }
                            else
                            {
                                return (null, null);
                            }
                        }
                    }
                    return (title, message);
                }
            }
            catch (MySqlException e)
            {
                Logger.Write("SQL.Blog: An error occured in GetBlogPost: " + e.Message + "\nSQL.Blog: Error Code: " + e.ErrorCode, "ERROR");
                return (null, null);
            }
        }

        public static int GetBlogPostCount()
        {
            int count = 0;
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "SELECT COUNT(*) FROM blog";
                    using (var cmd = new MySqlCommand(query, con))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader != null)
                            {
                                while (reader.Read())
                                {
                                    count = reader.GetInt32(0);
                                }
                            }
                        }
                    }
                }
                return count;
            }
            catch (MySqlException e)
            {
                Logger.Write("SQL.Blog: An error occured in GetBlogPostCount: " + e.Message + "\nSQL.Blog: Error Code: " + e.ErrorCode, "ERROR");
                return 0;
            }
        }
    }
}
