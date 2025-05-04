using System.Collections.Generic;
using Bachelor.Services;
using Xunit;

namespace Bachelor.Test.UnitTest.ServicesTest
{
    public class DefaultMovementSettingsHelperTest
    {
        [Fact]
        public void CreateDefaultSettings_ReturnsValidDictionary()
        {
            var settings = DefaultMovementSettingsHelper.CreateDefaultSettings();
            
            Assert.NotNull(settings);
            Assert.IsType<Dictionary<string, MovementManagerService.MovementSetting>>(settings);
            Assert.Equal(6, settings.Count);
        }
        
        [Fact]
        public void CreateDefaultSettings_ContainsExpectedKeys()
        {
            var settings = DefaultMovementSettingsHelper.CreateDefaultSettings();
            
            Assert.Contains("HeadTiltLeft", settings.Keys);
            Assert.Contains("HeadTiltRight", settings.Keys);
            Assert.Contains("MouthOpen", settings.Keys);
            Assert.Contains("MouthWide", settings.Keys);
            Assert.Contains("HeadLeft", settings.Keys);
            Assert.Contains("HeadRight", settings.Keys);
        }
        
        [Fact]
        public void CreateDefaultSettings_HeadTiltLeftHasCorrectProperties()
        {
            var settings = DefaultMovementSettingsHelper.CreateDefaultSettings();
            var headTiltLeft = settings["HeadTiltLeft"];
            
            Assert.Equal("Q", headTiltLeft.Key);
            Assert.Equal(10, headTiltLeft.Threshold);
            Assert.Equal(0.5, headTiltLeft.Sensitivity);
            Assert.Equal("Roll", headTiltLeft.Coordinate);
            Assert.Equal("Negative", headTiltLeft.Direction);
            Assert.True(headTiltLeft.Enabled);
            Assert.True(headTiltLeft.Continuous);
            Assert.Equal("None", headTiltLeft.MouseActionType);
            Assert.Equal("Tilt Head Left", headTiltLeft.DisplayName);
        }
        
        [Fact]
        public void CreateDefaultSettings_MouthOpenHasCorrectProperties()
        {
            var settings = DefaultMovementSettingsHelper.CreateDefaultSettings();
            var mouthOpen = settings["MouthOpen"];
            
            Assert.Equal("Space", mouthOpen.Key);
            Assert.Equal(0.1, mouthOpen.Threshold);
            Assert.Equal(0.5, mouthOpen.Sensitivity);
            Assert.Equal("MouthHeight", mouthOpen.Coordinate);
            Assert.Equal("Positive", mouthOpen.Direction);
            Assert.True(mouthOpen.Enabled);
            Assert.False(mouthOpen.Continuous);
            Assert.Equal("None", mouthOpen.MouseActionType);
            Assert.Equal("Open Mouth", mouthOpen.DisplayName);
        }
    }
}