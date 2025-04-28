using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bachelor.Models;
using Bachelor.ViewModels;
using System.ComponentModel; 

namespace Bachelor.Services;

public class MovementManagerService
{
    private Dictionary<string, MovementSetting> _settings;
    private FacialTrackingData _previousData;
    private Dictionary<string, bool> _movementStates = new Dictionary<string, bool>();
    private Dictionary<string, double> _normalizedOffsets = new Dictionary<string, double>();
    private bool _hasCalibrated = false;
    private bool debug = false;
    private DateTime _lastPrintTime = DateTime.MinValue;
    private readonly TimeSpan _printInterval = TimeSpan.FromMilliseconds(500);
    private int _dataPointsBeforeCalibration = 10;
    private int _dataPointsReceived = 0;
    private DateTime _lastInputTime = DateTime.Now;
    private bool _inputActive = false;
    private readonly TimeSpan _inputTimeout = TimeSpan.FromSeconds(1.5);
    private bool _hasReportedStop = false;
    private readonly InputService _inputService;
    private readonly OutputViewModel _outputViewModel;
    private SettingsManager _settingsManager;
    
    public class MovementSetting
    {
        public string Key { get; set; }
        public double Threshold { get; set; }
        public double Sensitivity { get; set; }
        public string Coordinate { get; set; }
        public string Direction { get; set; }
        public bool Enabled { get; set; }
        public bool Continuous { get; set; }
        public string MouseActionType { get; set; }
        public string DisplayName { get; set; }

    }
    
    public static class MouseActionTypes
    {
        public static readonly List<string> List = new List<string>
        {
            "None",
            "MoveX",
            "MoveY",
            "LeftClick",
            "RightClick",
            "Scroll"
        };
    }
    
    
    
    public MovementManagerService(InputService inputService, OutputViewModel outputViewModel, SettingsManager settingsManager)
    {
        _inputService = inputService;
        _outputViewModel = outputViewModel;
        _settingsManager = settingsManager;
        
        // Load settings from the SettingsManager
        _settings = _settingsManager.GetAllSettings();
        
        //To see if settings load correctly
        foreach (var entry in _settings)
        {
            Console.WriteLine($"Loaded setting: {entry.Key}, coordinate={entry.Value.Coordinate}, enabled={entry.Value.Enabled}, sens={entry.Value.Sensitivity}, threshold={entry.Value.Threshold}, mouse={entry.Value.MouseActionType}");
        }
    }
    
    public void RefreshSettings()
    {
        _settings = _settingsManager.GetAllSettings();
        Console.WriteLine("Settings refreshed from SettingsManager");
    }
    
    public void ProcessFacialData(FacialTrackingData data)
    {
        
        _lastInputTime = DateTime.Now;
    
        if (!_inputActive)
        {
            _inputActive = true;
            Console.WriteLine("Input started - receiving facial data");
            _outputViewModel?.Log("Input started - receiving facial data");
        }
        
        if (_previousData == null)
        {
            Console.WriteLine("First data point - storing as baseline");
            _outputViewModel?.Log("First data point - storing as baseline");
            _previousData = data;
            return;
        }
        
        _dataPointsReceived++;
        if (!_hasCalibrated && _dataPointsReceived >= _dataPointsBeforeCalibration)
        {
            Console.WriteLine("Auto-calibrating after receiving enough data...");
            CalibrateNormalization(data);
            //TODO make button to reset calibration
        }
        //Console.WriteLine($"Calibration status: {(_hasCalibrated ? "Calibrated" : "Not calibrated")}");

        if (debug && _hasCalibrated && DateTime.Now - _lastPrintTime > _printInterval)
        {
            Console.WriteLine("--- NORMALIZED VALUES ---");
            Console.WriteLine($"{GetCoordinateValue(data, new MovementSetting { Coordinate = "MouthHeight" }):F3} : Normalized Mouth openness");
            Console.WriteLine($"{GetCoordinateValue(data, new MovementSetting { Coordinate = "MouthWidth" }):F3} : Normalized Mouth width");
            Console.WriteLine($"{GetCoordinateValue(data, new MovementSetting { Coordinate = "RightEyebrowHeight" }):F3} : Normalized EyebrowR height");
            Console.WriteLine($"{GetCoordinateValue(data, new MovementSetting { Coordinate = "LeftEyebrowHeight" }):F3} : Normalized EyebrowL height");
            Console.WriteLine($"{GetCoordinateValue(data, new MovementSetting { Coordinate = "Roll" }):F3} : Normalized Head tilt");
            Console.WriteLine($"{GetCoordinateValue(data, new MovementSetting { Coordinate = "HeadRotation" }):F3} : Normalized Head rotation");
            Console.WriteLine($"{GetCoordinateValue(data, new MovementSetting { Coordinate = "HeadPitch" }):F3} : Normalized Head pitch");
            _lastPrintTime = DateTime.Now;
        }

        // Process each configured movement
        foreach (var entry in _settings)
        {
            string movementName = entry.Key;
            MovementSetting setting = entry.Value;

            // Skip disabled movements
            if (!setting.Enabled)
                continue;

            // Get the current value for this movement's coordinate
            double value = GetCoordinateValue(data, setting);

            // Apply sensitivity directly to the absolute value
            double adjustedValue = value * setting.Sensitivity;
            
            // Debug output for head movements
            /*
            if (movementName.ToLower().Contains("head") || movementName.ToLower().Contains("tilt"))
            {
                Console.WriteLine($"{movementName}: raw={value:F3}, adjusted={adjustedValue:F3}, threshold={setting.Threshold:F3}");
                Console.WriteLine($"  Would trigger: {(setting.Direction == "Positive" ? adjustedValue >= setting.Threshold : adjustedValue <= -setting.Threshold)}");
            }*/

            // Check if movement should trigger based on threshold and direction
            bool shouldTrigger = false;
            if (setting.Direction == "Positive" && adjustedValue >= setting.Threshold)
            {
                shouldTrigger = true;
            }
            else if (setting.Direction == "Negative" && adjustedValue <= -setting.Threshold)
            {
                shouldTrigger = true;
            }

            if (shouldTrigger)
            {
                // For continuous movements, trigger constantly while active
                if (setting.Continuous && setting.MouseActionType == "None")
                {
                    SimulateKeyPress(setting.Key, movementName);
                }
                // For non-continuous movements, only trigger on state change
                else if ((!_movementStates.ContainsKey(movementName) || !_movementStates[movementName])
                         && setting.MouseActionType == "None")
                {
                    SimulateKeyPress(setting.Key, movementName);
                }
                else if (setting.MouseActionType != "None")
                {
                    switch (setting.MouseActionType)
                    {
                        case "MoveX":
                            _inputService.MoveMouseRelative((int)(adjustedValue * setting.Sensitivity), 0);
                            break;
                        case "MoveY":
                            _inputService.MoveMouseRelative(0, (int)(adjustedValue * setting.Sensitivity));
                            break;
                        case "LeftClick":
                            if (!_movementStates.ContainsKey(movementName) || !_movementStates[movementName])
                            {
                                _inputService.MouseDown(false); // false = left button
                                _inputService.MouseUp(false);
                                _outputViewModel?.Log($"Mouse left button down | Movement: {movementName}");
                            }
                            break;
                        case "RightClick":
                            if (!_movementStates.ContainsKey(movementName) || !_movementStates[movementName])
                            {
                                _inputService.MouseDown(true); // true = right button
                                _inputService.MouseUp(true);
                                _outputViewModel?.Log($"Mouse right button down | Movement: {movementName}");
                            }
                            break;
                        case "Scroll":
                            _inputService.ScrollMouse((int)(adjustedValue * setting.Sensitivity));
                            break;
                    }
                }

                _movementStates[movementName] = true;
            }
            else
            {
                _movementStates[movementName] = false;
            }
        }
        foreach (var entry in _settings)
        {
            string movementName = entry.Key;
            MovementSetting setting = entry.Value;

            // If movement is no longer active, release its key
            if (!_movementStates.ContainsKey(movementName) || !_movementStates[movementName])
            {
                _inputService.ReleaseKey(setting.Key, movementName);
            }
        }

        // Save current data for next comparison
        _previousData = data;
    }
    
