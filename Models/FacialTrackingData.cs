using System;
using System.Text.Json.Serialization;

namespace Bachelor.Models;

public class FacialTrackingData
{
    [JsonPropertyName("4x")]
    public double X { get; set; }

    [JsonPropertyName("4y")]
    public double Y { get; set; }

    [JsonPropertyName("4z")]
    public double Z { get; set; }

    // Calculated properties
    public double Roll => lEyeCornerY - rEyeCornerY;

    [JsonPropertyName("rEyeCornerY")]
    public double rEyeCornerY { get; set; }

    [JsonPropertyName("lEyeCornerY")]
    public double lEyeCornerY { get; set; }

    public double LeftEyebrowHeight => lEyesocketY - lEyebrowY;
    
    public double RightEyebrowHeight => rEyesocketY - rEyebrowY;

    [JsonPropertyName("lEyebrowY")]
    public double lEyebrowY { get; set; }

    [JsonPropertyName("lEyesocketY")]
    public double lEyesocketY { get; set; }

    [JsonPropertyName("rEyebrowY")]
    public double rEyebrowY { get; set; }

    [JsonPropertyName("rEyesocketY")]
    public double rEyesocketY { get; set; }

    public double MouthHeight => MouthBotY - MouthTopY;
    
    public double MouthWidth => MouthLX - MouthRX;

    [JsonPropertyName("MouthBotY")]
    public double MouthBotY { get; set; }

    [JsonPropertyName("MouthTopY")]
    public double MouthTopY { get; set; }

    [JsonPropertyName("MouthRX")]
    public double MouthRX { get; set; }

    [JsonPropertyName("MouthLX")]
    public double MouthLX { get; set; }

    public double HeadRotation => lEarZ - rEarZ;

    [JsonPropertyName("rEarZ")]
    public double rEarZ { get; set; }

    [JsonPropertyName("lEarZ")]
    public double lEarZ { get; set; }
}