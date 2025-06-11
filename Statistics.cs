namespace FunWebsiteThing
{
    public struct Stats
    {
        public int Logins;
        public int Registrations;
    }
    public static class Statistics
    {
        public async static void IncrementLogins()
        {
            Logger.Write("Increasementing logins", "STATS");
            await SQL.Stats.UpdateStat("logins");
        }

        public async static void IncrementRegistrations()
        {
            Logger.Write("Increasementing registration", "STATS");
            await SQL.Stats.UpdateStat("registrations");
        }

        public async static void ResetStats()
        {
            Logger.Write("Resetting stats", "STATS");
            await SQL.Stats.ResetStats();
        }

        public async static Task<Stats> GetStats()
        {
            int[] stats = await SQL.Stats.GetStats();
            return new Stats() { Logins = stats[0], Registrations = stats[1] };
        }

    }
}
