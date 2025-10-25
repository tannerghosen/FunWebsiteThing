using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MySql.Data.MySqlClient;

namespace FunWebsiteThing.SQL
{
    public static class Triggers
    {
        public static void Accounts()
        {
            using (var con = Main.Connect())
            {
                con.Open();

                string trigger = "CREATE TRIGGER IF NOT EXISTS accountstrigger AFTER INSERT ON accounts FOR EACH ROW BEGIN INSERT INTO accountbans(id, banned, reason) VALUES (NEW.id, false, ''); END;";
                using (var cmd = new MySqlCommand(trigger, con))
                {
                    cmd.ExecuteNonQuery();
                }

                con.Close();
            }
        }

        public static void UnbanOnExpire()
        {
            using (var con = Main.Connect())
            {
                con.Open();
                string e = "CREATE EVENT IF NOT EXISTS unbanonexpireevent ON SCHEDULE EVERY 30 SECOND COMMENT 'Unban every 30 seconds' DO UPDATE accountbans SET expire = CURDATE(), banned = 0 WHERE expire <= CURDATE()";
                using (var cmd = new MySqlCommand(e, con))
                {
                    cmd.ExecuteNonQuery();
                }
                con.Close();
            }
        }

        public static void UnbanOnExpireIP()
        {
            using (var con = Main.Connect())
            {
                con.Open();
                string e = "CREATE EVENT IF NOT EXISTS unbanonexpireipevent ON SCHEDULE EVERY 30 SECOND COMMENT 'Unban every 30 seconds' DO DELETE FROM bans WHERE expire <= CURDATE()";
                using (var cmd = new MySqlCommand(e, con))
                {
                    cmd.ExecuteNonQuery();
                }
                con.Close();
            }
        }
    }
}
