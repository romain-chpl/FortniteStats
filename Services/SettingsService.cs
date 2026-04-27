using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using FortniteStatsDesktop.Models;

namespace FortniteStatsDesktop.Services
{
    public class SettingsService
    {
        private readonly string _settingsFilePath;
        public UserSettings CurrentSettings { get; private set; }

        public SettingsService()
        {
            _settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserSettings.json");
            CurrentSettings = new UserSettings();
            LoadSettingsSync();
        }

        private void LoadSettingsSync()
        {
            if (File.Exists(_settingsFilePath))
            {
                try
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize<UserSettings>(json);
                    if (settings != null)
                    {
                        CurrentSettings = settings;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de la lecture des paramètres: {ex.Message}");
                }
            }
        }

        public async Task SaveSettingsAsync()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(CurrentSettings, options);
                await File.WriteAllTextAsync(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la sauvegarde des paramètres: {ex.Message}");
            }
        }
    }
}
