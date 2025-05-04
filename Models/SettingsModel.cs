using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Bachelor.Services;

namespace Bachelor.Models
{
    public class SettingsModel : INotifyPropertyChanged
    {
        private readonly ISettingsManager _settingsManager;
        private Dictionary<string, MovementManagerService.MovementSetting> _settings;

        public event PropertyChangedEventHandler PropertyChanged;

        public Dictionary<string, MovementManagerService.MovementSetting> Settings
        {
            get => _settings;
            private set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        public SettingsModel(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
            _settings = _settingsManager.GetAllSettings();
        }

        /*
        public void UpdateSetting(string movementName, MovementManagerService.MovementSetting setting)
        {
            _settingsManager.UpdateSetting(movementName, setting);
            // Refresh settings from manager
            Settings = _settingsManager.GetAllSettings();
        }
        //isnt used atm
        */ 
        
        public void UpdateSettingProperty(string movementName, string propertyName, object value)
        {
            _settingsManager.UpdateSettingProperty(movementName, propertyName, value);
            // Refresh settings from manager
            Settings = _settingsManager.GetAllSettings();
        }

        public void ResetToDefaults()
        {
            _settingsManager.ResetToDefaults();
            Settings = _settingsManager.GetAllSettings();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}