using Microsoft.Data.Sqlite;

namespace FunWebsiteThing.SQL
{
    public class Comments
    {
        public static void Init()
        {
            using (var con = Main.Connect())
            {
                con.Open();

                string comments = "CREATE TABLE IF NOT EXISTS comments (id INTEGER PRIMARY KEY AUTOINCREMENT, commentsid INTEGER NOT NULL, userid INTEGER NOT NULL, comment NVARCHAR(255), date DATETIME DEFAULT CURRENT_TIMESTAMP)";
                using (var cmd = new SqliteCommand(comments, con))
                {
                    cmd.ExecuteNonQuery();
                }

                con.Close();
            }
        }
        // Adds a comment to a specified comment section
        public static async Task AddComment(string comment, string username = "Anonymous", int commentsection = 0)
        {
            int userid, anonymousid = -1;
            if (comment == null)
            {
                comment = "";
            }
            if (!Accounts.DoesUserExist(username) || (username == "" || username == null || username == "Anonymous"))
            {
                userid = anonymousid;
            }
            else
            {
                userid = Accounts.GetUserID(username);
            }
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string q = "INSERT INTO comments (userid, commentsid, comment, date) VALUES (@userid, @commentsection, @comment, DATETIME('now', 'utc', '-8 hours'))";
                    using (var cmd = new SqliteCommand(q, con))
                    {
                        cmd.Parameters.AddWithValue("@userid", userid);
                        cmd.Parameters.AddWithValue("@comment", comment);
                        cmd.Parameters.AddWithValue("@commentsection", commentsection);
                        await cmd.ExecuteNonQueryAsync();
                    }
                    Logger.Write("Comment added by " + username + " to comment section id " + commentsection);
                }
            }
            catch (SqliteException e)
            {
                Logger.Write("SQL.Comments: An error occured in AddComment: " + e.Message + "\nSQL.Comments: Error Code: " + e.SqliteErrorCode, "ERROR");
            }
        }

        // Grabs usernames, comments, and dates from database and returns them as arrays
        public static string[][]? GrabComments(int section = 0)
        {
            // we grab username for the poster's name, comments for the comment, dates for the date of comment posted, and the comment's id
            List<string> usernames = new List<string>();
            List<string> comments = new List<string>();
            List<string> dates = new List<string>();
            List<string> ids = new List<string>();
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    // SELECT account username, comments comment, comments date FROM comments JOIN accounts on comments userid = accounts id WHERE commentsid = section ORDER BY comments date DESC
                    string query = @"SELECT a.username, c.comment, c.date, c.id FROM comments c JOIN accounts a ON a.id = c.userid WHERE c.commentsid = @section ORDER BY c.date DESC";
                    using (var cmd = new SqliteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@section", section);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader != null)
                            {
                                while (reader.Read())
                                {
                                    usernames.Add(reader.GetString(0));
                                    comments.Add(reader.GetString(1));
                                    dates.Add(reader.GetString(2));
                                    ids.Add(reader.GetString(3));
                                }
                            }
                        }
                    }
                    return new string[][] { usernames.ToArray(), comments.ToArray(), dates.ToArray(), ids.ToArray() };
                }
            }
            catch (SqliteException e)
            {
                Logger.Write("SQL.Comments: An error occured in GrabComments: " + e.Message + "\nSQL.Comments: Error Code: " + e.SqliteErrorCode, "ERROR");
                return null;
            }
        }

        public static async Task DeleteComment(int? commentid)
        {
            try
            {
                using (var con = Main.Connect())
                {
                    con.Open();
                    string query = "DELETE FROM comments WHERE id = @id";
                    using (var cmd = new SqliteCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@id", commentid);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                Logger.Write("Deleted comment with id " + commentid);
            }
            catch (SqliteException e)
            {
                Logger.Write("SQL.Comments: An error occured in DeleteComment: " + e.Message + "\nSQL.Comments: Error Code: " + e.SqliteErrorCode, "ERROR");
            }
        }
    }
}
