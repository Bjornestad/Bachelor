using Bachelor.Models;

namespace Bachelor.ViewModels
{
    public class SettingsViewModel
    {
        public SettingsModel SettingsModel { get; }

        public SettingsViewModel(SettingsModel settingsModel)
        {
            SettingsModel = settingsModel;
        }
    }
}