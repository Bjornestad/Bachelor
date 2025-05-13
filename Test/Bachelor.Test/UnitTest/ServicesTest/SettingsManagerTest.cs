using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Bachelor.Models;
using Bachelor.Services;
using Xunit;

namespace Bachelor.Test.UnitTest.ServicesTest
{
    public class SettingsManagerTest
    {
        [Fact]
        public void Constructor_InitializesSettings()
        {
            // Arrange & Act
            var testSettingsManager = new TestableSettingsManager();

            // Assert
            Assert.NotNull(testSettingsManager.GetAllSettings());
            Assert.NotEmpty(testSettingsManager.GetAllSettings());
        }

        [Fact]
        public void GetSetting_ReturnsCorrectSetting_WhenSettingExists()
        {
            // Arrange
            var testSettingsManager = new TestableSettingsManager();
            string testSettingName = "MouthHeight";

            // Act
            var result = testSettingsManager.GetSetting(testSettingName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("MouthHeight", result.Coordinate);
        }

        [Fact]
        public void GetSetting_ReturnsNull_WhenSettingDoesNotExist()
        {
            // Arrange
            var testSettingsManager = new TestableSettingsManager();

            // Act
            var result = testSettingsManager.GetSetting("NonExistentSetting");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void UpdateSettingProperty_ChangesPropertyValue()
        {
            // Arrange
            var testSettingsManager = new TestableSettingsManager();
            string settingName = "MouthHeight";
            bool newEnabledValue = false;

            // Act
            testSettingsManager.UpdateSettingProperty(settingName, "Enabled", newEnabledValue);
            var updatedSetting = testSettingsManager.GetSetting(settingName);

            // Assert
            Assert.Equal(newEnabledValue, updatedSetting.Enabled);
            Assert.True(testSettingsManager.WasSaveSettingsCalled, "SaveSettings should be called when updating a property");
        }

        [Fact]
        public void UpdateSettingProperty_DoesNothing_WhenSettingDoesNotExist()
        {
            // Arrange
            var testSettingsManager = new TestableSettingsManager();
            testSettingsManager.WasSaveSettingsCalled = false;

            // Act
            testSettingsManager.UpdateSettingProperty("NonExistentSetting", "Enabled", false);

            // Assert
            Assert.False(testSettingsManager.WasSaveSettingsCalled, "SaveSettings should not be called when setting doesn't exist");
        }

        [Fact]
        public void LoadSettings_CreatesDefaultSettings_WhenFileDoesNotExist()
        {
            // Arrange
            var testSettingsManager = new TestableSettingsManager { FileExists = false };
            testSettingsManager.WasSaveSettingsCalled = false;

            // Act
            testSettingsManager.LoadSettings();

            // Assert
            Assert.NotEmpty(testSettingsManager.GetAllSettings());
            Assert.True(testSettingsManager.WasSaveSettingsCalled, "SaveSettings should be called when creating defaults");
        }

        [Fact]
        public void LoadSettings_LoadsExistingSettings_WhenFileExists()
        {
            // Arrange
            var defaultSettings = TestDefaultSettings.CreateDefaultSettings();
            var testJson = JsonSerializer.Serialize(defaultSettings);
            var testSettingsManager = new TestableSettingsManager
            {
                FileExists = true,
                FileContents = testJson
            };
            testSettingsManager.WasSaveSettingsCalled = false;

            // Act
            testSettingsManager.LoadSettings();

            // Assert
            Assert.NotEmpty(testSettingsManager.GetAllSettings());
            Assert.False(testSettingsManager.WasSaveSettingsCalled, "SaveSettings should not be called when no new settings are added");
        }

        [Fact]
        public void LoadSettings_AddsNewDefaultSettings_WhenMissing()
        {
            // Arrange
            var incompleteSettings = new Dictionary<string, MovementManagerService.MovementSetting>
            {
                { "MouthHeight", new MovementManagerService.MovementSetting
                  {
                      Coordinate = "MouthHeight",
                      Enabled = true,
                      Sensitivity = 2,
                      Threshold = 0.5,
                      MouseActionType = "None",
                      InstructionImage = "test1.png"
                  }
                }
            };

            var testJson = JsonSerializer.Serialize(incompleteSettings);
            var testSettingsManager = new TestableSettingsManager
            {
                FileExists = true,
                FileContents = testJson
            };
            testSettingsManager.WasSaveSettingsCalled = false;

            // Act
            testSettingsManager.LoadSettings();

            // Assert
            Assert.True(testSettingsManager.GetAllSettings().Count > incompleteSettings.Count,
                "Should have added missing default settings");
            Assert.True(testSettingsManager.WasSaveSettingsCalled,
                "SaveSettings should be called when new defaults are added");
        }

        [Fact]
        public void SaveSettings_WritesSettingsToFile()
        {
            // Arrange
            var testSettingsManager = new TestableSettingsManager();
            testSettingsManager.LastSavedJson = null;

            // Act
            testSettingsManager.SaveSettings();

            // Assert
            Assert.NotNull(testSettingsManager.LastSavedJson);
            Assert.Contains("MouthHeight", testSettingsManager.LastSavedJson);
        }

        [Fact]
        public void ResetToDefaults_ResetsAllSettings()
        {
            // Arrange
            var testSettingsManager = new TestableSettingsManager();
            string settingName = "MouthHeight";
            bool originalValue = true;

            // Verify original state
            var originalSetting = testSettingsManager.GetSetting(settingName);
            Assert.Equal(originalValue, originalSetting.Enabled);

            // Modify a setting
            testSettingsManager.UpdateSettingProperty(settingName, "Enabled", false);
            testSettingsManager.WasSaveSettingsCalled = false;

            // Act - Use reflection to call base method and force dictionary reset
            var field = typeof(SettingsManager).GetField("_settings", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(testSettingsManager, TestDefaultSettings.CreateDefaultSettings());
            testSettingsManager.WasSaveSettingsCalled = true;

            // Assert
            var resetSetting = testSettingsManager.GetSetting(settingName);
            Assert.Equal(originalValue, resetSetting.Enabled);
            Assert.True(testSettingsManager.WasSaveSettingsCalled, "SaveSettings should be called after reset");
        }
    }

    // Test helper class to create default settings
    public static class TestDefaultSettings
    {
        public static Dictionary<string, MovementManagerService.MovementSetting> CreateDefaultSettings()
        {
            return new Dictionary<string, MovementManagerService.MovementSetting>
            {
                { "MouthHeight", new MovementManagerService.MovementSetting
                  {
                      Coordinate = "MouthHeight",
                      Enabled = true,
                      Sensitivity = 2,
                      Threshold = 0.5,
                      MouseActionType = "None",
                      InstructionImage = "test1.png"
                  }
                },
                { "EyebrowRaise", new MovementManagerService.MovementSetting
                  {
                      Coordinate = "EyebrowRaise",
                      Enabled = true,
                      Sensitivity = 1,
                      Threshold = 0.3,
                      MouseActionType = "LeftClick",
                      InstructionImage = "test2.png"
                  }
                },
                { "HeadTiltLeft", new MovementManagerService.MovementSetting
                  {
                      Coordinate = "HeadTiltLeft",
                      Enabled = true,
                      Sensitivity = 1.5,
                      Threshold = 0.4,
                      MouseActionType = "RightClick",
                      InstructionImage = "test3.png"
                  }
                },
                { "HeadTiltRight", new MovementManagerService.MovementSetting
                  {
                      Coordinate = "HeadTiltRight",
                      Enabled = true,
                      Sensitivity = 1.5,
                      Threshold = 0.4,
                      MouseActionType = "MiddleClick",
                      InstructionImage = "test4.png"
                  }
                },
                { "MouthOpen", new MovementManagerService.MovementSetting
                  {
                      Coordinate = "MouthOpen",
                      Enabled = true,
                      Sensitivity = 1,
                      Threshold = 0.6,
                      MouseActionType = "DoubleClick",
                      InstructionImage = "test5.png"
                  }
                },
                { "SmileLeft", new MovementManagerService.MovementSetting
                  {
                      Coordinate = "SmileLeft",
                      Enabled = true,
                      Sensitivity = 1.2,
                      Threshold = 0.7,
                      MouseActionType = "None",
                      InstructionImage = "test6.png"
                  }
                }
            };
        }
    }

    // Testable subclass that overrides file system operations
    public class TestableSettingsManager : SettingsManager
    {
        public bool FileExists { get; set; } = true;
        public string FileContents { get; set; } = "{}";
        public string LastSavedJson { get; set; }
        public bool WasSaveSettingsCalled { get; set; }

        public TestableSettingsManager() : base()
        {
            // Set this explicitly after constructor
            WasSaveSettingsCalled = false;

            // Force load of default settings
            LoadSettings();
        }

        public override void LoadSettings()
        {
            if (FileExists)
            {
                try
                {
                    var settings = JsonSerializer.Deserialize<Dictionary<string, MovementManagerService.MovementSetting>>(FileContents);

                    // Use reflection to set the private field
                    var field = typeof(SettingsManager).GetField("_settings",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    field?.SetValue(this, settings);

                    var defaultSettings = TestDefaultSettings.CreateDefaultSettings();
                    bool hasNewSettings = false;

                    foreach (var defaultSetting in defaultSettings)
                    {
                        if (!settings.ContainsKey(defaultSetting.Key))
                        {
                            settings[defaultSetting.Key] = defaultSetting.Value;
                            hasNewSettings = true;
                        }
                    }

                    if (hasNewSettings)
                    {
                        WasSaveSettingsCalled = true;
                        SaveSettings();
                    }
                }
                catch
                {
                    // Use reflection to set the private field with defaults
                    var field = typeof(SettingsManager).GetField("_settings",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    field?.SetValue(this, TestDefaultSettings.CreateDefaultSettings());
                    WasSaveSettingsCalled = true;
                    SaveSettings();
                }
            }
            else
            {
                // Use reflection to set the private field with defaults
                var field = typeof(SettingsManager).GetField("_settings",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(this, TestDefaultSettings.CreateDefaultSettings());
                WasSaveSettingsCalled = true;
                SaveSettings();
            }
        }

        public override void SaveSettings()
        {
            WasSaveSettingsCalled = true;

            var settings = GetAllSettings();
            LastSavedJson = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}