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
    public double Roll => SelectiveRoll();
    public double HeadRotation => SelectiveHeadRotation();
    
    //head pitch calc
    public double HeadPitch => ((ForeheadZ + ChinZ) / 2) - ((rEyeCornerZ + lEyeCornerZ) / 2);  
    
    //Disable head rotation based on amount of head roll
    private double SelectiveRoll()
    {
        double baseRoll = lEyeCornerY - rEyeCornerY;
        double rotationMagnitude = Math.Abs(lEarZ - rEarZ);
    
        // Threshold approach - if rotation is significant, reduce or disable roll
        if (rotationMagnitude > 0.6) // Strong rotation threshold
            return 0; // Disable roll completely
        else if (rotationMagnitude > 0.15) // Moderate rotation
            return baseRoll * 0.5; // Reduce roll sensitivity
    
        return baseRoll; // Full roll sensitivity
    }
    //Disable roll based on amount of head rotation
    private double SelectiveHeadRotation()
    {
        double baseRotation = lEarZ - rEarZ;
        double rollMagnitude = Math.Abs(lEyeCornerY - rEyeCornerY);
    
        // Threshold approach - if roll is significant, reduce or disable rotation
        if (rollMagnitude > 0.6) // Strong roll threshold
            return 0; // Disable rotation completely
        else if (rollMagnitude > 0.1) // Moderate roll
            return baseRotation * 0.5; // Reduce rotation sensitivity
    
        return baseRotation; // Full rotation sensitivity
    }
}