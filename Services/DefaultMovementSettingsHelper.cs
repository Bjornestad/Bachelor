using System.Collections.Generic;

namespace Bachelor.Services;

public static class DefaultMovementSettingsHelper
{
   public static Dictionary<string, MovementManagerService.MovementSetting> CreateDefaultSettings()
   {
       return new Dictionary<string, MovementManagerService.MovementSetting>
       {
            ["HeadTiltLeft"] = new MovementManagerService.MovementSetting {
                Key = "Q", 
                Threshold = 10, 
                Sensitivity = 0.5,
                Coordinate = "Roll",
                Direction = "Negative",
                Enabled = true,
                Continuous = true,
                MouseActionType = "None"
            },
            ["HeadTiltRight"] = new MovementManagerService.MovementSetting {
                Key = "E", 
                Threshold = 10, 
                Sensitivity = 0.5,
                Coordinate = "Roll",
                Direction = "Positive",
                Enabled = true,
                Continuous = true,
                MouseActionType = "None"
            },
            ["MouthOpen"] = new MovementManagerService.MovementSetting {
                Key = "Space", 
                Threshold = 0.1, 
                Sensitivity = 0.5,
                Coordinate = "MouthHeight",
                Direction = "Positive",
                Enabled = true,
                Continuous = false,
                MouseActionType = "None"
            },
            ["MouthWide"] = new MovementManagerService.MovementSetting {
                Key = "Enter", 
                Threshold = 0.3, 
                Sensitivity = 2.0,
                Coordinate = "MouthWidth",
                Direction = "Positive",
                Enabled = true,
                Continuous = false,
                MouseActionType = "None"
            },
            ["HeadLeft"] = new MovementManagerService.MovementSetting {
                Key = "Left", 
                Threshold = 20, 
                Sensitivity = 1.0,
                Coordinate = "HeadRotation",
                Direction = "Negative",
                Enabled = true,
                Continuous = true,
                MouseActionType = "None"
            },
            ["HeadRight"] = new MovementManagerService.MovementSetting {
                Key = "Right", 
                Threshold = 20, 
                Sensitivity = 1.0,
                Coordinate = "HeadRotation",
                Direction = "Positive",
                Enabled = true,
                Continuous = true,
                MouseActionType = "None"
            }
       };
   }
}