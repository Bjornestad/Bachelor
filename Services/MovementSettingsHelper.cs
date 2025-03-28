using System.Collections.Generic;

namespace Bachelor.Services;

public static class MovementSettingsHelper
{
   public static Dictionary<string, MovementManagerService.MovementSetting> CreateDefaultSettings()
{
    return new Dictionary<string, MovementManagerService.MovementSetting>
    {
        //set defaults as if it cant find any it crashes, will be overridden by user settings
        ["HeadLeft"] = new MovementManagerService.MovementSetting {
            Key = "Left", 
            Threshold = 0.15, 
            Sensitivity = 1.0,
            Coordinate = "X",
            Direction = "Negative"
        },
        ["HeadRight"] = new MovementManagerService.MovementSetting {
            Key = "Right", 
            Threshold = 0.15, 
            Sensitivity = 1.0,
            Coordinate = "X",
            Direction = "Positive"
        },
        ["LeftEyebrowRaise"] = new MovementManagerService.MovementSetting {
            Key = "Up", 
            Threshold = 0.1, 
            Sensitivity = 1.0,
            Coordinate = "LeftEyebrow",
            Direction = "Positive"
        },
        ["RightEyebrowRaise"] = new MovementManagerService.MovementSetting {
            Key = "Down", 
            Threshold = 0.1, 
            Sensitivity = 1.0,
            Coordinate = "RightEyebrow",
            Direction = "Positive"
        },
        ["MouthOpen"] = new MovementManagerService.MovementSetting {
            Key = "Space", 
            Threshold = 0.2, 
            Sensitivity = 1.0,
            Coordinate = "MouthHeight",
            Direction = "Positive"
        },
        ["MouthWide"] = new MovementManagerService.MovementSetting {
            Key = "Enter", 
            Threshold = 0.2, 
            Sensitivity = 1.0,
            Coordinate = "MouthWidth",
            Direction = "Positive"
        },
        ["HeadTiltLeft"] = new MovementManagerService.MovementSetting {
            Key = "Q", 
            Threshold = 0.1, 
            Sensitivity = 1.0,
            Coordinate = "Roll",
            Direction = "Negative"
        },
        ["HeadTiltRight"] = new MovementManagerService.MovementSetting {
            Key = "E", 
            Threshold = 0.1, 
            Sensitivity = 1.0,
            Coordinate = "Roll",
            Direction = "Positive"
        }
    };
}
}