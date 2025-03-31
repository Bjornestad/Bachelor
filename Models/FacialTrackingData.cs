using System;
using System.Runtime.InteropServices.JavaScript;

namespace Bachelor.Models;

public class FacialTrackingData
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double Roll { get; set; }
    public double LeftEyebrowHeight { get; set; }
    public double RightEyebrowHeight { get; set; }
    public double MouthHeight { get; set; }
    public double MouthWidth { get; set; }
    public double HeadRotation { get; set; } 
}
