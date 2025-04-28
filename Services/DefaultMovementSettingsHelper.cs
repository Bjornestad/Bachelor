using System.Collections.Generic;

namespace Bachelor.Services;

public static class DefaultMovementSettingsHelper
{
   public static Dictionary<string, MovementManagerService.MovementSetting> CreateDefaultSettings()
   {
       return new Dictionary<string, MovementManagerService.MovementSetting>
       {
            ["HeadTiltLeft"] = new()
            {
                Key = "Q", 
                Threshold = 10, 
                Sensitivity = 0.5,
                Coordinate = "Roll",
                Direction = "Negative",
                Enabled = true,
                Continuous = true,
                MouseActionType = "None",
                DisplayName = "Tilt Head Left"
            },
            ["HeadTiltRight"] = new()
            {
                Key = "E", 
                Threshold = 10, 
                Sensitivity = 0.5,
                Coordinate = "Roll",
                Direction = "Positive",
                Enabled = true,
                Continuous = true,
                MouseActionType = "None",
                DisplayName = "Tilt Head Right"
            },
            ["MouthOpen"] = new()
            {
                Key = "Space", 
                Threshold = 0.1, 
                Sensitivity = 0.5,
                Coordinate = "MouthHeight",
                Direction = "Positive",
                Enabled = true,
                Continuous = false,
                MouseActionType = "None",
                DisplayName = "Open Mouth"
            },
            ["MouthWide"] = new()
            {
                Key = "Enter", 
                Threshold = 0.3, 
                Sensitivity = 2.0,
                Coordinate = "MouthWidth",
                Direction = "Positive",
                Enabled = true,
                Continuous = false,
                MouseActionType = "None",
                DisplayName = "Open Mouth Wide"
            },
            ["HeadLeft"] = new()
            {
                Key = "Left", 
                Threshold = 20, 
                Sensitivity = 1.0,
                Coordinate = "HeadRotation",
                Direction = "Negative",
                Enabled = true,
                Continuous = true,
                MouseActionType = "None",
                DisplayName = "Turn Head Left"
            },
            ["HeadRight"] = new()
            {
                Key = "Right", 
                Threshold = 20, 
                Sensitivity = 1.0,
                Coordinate = "HeadRotation",
                Direction = "Positive",
                Enabled = true,
                Continuous = true,
                MouseActionType = "None",
                DisplayName = "Turn Head Right"
            }
       };
   }
}