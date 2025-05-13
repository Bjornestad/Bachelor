using Xunit;
using Bachelor.Models;
using Bachelor.Services;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Bachelor.Interfaces;

public class ConfigurationIntegrationTests
{

    [Fact]
    public void SettingsManager_LoadsAndSavesConfigurationCorrectly()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ISettingsManager, SettingsManager>();
        services.AddSingleton<SettingsModel>(); 
        var serviceProvider = services.BuildServiceProvider();

        var settingsManager = serviceProvider.GetRequiredService<ISettingsManager>();
        var settingsModel = serviceProvider.GetRequiredService<SettingsModel>();

       
        var testMovementName = "HeadTiltLeft";
        var testPropertyName = "Sensitivity";
        var testValue = 0.85;

        settingsModel.UpdateSettingProperty(testMovementName, testPropertyName, testValue);

        var settings = settingsModel.Settings;

        Assert.Equal(testValue, settings[testMovementName].Sensitivity);
    }
}