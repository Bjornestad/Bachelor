using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Bachelor.Models;

namespace Bachelor.Services
{
    public class SettingsManager
    {
        private readonly string _settingsFilePath;
        private Dictionary<string, MovementManagerService.MovementSetting> _settings;

        public SettingsManager()
        {
            _settingsFilePath = GetUserSettingsPath();
            LoadSettings();
        }

        private string GetUserSettingsPath()
        {
            string appFolder;
    
            // Determine OS and use appropriate path
            if (OperatingSystem.IsWindows())
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                appFolder = Path.Combine(appDataPath, "HeadPut");
            }
            else if (OperatingSystem.IsMacOS())
            {
                string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                appFolder = Path.Combine(homeDir, "HeadPut");
            }
            else // Linux and others
            {
                string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                appFolder = Path.Combine(homeDir, ".config", "HeadPut");
            }
    
            // Create app directory if it doesn't exist
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            return Path.Combine(appFolder, "UserSettings.json");
        }

        public Dictionary<string, MovementManagerService.MovementSetting> GetAllSettings()
        {
            return _settings;
        }

        public MovementManagerService.MovementSetting GetSetting(string movementName)
        {
            if (_settings.TryGetValue(movementName, out var setting))
            {
                return setting;
            }
            return null;
        }
        
        /*
        public void UpdateSetting(string movementName, MovementManagerService.MovementSetting updatedSetting)
        {
            if (_settings.ContainsKey(movementName))
            {
                _settings[movementName] = updatedSetting;
                SaveSettings();
            }
        }
        */
        
        public void UpdateSettingProperty(string movementName, string propertyName, object value)
        {
            if (!_settings.TryGetValue(movementName, out var setting))
            {
                return;
            }

            var property = typeof(MovementManagerService.MovementSetting).GetProperty(propertyName);
            if (property != null)
            {
                try
                {
                    property.SetValue(setting, Convert.ChangeType(value, property.PropertyType));
                    _settings[movementName] = setting;
                    SaveSettings();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating setting property: {ex.Message}");
                }
            }
        }

        public void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    _settings = JsonSerializer.Deserialize<Dictionary<string, MovementManagerService.MovementSetting>>(json);
            
                    var defaultSettings = DefaultMovementSettingsHelper.CreateDefaultSettings();
                    bool hasNewSettings = false;
            
                    foreach (var defaultSetting in defaultSettings)
                    {
                        if (!_settings.ContainsKey(defaultSetting.Key))
                        {
                            _settings[defaultSetting.Key] = defaultSetting.Value;
                            hasNewSettings = true;
                        }
                    }
            
                    // Save if new settings were added
                    if (hasNewSettings)
                    {
                        SaveSettings();
                    }
                }
                else
                {
                    // Create default settings if file doesn't exist
                    _settings = DefaultMovementSettingsHelper.CreateDefaultSettings();
                    SaveSettings();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
                _settings = DefaultMovementSettingsHelper.CreateDefaultSettings();
            }
        }

        public void SaveSettings()
        {
            try
            {
                string json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public void ResetToDefaults()
        {
            _settings = DefaultMovementSettingsHelper.CreateDefaultSettings();
            SaveSettings();
        }
    }
}