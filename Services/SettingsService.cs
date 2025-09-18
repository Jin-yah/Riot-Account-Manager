using System.Text.Json;

namespace LoLAccountLauncher.Services
{
    /// <summary>
    /// Represents the application settings.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Gets or sets the path to the Riot Client executable.
        /// </summary>
        public string? RiotClientPath { get; set; }

        /// <summary>
        /// Gets or sets the delay in milliseconds before launching the Riot Client.
        /// Default is 3000ms (3 seconds).
        /// </summary>
        public int LaunchDelayMs { get; set; } = 3000; // Default to 3 seconds

        /// <summary>
        /// Gets or sets a value indicating whether to automatically check for updates.
        /// Default is true.
        /// </summary>
        public bool CheckForUpdates { get; set; } = true;
    }

    /// <summary>
    /// Provides methods for loading and saving application settings.
    /// </summary>
    public static class SettingsService
    {
        private static readonly string _settingsFilePath;
        private static AppSettings? _settings;

        /// <summary>
        /// Initializes the <see cref="SettingsService"/> class.
        /// Sets up the path for the settings file.
        /// </summary>
        static SettingsService()
        {
            var appDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "LoLAccountLauncher"
            );
            Directory.CreateDirectory(appDataDir);
            _settingsFilePath = Path.Combine(appDataDir, "settings.json");
        }

        /// <summary>
        /// Loads the application settings from the settings file.
        /// If the file does not exist or an error occurs, default settings are returned.
        /// </summary>
        /// <returns>The loaded application settings.</returns>
        public static AppSettings LoadSettings()
        {
            if (_settings != null)
            {
                return _settings;
            }

            if (!File.Exists(_settingsFilePath))
            {
                _settings = new AppSettings();
                return _settings;
            }

            try
            {
                var json = File.ReadAllText(_settingsFilePath);
                _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch (Exception)
            {
                _settings = new AppSettings();
            }

            return _settings;
        }

        /// <summary>
        /// Saves the provided application settings to the settings file.
        /// </summary>
        /// <param name="settings">The settings to save.</param>
        public static void SaveSettings(AppSettings settings)
        {
            try
            {
                var json = JsonSerializer.Serialize(
                    settings,
                    new JsonSerializerOptions { WriteIndented = true }
                );
                File.WriteAllText(_settingsFilePath, json);
                _settings = settings;
            }
            catch (Exception) { }
        }
    }
}
