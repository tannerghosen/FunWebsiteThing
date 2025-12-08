using MySql.Data.MySqlClient;

namespace FunWebsiteThing.SQL
{
    public static class Triggers
    {
        // Creates ban entries for existing accounts
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

        // Unbans on Expire for Accounts
        public static void UnbanOnExpire()
        {
            using (var con = Main.Connect())
            {
                con.Open();
                string e = "CREATE EVENT IF NOT EXISTS unbanonexpireevent ON SCHEDULE EVERY 30 SECOND DO UPDATE accountbans SET expire = NOW(), banned = 0 WHERE expire <= NOW()";
                using (var cmd = new MySqlCommand(e, con))
                {
                    cmd.ExecuteNonQuery();
                }
                con.Close();
            }
        }

        // Unbans on Expire for IPs
        public static void UnbanOnExpireIP()
        {
            using (var con = Main.Connect())
            {
                con.Open();
                string e = "CREATE EVENT IF NOT EXISTS unbanonexpireipevent ON SCHEDULE EVERY 30 SECOND DO DELETE FROM bans WHERE expire <= NOW()";
                using (var cmd = new MySqlCommand(e, con))
                {
                    cmd.ExecuteNonQuery();
                }
                con.Close();
            }
        }
    }
}
