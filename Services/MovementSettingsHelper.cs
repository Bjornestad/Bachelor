using System.Collections.Generic;

namespace Bachelor.Services;

public static class MovementSettingsHelper
{
   public static Dictionary<string, MovementManagerService.MovementSetting> CreateDefaultSettings()
   {
       return new Dictionary<string, MovementManagerService.MovementSetting>
       {
            ["HeadTiltLeft"] = new MovementManagerService.MovementSetting {
                Key = "Q", 
                Threshold = 0.2, 
                Sensitivity = 0.5,
                Coordinate = "Roll",
                Direction = "Negative",
                Enabled = true,
                Continuous = true
            },
            ["HeadTiltRight"] = new MovementManagerService.MovementSetting {
                Key = "E", 
                Threshold = 0.2, 
                Sensitivity = 3.0,
                Coordinate = "Roll",
                Direction = "Positive",
                Enabled = true,
                Continuous = true
            },
            ["MouthOpen"] = new MovementManagerService.MovementSetting {
                Key = "Space", 
                Threshold = 0.1, 
                Sensitivity = 0.5,
                Coordinate = "MouthHeight",
                Direction = "Positive",
                Enabled = true,
                Continuous = false
            },
            ["MouthWide"] = new MovementManagerService.MovementSetting {
                Key = "Enter", 
                Threshold = 0.3, 
                Sensitivity = 2.0,
                Coordinate = "MouthWidth",
                Direction = "Positive",
                Enabled = true,
                Continuous = false
            },
            ["HeadLeft"] = new MovementManagerService.MovementSetting {
                Key = "Left", 
                Threshold = 0.15, 
                Sensitivity = 1.0,
                Coordinate = "HeadRotation",
                Direction = "Negative",
                Enabled = true,
                Continuous = true
            },
            ["HeadRight"] = new MovementManagerService.MovementSetting {
                Key = "Right", 
                Threshold = 0.15, 
                Sensitivity = 1.0,
                Coordinate = "HeadRotation",
                Direction = "Positive",
                Enabled = true,
                Continuous = true
            }
       };
   }
}