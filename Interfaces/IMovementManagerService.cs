using Bachelor.Models;

namespace Bachelor.Interfaces;

public interface IMovementManagerService
{
    void ProcessFacialData(FacialTrackingData data);
    void RefreshSettings();
    void CalibrateNormalization(FacialTrackingData neutralData);
    void CheckInputTimeout();

}