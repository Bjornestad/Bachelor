using Xunit;
using Bachelor.Models;
using Bachelor.Services;
using Bachelor.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Bachelor.Interfaces;

namespace Bachelor.Test.IntegrationTest
{
    public class InputSimulationIntegrationTest
    {
        [Fact]
        public async Task HeadTiltLeft_ShouldTriggerCorrespondingKeyPress()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ISettingsManager, SettingsManager>();
            services.AddSingleton<SettingsModel>();
            services.AddSingleton<OutputViewModel>();
            services.AddSingleton<MockInputService>();
            var serviceProvider = services.BuildServiceProvider();

            var settingsModel = serviceProvider.GetRequiredService<SettingsModel>();
            var inputService = serviceProvider.GetRequiredService<MockInputService>();
            var tracker = new MockTracker(inputService, settingsModel);
            
            var trackingData = new FacialTrackingData
            {
                lEyeCornerY = 0.8,
                rEyeCornerY = 0.2
            };

            tracker.ProcessTrackingData(trackingData);
            await Task.Delay(100); // Allow processing time

            Assert.Contains("HeadTiltLeft", inputService.PressedMovements);
        }

        [Fact]
        public async Task MouthOpen_ShouldTriggerCorrespondingKeyPress()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ISettingsManager, SettingsManager>();
            services.AddSingleton<SettingsModel>();
            services.AddSingleton<OutputViewModel>();
            services.AddSingleton<MockInputService>();
            var serviceProvider = services.BuildServiceProvider();

            var settingsModel = serviceProvider.GetRequiredService<SettingsModel>();
            var inputService = serviceProvider.GetRequiredService<MockInputService>();
            var tracker = new MockTracker(inputService, settingsModel);
            
            // Create facial tracking data representing an open mouth
            var trackingData = new FacialTrackingData
            {
                MouthTopY = 0.3,
                MouthBotY = 0.7
                
            };

            tracker.ProcessTrackingData(trackingData);
            await Task.Delay(100); // Allow processing time

            Assert.Contains("MouthOpen", inputService.PressedMovements);
        }
    }

    // Mock classes to support testing
    public class MockInputService : IInputService
    {
        public List<string> PressedMovements { get; } = new List<string>();

        public void SimulateKeyDown(string keyName, string movementName)
        {
            PressedMovements.Add(movementName);
        }

        public void ReleaseKey(string keyName, string movementName) {}
        public void MoveMouseRelative(int x, int y)
        {
            throw new NotImplementedException();
        }

        public void MouseDown(bool isRight)
        {
            throw new NotImplementedException();
        }

        public void MouseUp(bool isRight)
        {
            throw new NotImplementedException();
        }

        public void ScrollMouse(int amount)
        {
            throw new NotImplementedException();
        }


    }

    public class MockTracker
    {
        private readonly IInputService _inputService;
        private readonly SettingsModel _settings;

        public MockTracker(IInputService inputService, SettingsModel settings)
        {
            _inputService = inputService;
            _settings = settings;
        }

        public void ProcessTrackingData(FacialTrackingData data)
        {
            if (data.CalculateHeadTiltAngle() > 15)
                _inputService.SimulateKeyDown(_settings.Settings["HeadTiltLeft"].Key, "HeadTiltLeft");
                
            if (data.MouthHeight > 0.3)
                _inputService.SimulateKeyDown(_settings.Settings["MouthOpen"].Key, "MouthOpen");
        }
    }
}