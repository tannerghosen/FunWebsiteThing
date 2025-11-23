namespace FunWebsiteThing
{
    // Just a basic class to help JavaScript files in the website
    public static class JavaScriptHelper
    {
        public static string DomainName = String.Empty;
        public static void SetDomainName(string DN = "")
        {
            DomainName = DN;
        }
        public static string GetDomainName()
        {
            return DomainName;
        }
    }
}
