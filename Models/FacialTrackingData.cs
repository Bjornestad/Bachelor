namespace Bachelor.Models;

public class FacialTrackingData
{
    //head position 
    public int Id { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double Confidence { get; set; }
    
    //eyebrow features
    public double LeftEyebrowHeight { get; set; }
    public double RightEyebrowHeight { get; set; }
    
    //mouth features
    public double MouthWidth { get; set; }
    public double MouthHeight { get; set; }
    public bool MouthOpen { get; set; }
}
