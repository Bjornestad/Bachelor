using System;
using System.Text.Json.Serialization;

namespace Bachelor.Models;

public class FacialTrackingData
{
    
    [JsonPropertyName("rEyeCornerY")]
    public double rEyeCornerY { get; set; }

    [JsonPropertyName("lEyeCornerY")]
    public double lEyeCornerY { get; set; }
    
    //Left eyebrow up and down-------------------------------
    [JsonPropertyName("lEyebrowY")]
    public double lEyebrowY { get; set; }

    [JsonPropertyName("lEyesocketY")]
    public double lEyesocketY { get; set; }
    public double LeftEyebrowHeight => lEyebrowY- lEyesocketY;
    //---------------------------------------------------
    
    //Right eyebrow up and down-------------------------------
    [JsonPropertyName("rEyebrowY")]
    public double rEyebrowY { get; set; }

    [JsonPropertyName("rEyesocketY")]
    public double rEyesocketY { get; set; }
    public double RightEyebrowHeight => rEyebrowY - rEyesocketY;
    //---------------------------------------------------
    
    //Mouth open and close-------------------------------
    [JsonPropertyName("MouthBotY")]
    public double MouthBotY { get; set; }

    [JsonPropertyName("MouthTopY")]
    public double MouthTopY { get; set; }
    public double MouthHeight => MouthBotY - MouthTopY;
    //---------------------------------------------------

    //Mouth widening and narrowing-------------------------------
    [JsonPropertyName("MouthRX")]
    public double MouthRX { get; set; }

    [JsonPropertyName("MouthLX")]
    public double MouthLX { get; set; }
    public double MouthWidth => MouthLX - MouthRX;
    //---------------------------------------------------

    
    [JsonPropertyName("rEarZ")]
    public double rEarZ { get; set; }

    [JsonPropertyName("lEarZ")]
    public double lEarZ { get; set; }
    
    
    //Head pitch roll things--------------------------------
    [JsonPropertyName("ForeheadZ")]
    public double ForeheadZ { get; set; }
    [JsonPropertyName("ChinZ")]
    public double ChinZ { get; set; }
    [JsonPropertyName("rEyeCornerZ")]
    public double rEyeCornerZ { get; set; }
    [JsonPropertyName("lEyeCornerZ")]
    public double lEyeCornerZ { get; set; }
    //---------------------------------------------------
    
    
    
    //Disable roll or rotation based on users input as to not overlap inputs
    public double Roll => CalculateHeadTiltAngle();
    public double HeadRotation => SelectiveHeadRotation();
    
    //head pitch calc
    public double HeadPitch => ((ForeheadZ + ChinZ) / 2) - ((rEyeCornerZ + lEyeCornerZ) / 2);  
    
    // calculate head tilt angle in degrees
    public double CalculateHeadTiltAngle()
    {
        // Calculate the difference in y-coordinates
        double deltaY = lEyeCornerY - rEyeCornerY;
        // Calculate the angle in radians
        double angleInRadians = Math.Atan2(deltaY, 1); // Assuming a horizontal distance of 1 unit
        // Convert the angle to degrees
        double angleInDegrees = angleInRadians * (180.0 / Math.PI);
        return angleInDegrees;
    }
    // calculate head rotation angle
    public double SelectiveHeadRotation()
    {
        // Calculate the difference in x-coordinates
        double deltaX = lEarZ - rEarZ;
        // Calculate the angle in radians
        double angleInRadians = Math.Atan2(deltaX, 1); // Assuming a horizontal distance of 1 unit
        // Convert the angle to degrees
        double angleInDegrees = angleInRadians * (180.0 / Math.PI);
        return angleInDegrees;
    }
}