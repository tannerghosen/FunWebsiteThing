namespace FunWebsiteThing
{
    public struct Stats
    {
        public int Logins;
        public int Registrations;
    }
    public static class Statistics
    {
        public static int Logins;
        public static int Registrations;
        public static void IncrementLogins()
        {
        }

        public static void IncrementRegistrations()
        {
        }

        public static void ResetStats()
        {
        }

        public static Stats GetStats()
        {
            return new Stats() { Logins = Logins, Registrations = Registrations };
        }

        public static void SaveStats()
        {
        }
    }
}
