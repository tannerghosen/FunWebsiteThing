using MySql.Data.MySqlClient;

namespace FunWebsiteThing.SQL
{
    public static class Files
    {
        public static void Init()
        {
            using (var con = Main.Connect())
            {
                con.Open();

                // Files Table
                //                 string files = "CREATE TABLE IF NOT EXISTS files (fileid INT(11) PRIMARY KEY AUTO_INCREMENT, filename VARCHAR(255) NOT NULL, filetype TEXT NOT NULL, filelocation TEXT NOT NULL, userid INT(11), FOREIGN KEY (userid) REFERENCES accounts(id))";
                string files = "CREATE TABLE IF NOT EXISTS files (fileid INT(11) PRIMARY KEY AUTO_INCREMENT, filename VARCHAR(255) NOT NULL, filetype TEXT NOT NULL, filelocation TEXT NOT NULL, userid INT(11))";
                using (var cmd = new MySqlCommand(files, con))
                {
                    cmd.ExecuteNonQuery();
                }

                // File Description Table
                string filesd = "CREATE TABLE IF NOT EXISTS filesdescription (fileid INT(11) PRIMARY KEY, filetitle VARCHAR(255) NOT NULL, filedescription TEXT NOT NULL, FOREIGN KEY (fileid) REFERENCES files(fileid))";
                using (var cmd = new MySqlCommand(filesd, con))
                {
                    cmd.ExecuteNonQuery();
                }

                // Delete File Trigger
                string deletefiletrigger = "CREATE TRIGGER IF NOT EXISTS deletefiletrigger AFTER DELETE ON files FOR EACH ROW BEGIN DELETE FROM filesdescription WHERE fileid = OLD.fileid; END";
                using (var cmd = new MySqlCommand(deletefiletrigger, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
