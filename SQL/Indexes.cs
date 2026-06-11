using MySql.Data.MySqlClient;

namespace FunWebsiteThing.SQL
{
    public static class Indexes
    {
        public static void AccountID()
        {
            using (var con = Main.Connect())
            {
                con.Open();

                string trigger = "CREATE INDEX IF NOT EXISTS index_accountid ON accounts (id)";
                using (var cmd = new MySqlCommand(trigger, con))
                {
                    cmd.ExecuteNonQuery();
                }

                con.Close();
            }
        }
        public static void AccountUsername()
        {
            using (var con = Main.Connect())
            {
                con.Open();

                string trigger = "CREATE INDEX IF NOT EXISTS index_username ON accounts (username)";
                using (var cmd = new MySqlCommand(trigger, con))
                {
                    cmd.ExecuteNonQuery();
                }

                con.Close();
            }
        }
        public static void AccountEmail()
        {
            using (var con = Main.Connect())
            {
                con.Open();

                string trigger = "CREATE INDEX IF NOT EXISTS index_accountemail ON accounts (email)";
                using (var cmd = new MySqlCommand(trigger, con))
                {
                    cmd.ExecuteNonQuery();
                }

                con.Close();
            }
        }
        public static void BlogPostID()
        {
            using (var con = Main.Connect())
            {
                con.Open();

                string trigger = "CREATE INDEX IF NOT EXISTS index_blogpostid ON blog (id)";
                using (var cmd = new MySqlCommand(trigger, con))
                {
                    cmd.ExecuteNonQuery();
                }

                con.Close();
            }
        }
        public static void CommentSID()
        {
            using (var con = Main.Connect())
            {
                con.Open();

                string trigger = "CREATE INDEX IF NOT EXISTS index_commentsid ON comments (commentsid)";
                using (var cmd = new MySqlCommand(trigger, con))
                {
                    cmd.ExecuteNonQuery();
                }

                con.Close();
            }
        }
    }
}
