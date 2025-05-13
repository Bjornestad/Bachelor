using System.Collections.Generic;
using Bachelor.Services;

namespace Bachelor.Interfaces
{
    public interface ISettingsManager
    {
        Dictionary<string, MovementManagerService.MovementSetting> GetAllSettings();
        MovementManagerService.MovementSetting GetSetting(string movementName);
        void UpdateSettingProperty(string movementName, string propertyName, object value);
        void ResetToDefaults();
        void LoadSettings();
        void SaveSettings();
    }
}