using Bachelor.Models;
using Bachelor.Services;
using Bachelor.ViewModels;
using Moq;
using System.Reflection;
using Bachelor.Interfaces;

namespace Bachelor.Test.UnitTest.ServicesTest
{
    public class MovementManagerServiceTest
    {
        private readonly Mock<IInputService> _mockInputService;
        private readonly OutputViewModel _outputViewModel;
        private readonly Mock<ISettingsManager> _mockSettingsManager;
        private readonly Dictionary<string, MovementManagerService.MovementSetting> _testSettings;

        public MovementManagerServiceTest()
        {
            _mockInputService = new Mock<IInputService>();
            _outputViewModel = new OutputViewModel();
            _mockSettingsManager = new Mock<ISettingsManager>();

            // Create test settings
            _testSettings = new Dictionary<string, MovementManagerService.MovementSetting>
            {
                ["TestMovement1"] = new MovementManagerService.MovementSetting
                {
                    Key = "A",
                    Coordinate = "MouthHeight",
                    Direction = "Positive",
                    Enabled = true,
                    Sensitivity = 2.0,
                    Threshold = 0.5,
                    Continuous = false,
                    MouseActionType = "None",
                    DisplayName = "Test Movement 1",
                    InstructionImage = "test1.png"
                }
            };

            _mockSettingsManager.Setup(m => m.GetAllSettings()).Returns(_testSettings);
        }

        [Fact]
        public void Constructor_LoadsSettingsFromManager()
        {
            var manager = new MovementManagerService(
                _mockInputService.Object,
                _outputViewModel,
                _mockSettingsManager.Object);

            _mockSettingsManager.Verify(m => m.GetAllSettings(), Times.Once);
        }

        [Fact]
        public void RefreshSettings_UpdatesSettingsFromManager()
        {
            var manager = new MovementManagerService(
                _mockInputService.Object,
                _outputViewModel,
                _mockSettingsManager.Object);

            manager.RefreshSettings();

            _mockSettingsManager.Verify(m => m.GetAllSettings(), Times.Exactly(2));
        }

        [Fact]
        public void ProcessFacialData_TriggersAction_WhenThresholdExceeded()
        {
            var manager = new MovementManagerService(
                _mockInputService.Object,
                _outputViewModel,
                _mockSettingsManager.Object);

            var baselineData = new FacialTrackingData
            {
                MouthBotY = 0.5,
                MouthTopY = 0.5 
            };
            
            manager.ProcessFacialData(baselineData);
            manager.CalibrateNormalization(baselineData);

            _mockInputService.Invocations.Clear();

            var triggerData = new FacialTrackingData
            {
                MouthBotY = 1.5,
                MouthTopY = 0.5 
            };

            manager.ProcessFacialData(triggerData);

            _mockInputService.Verify(m => m.SimulateKeyDown("A", "TestMovement1"), Times.AtLeastOnce());
            Assert.True(_mockInputService.Invocations.Count > 0, "No input service methods were called");
        }

        [Fact]
        public void ProcessFacialData_TriggersOnlyReleaseKey_WhenThresholdNotExceeded()
        {
            var manager = new MovementManagerService(
                _mockInputService.Object,
                _outputViewModel,
                _mockSettingsManager.Object);

            var baselineData = new FacialTrackingData
            {
                MouthBotY = 0.5,
                MouthTopY = 0.5 
            };

            manager.ProcessFacialData(baselineData);
            manager.CalibrateNormalization(baselineData);

            _mockInputService.Invocations.Clear();

            var triggerData = new FacialTrackingData
            {
                MouthBotY = 0.51,
                MouthTopY = 0.5 
            };

            manager.ProcessFacialData(triggerData);

            // Verify that SimulateKeyDown is never called
            _mockInputService.Verify(m => m.SimulateKeyDown(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
            // But verify that ReleaseKey is called due to how behavior in mms is setup
            _mockInputService.Verify(m => m.ReleaseKey("A", "TestMovement1"), Times.Once());

        }
    }
}