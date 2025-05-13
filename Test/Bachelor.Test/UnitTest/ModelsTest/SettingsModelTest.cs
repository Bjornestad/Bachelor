using Bachelor.Models;
using Bachelor.Services;
using System.Collections.Generic;
using System.ComponentModel;
using Bachelor.Interfaces;
using Xunit;
using Moq;

namespace Bachelor.Test
{
    public class SettingsModelTest
    {
        [Fact]
        public void Constructor_InitializesSettings()
        {
            var mockSettings = new Dictionary<string, MovementManagerService.MovementSetting>
            {
                { "movement1", new MovementManagerService.MovementSetting() }
            };
            
            var mockSettingsManager = new Mock<ISettingsManager>();
            mockSettingsManager.Setup(m => m.GetAllSettings()).Returns(mockSettings);
            
            var settingsModel = new SettingsModel(mockSettingsManager.Object);
            
            Assert.Same(mockSettings, settingsModel.Settings);
            mockSettingsManager.Verify(m => m.GetAllSettings(), Times.Once);
        }
        
        [Fact]
        public void UpdateSettingProperty_CallsManagerAndRefreshesSettings()
        {
            var initialSettings = new Dictionary<string, MovementManagerService.MovementSetting>();
            var updatedSettings = new Dictionary<string, MovementManagerService.MovementSetting>();
            
            var mockSettingsManager = new Mock<ISettingsManager>();
            mockSettingsManager.Setup(m => m.GetAllSettings())
                .Returns(initialSettings)
                .Verifiable();
            
            mockSettingsManager.Setup(m => m.UpdateSettingProperty("test", "sensitivity", 0.5))
                .Verifiable();
            
            // Setup second call to return updated settings
            mockSettingsManager.SetupSequence(m => m.GetAllSettings())
                .Returns(initialSettings)
                .Returns(updatedSettings);
            
            var settingsModel = new SettingsModel(mockSettingsManager.Object);
            
            settingsModel.UpdateSettingProperty("test", "sensitivity", 0.5);
            
            mockSettingsManager.Verify(m => m.UpdateSettingProperty("test", "sensitivity", 0.5), Times.Once);
            mockSettingsManager.Verify(m => m.GetAllSettings(), Times.Exactly(2));
            Assert.Same(updatedSettings, settingsModel.Settings);
        }
        
        [Fact]
        public void ResetToDefaults_CallsManagerAndRefreshesSettings()
        {
            var initialSettings = new Dictionary<string, MovementManagerService.MovementSetting>();
            var defaultSettings = new Dictionary<string, MovementManagerService.MovementSetting>();
            
            var mockSettingsManager = new Mock<ISettingsManager>();
            mockSettingsManager.SetupSequence(m => m.GetAllSettings())
                .Returns(initialSettings)
                .Returns(defaultSettings);
            
            mockSettingsManager.Setup(m => m.ResetToDefaults())
                .Verifiable();
            
            var settingsModel = new SettingsModel(mockSettingsManager.Object);
            
            settingsModel.ResetToDefaults();
            
            mockSettingsManager.Verify(m => m.ResetToDefaults(), Times.Once);
            Assert.Same(defaultSettings, settingsModel.Settings);
        }
        
        [Fact]
        public void Settings_PropertyChanged_RaisesEvent()
        {
            var mockSettingsManager = new Mock<ISettingsManager>();
            mockSettingsManager.Setup(m => m.GetAllSettings())
                .Returns(new Dictionary<string, MovementManagerService.MovementSetting>());
                
            var settingsModel = new SettingsModel(mockSettingsManager.Object);
            
            bool eventRaised = false;
            PropertyChangedEventArgs eventArgs = null;
            
            settingsModel.PropertyChanged += (sender, args) => {
                eventRaised = true;
                eventArgs = args;
            };
            
            settingsModel.ResetToDefaults();
            
            Assert.True(eventRaised);
            Assert.Equal("Settings", eventArgs.PropertyName);
        }
    }
}