using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;
using Bachelor.Models;
using Bachelor.Services;
using Bachelor.ViewModels;
using Bachelor.Interfaces;
using InputSimulatorStandard.Native;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Bachelor.Test.IntegrationTest
{
    public class InputSimulationIntegrationTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public InputSimulationIntegrationTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task HeadTiltLeft_ShouldTriggerCorrespondingKeyPress()
        {
            // Existing test code...
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
                MouthTopY = 0.3,
                MouthBotY = 0.7
            };

            tracker.ProcessTrackingData(trackingData);
            await Task.Delay(100); // Allow processing time

            Assert.Contains("MouthOpen", inputService.PressedMovements);
        }

        [Fact]
        public async Task KeyboardInput_ShouldActuallyTriggerOsEvents()
        {
            if (!OperatingSystem.IsWindows())
            {
                _testOutputHelper.WriteLine("Test skipped: Only runs on Windows");
                return;
            }

            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<OutputViewModel>();
            services.AddSingleton<IInputService, TestableInputService>();
            services.AddSingleton<ISettingsManager, SettingsManager>();
            services.AddSingleton<SettingsModel>();

            var serviceProvider = services.BuildServiceProvider();
            var inputService = serviceProvider.GetRequiredService<IInputService>() as TestableInputService;
    
            using var detector = new KeyboardDetector(VirtualKeyCode.VK_A);
    
            try
            {
                _testOutputHelper.WriteLine("Simulating key press for 'A'");
                inputService.SimulateKeyDown("A", "TestMovement");
        
                await Task.Delay(500);
        
                Assert.True(inputService.WasKeyPressed("TestMovement:A"),
                    "Key press was not registered in InputService");
            
                if (Environment.GetEnvironmentVariable("CI") != null)
                {
                    _testOutputHelper.WriteLine("Skipping OS-level key detection check in CI environment");
                    return;
                }
        
                Assert.True(detector.WasKeyDetected(), 
                    "Key press was not detected at OS level");
            }
            finally
            {
                inputService.ReleaseAllKeys();
            }
        }
        
        [Fact]
        public async Task MouseMovement_ShouldActuallyTriggerOsEvents()
        {
            if (!OperatingSystem.IsWindows())
            {
                _testOutputHelper.WriteLine("Test skipped: Only runs on Windows");
                return;
            }

            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<OutputViewModel>();
            services.AddSingleton<IInputService, TestableInputService>();
            services.AddSingleton<ISettingsManager, SettingsManager>();
            services.AddSingleton<SettingsModel>();

            var serviceProvider = services.BuildServiceProvider();
            var inputService = serviceProvider.GetRequiredService<IInputService>() as TestableInputService;

            using var detector = new MouseDetector(_testOutputHelper);

            try
            {
                detector.ResetDetection(); // Reset detection state
    
                _testOutputHelper.WriteLine("Simulating mouse movement");
                inputService.MoveMouseRelative(50, 50);
                await Task.Delay(1000);

                bool positionChanged = detector.CheckForPositionChange();
                _testOutputHelper.WriteLine($"Position changed: {positionChanged}");

                if (Environment.GetEnvironmentVariable("CI") != null)
                {
                    _testOutputHelper.WriteLine("Skipping OS-level mouse detection check in CI environment");
                    return;
                }

                Assert.True(detector.WasMouseMovementDetected(),
                    "Mouse movement was not detected at OS level");
            }
            finally
            {
                inputService.ReleaseAllKeys();
            }
        }

        public class MockInputService : IInputService
        {
            public List<string> PressedMovements { get; } = new List<string>();

            public void SimulateKeyDown(string keyName, string movementName)
            {
                PressedMovements.Add(movementName);
            }

            public void ReleaseKey(string keyName, string movementName)
            {
            }

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
    

    public class TestableInputService : InputService
    {
        private Dictionary<string, bool> _keysPressedInSystem = new Dictionary<string, bool>();
    
        public Action OnRealKeyPressed { get; set; }

        public TestableInputService(OutputViewModel outputViewModel) : base(outputViewModel)
        {
        }

        public override void SimulateKeyDown(string keyName, string movementName)
        {
            string keyIdentifier = $"{movementName}:{keyName}";
            _keysPressedInSystem[keyIdentifier] = true;

            base.SimulateKeyDown(keyName, movementName);
        
            OnRealKeyPressed?.Invoke();
        }

        public bool WasKeyPressed(string keyIdentifier)
        {
            return _keysPressedInSystem.ContainsKey(keyIdentifier) && _keysPressedInSystem[keyIdentifier];
        }

        public void ReleaseAllKeys()
        {
            base.ReleaseAllKeys();
            _keysPressedInSystem.Clear();
        }
    }
    
    
    
}