    public void CalibrateNormalization(FacialTrackingData neutralData)
    {
        _normalizedOffsets.Clear();
        
        // Store the offsets for all relevant properties
        _normalizedOffsets["MouthHeight"] = neutralData.MouthHeight;
        _normalizedOffsets["MouthWidth"] = neutralData.MouthWidth;
        _normalizedOffsets["LeftEyebrowHeight"] = neutralData.LeftEyebrowHeight;
        _normalizedOffsets["RightEyebrowHeight"] = neutralData.RightEyebrowHeight;
        _normalizedOffsets["Roll"] = neutralData.Roll;
        _normalizedOffsets["HeadRotation"] = neutralData.HeadRotation;
        _normalizedOffsets["HeadPitch"] = neutralData.HeadPitch;
        
        _hasCalibrated = true;
        Console.WriteLine("Normalization calibration complete");
    }

    private double GetCoordinateValue(FacialTrackingData data, MovementSetting setting)
    {
        // Add safety check for empty coordinate
        if (string.IsNullOrEmpty(setting.Coordinate))
        {
            Console.WriteLine($"Warning: Empty coordinate for setting");
            return 0.0;
        }

        var property = typeof(FacialTrackingData).GetProperty(setting.Coordinate);
        if (property != null && property.PropertyType == typeof(double))
        {
            double value = (double)property.GetValue(data);

            // Apply normalization if calibrated
            if (_hasCalibrated && _normalizedOffsets.ContainsKey(setting.Coordinate))
            {
                value -= _normalizedOffsets[setting.Coordinate];
            }

            return value;
        }
        else
        {
            Console.WriteLine($"Warning: Could not find property {setting.Coordinate} on FacialTrackingData");
            return 0.0;
        }
    }
    
    public void CheckInputTimeout()
    {
        if (_inputActive && DateTime.Now - _lastInputTime > _inputTimeout)
        {
            Console.WriteLine("Input stopped - no facial data received");
            _outputViewModel?.Log("Input stopped - no facial data received");
            _inputActive = false;
        }
    }

    private void SimulateKeyPress(string keyName, string movementName)
    {
        // Reset input timing
        _lastInputTime = DateTime.Now;
        if (_hasReportedStop)
        {
            Console.WriteLine("Input resumed - receiving facial data again");
            _outputViewModel?.Log("Input resumed - receiving facial data again");
            _hasReportedStop = false;
        }

        if (_settings.TryGetValue(movementName, out var setting) && !setting.Continuous)
        {
            _inputService.SimulateKeyDown(keyName, movementName);

            Task.Run(async () =>
            {
                await Task.Delay(50); // Short delay to ensure key press registers
                _inputService.ReleaseKey(keyName, movementName);
            });
        }
        else
        {
            _inputService.SimulateKeyDown(keyName, movementName);
        }
    }
}