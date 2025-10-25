using MySql.Data.MySqlClient;

namespace FunWebsiteThing.SQL
{
    public static class Misc
    {
        public static void CreateDefaultAccounts()
        {
            using (var con = Main.Connect())
            {
                con.Open();
                // Admin Account
                string doesadminexist = "SELECT COUNT(*) FROM accounts WHERE id = 1";
                using (var c = new MySqlCommand(doesadminexist, con))
                {
                    int count = Convert.ToInt32(c.ExecuteScalar());
                    if (count == 0)
                    {
                        string pass = BCrypt.Net.BCrypt.HashPassword("test");
                        string createadmin = "INSERT INTO accounts (id, email, username, password, isadmin) VALUES (1, 'admin@email.com', 'Admin', @pass, 1)";
                        using (var cmd = new MySqlCommand(createadmin, con))
                        {
                            cmd.Parameters.AddWithValue("@pass", pass);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                // Anonymous Account
                string doesanonymousexist = "SELECT COUNT(*) FROM accounts WHERE id = -1";
                using (var c = new MySqlCommand(doesanonymousexist, con))
                {
                    int count = Convert.ToInt32(c.ExecuteScalar());
                    if (count == 0)
                    {
                        string pass = BCrypt.Net.BCrypt.HashPassword("tSFSDAKFSDJKGFISDJTR89324JR283JI213HE812H3E8D1H2IKASKFHDASKDFHKASHDKASHDKAH1231241251241231;;'===---+++SDA");
                        string createanonymous = "INSERT INTO accounts (id, email, username, password) VALUES (-1, 'anonymous@email.com', 'Anonymous', @pass)";
                        using (var cmd = new MySqlCommand(createanonymous, con))
                        {
                            cmd.Parameters.AddWithValue("@pass", pass);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                con.Close();
            }
        }
    }
}
