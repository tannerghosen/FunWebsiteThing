using MySql.Data.MySqlClient;

namespace FunWebsiteThing.SQL
{
    public class Files
    {
        public static void Init()
        {
            using (var con = Main.Connect())
            {
                con.Open();

                // Files Table
                string accounts = "CREATE TABLE IF NOT EXISTS files (fileid INT(11) PRIMARY KEY AUTO_INCREMENT, filename TEXT NOT NULL, filetype TEXT NOT NULL, filelocation TEXT NOT NULL, userid INT(11))";
                using (var cmd = new MySqlCommand(accounts, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
