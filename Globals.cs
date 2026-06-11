namespace FunWebsiteThing
{
    public struct Globals
    {
        public static string AdminPassword { get; set; } // Password generated for the admin account on first setup
        public static string DatabaseName { get; set; } // Database Name
        public static bool DisableGoogle = true; // Disable Google OAuth
        public static string DomainName { get; set; } // Domain Name
        public static bool FirstTimeRunning { get; set; } // First Time Running check
    }
}
