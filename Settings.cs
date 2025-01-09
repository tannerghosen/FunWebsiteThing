using System.IO;
using System.Text.Json;

namespace FunWebsiteThing
{
    /// <summary>
    /// This class handles FWT's settings
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// Settings file
        /// </summary>
        public static string SettingsFile = "./settings.json";

        /// <summary>
        /// Our SQL connection string
        /// </summary>
        private static string ConnectionString { get; set; }

        public static void Init()
        {
            if (!File.Exists(SettingsFile)) // if settings.json file doesn't exist
            {
                // We initialize it with default settings
                ConnectionString = "Data Source=database.db;Password=your_password_here";


                SaveSettings(); // Save the settings

                Logger.Write("Created settings file.");
            }
            else // else load the settings from settings.json
            {
                string json = File.ReadAllText(SettingsFile); // read the file as a string
                JsonDocument settings = JsonDocument.Parse(json); // parse it as a json string

                ConnectionString = settings.RootElement.GetProperty("ConnectionString").GetString();

                settings.Dispose(); // end the Parse

                Logger.Write("Loaded settings file.");
            }
            //SQLStuff.SetConnectionString(ConnectionString);
        }
        /// <summary>
        /// Updates a specified setting.
        /// </summary>
        /// <param name="setting"></param>
        public static void UpdateSettings(string setting)
        {
            // Simply enough, this switch inverts our setting
            switch (setting)
            {
                default:
                    Logger.Write("Invalid setting was specified for UpdateSettings without value parameter.", "ERROR");
                    break;
            }
            // And we save our settings.
            Logger.Write("Updated Settings");

            SaveSettings();
        }

        /// <summary>
        /// Updates a specified setting with a value.
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="value"></param>
        public static void UpdateSettings(string setting, string value)
        {
            switch (setting)
            {
                case "ConnectionString":
                    ConnectionString = value;
                    break;
                default:
                    Logger.Write("Invalid setting was specified for UpdateSettings with value parameter.", "ERROR");
                    break;
            }

            Logger.Write("Updated Settings");

            SaveSettings();
        }
        /// <summary>
        /// Saves our settings to settings.json.
        /// </summary>
        public static void SaveSettings()
        {
            // We write into our settings.json file a JSON object
            // This contains our settings.
            string cs = JsonSerializer.Serialize(ConnectionString);
            using (StreamWriter writer = new StreamWriter(SettingsFile))
            {
                // Because C# bools are capitalized, we need to lowercase it before we send it,
                // as shown in the code below.
                writer.WriteLine("{");
                writer.WriteLine("\"ConnectionString\": " + cs); // we need to make our Folder string into a JSON string that won't cause errors.
                writer.WriteLine("}");
                writer.Close();
            }
            Logger.Write("Saved Settings.");
        }

        public static string[] GetSettings()
        {
            return new string[] { ConnectionString };
        }
    }
}
