using System.Text.RegularExpressions;
using System.Text;

namespace FunWebsiteThing.Controllers.Classes
{
    public class Password
    {
        /* Passwords: Generated passwords kept in a HashSet to prevent duplicate passwords from being generated. */
        //private static HashSet<string> Passwords = new HashSet<string>();

        /* Other Misc Things: characters, regexpattern, regex */
        private static string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
        /* It checks for:
        1 uppercase letter
        1 lowercase letter
        1 number
        1 special character
        should not repeat characters more than 5 times consecutively
        8-32 characters in width
        */
        private static string passregexpattern = "^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-])(?!.*(.)\\1{5,}).{8,32}$";
        private static Regex passregex = new Regex(passregexpattern);

        public static string GeneratePassword()
        {
            StringBuilder password = new StringBuilder(string.Empty);
            Random r = new Random();
            for (int i = 0; i < 16; i++)
            {
                // characters[Random([0, characters' length])];
                password.Append(characters[r.Next(characters.Length)]);
            }
            string GeneratedPass = password.ToString();
            return passregex.IsMatch(GeneratedPass) == true ? GeneratedPass : GeneratePassword();
        }

        public static bool ValidatePassword(string password)
        {
            return (!passregex.IsMatch(password) || password == null) ? true : false;
        }
    }
}
