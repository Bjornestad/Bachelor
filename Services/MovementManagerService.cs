using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using Avalonia.Input;
using Bachelor.Models;

namespace Bachelor.Services;

public class MovementManagerService
{
    private readonly Dictionary<string, MovementSetting> _settings;
    private FacialTrackingData _previousData;
    private FacialTrackingData _calibrationData;
    private Dictionary<string, bool> _movementStates = new Dictionary<string, bool>();


    public class MovementSetting
    {
        public string Key { get; set; }
        public double Threshold { get; set; }
        public double Sensitivity { get; set; }
        public string Coordinate { get; set; }
        public string Direction { get; set; }
        public bool Enabled { get; set; }
        public bool Continuous { get; set; }

        // New properties for relative measurements
        public bool IsRelative { get; set; }
        public string RelativeFrom { get; set; }
        public string RelativeTo { get; set; }
    }

    public MovementManagerService()
    {
        // Set up paths
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string settingsDirectory = Path.Combine(baseDirectory, "Assets", "DefaultSettings");
        string settingsPath = Path.Combine(settingsDirectory, "DefaultSetting.json");

        // Create directory if it doesn't exist
        if (!Directory.Exists(settingsDirectory))
        {
            Directory.CreateDirectory(settingsDirectory);
        }

        // Create default settings file if it doesn't exist
        if (!File.Exists(settingsPath))
        {
            _settings = MovementSettingsHelper.CreateDefaultSettings();
            string defaultJson =
                JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsPath, defaultJson);
        }
        else
        {
            // Load settings from JSON
            string json = File.ReadAllText(settingsPath);
            _settings = JsonSerializer.Deserialize<Dictionary<string, MovementSetting>>(json);
        }

    }

    public void ProcessFacialData(FacialTrackingData data)
    {
        // Initialize previous data if null
        _previousData ??= new FacialTrackingData();

        // Set calibration data if it doesn't exist
        if (_calibrationData == null)
        {
            _calibrationData = data;
            Console.WriteLine("Calibration set");
            return;
        }

        // Process each movement
        foreach (var entry in _settings)
        {
            string movementName = entry.Key;
            MovementSetting setting = entry.Value;

            if (!setting.Enabled)
                continue;

            // Get the current and base values
            double currentValue = GetCoordinateValue(data, setting);
            double baseValue = GetCoordinateValue(_calibrationData, setting);
            double relativeChange = currentValue - baseValue;

            // Apply sensitivity
            double adjustedValue = relativeChange * setting.Sensitivity;

            // Determine if threshold is exceeded based on direction
            bool thresholdExceeded = setting.Direction == "Positive"
                ? adjustedValue > setting.Threshold
                : setting.Direction == "Negative"
                    ? adjustedValue < -setting.Threshold
                    : Math.Abs(adjustedValue) > setting.Threshold;

            // Track movement state
            if (!_movementStates.ContainsKey(movementName))
                _movementStates[movementName] = false;

            // Trigger key press
            if (thresholdExceeded)
            {
                if (setting.Continuous || !_movementStates[movementName])
                {
                    SimulateKeyPress(setting.Key, movementName);
                    _movementStates[movementName] = true;
                }
            }
            else
            {
                _movementStates[movementName] = false;
            }
        }

        _previousData = data;
    }

    private double GetCoordinateValue(FacialTrackingData data, MovementSetting setting)
    {
        // If relative coordinates are specified, calculate the difference
        if (setting.IsRelative && !string.IsNullOrEmpty(setting.RelativeFrom) &&
            !string.IsNullOrEmpty(setting.RelativeTo))
        {
            double fromValue = GetSingleCoordinateValue(data, setting.RelativeFrom);
            double toValue = GetSingleCoordinateValue(data, setting.RelativeTo);
            return toValue - fromValue;
        }

        // Otherwise use the single coordinate
        return GetSingleCoordinateValue(data, setting.Coordinate);
    }

    private double GetSingleCoordinateValue(FacialTrackingData data, string coordinate)
    {
        return coordinate switch
        {
            "NoseX" => data.X,
            "NoseY" => data.Y,
            "NoseZ" => data.Z,
            "rEyeCornerY" => data.rEyeCornerY,
            "lEyeCornerY" => data.lEyeCornerY,
            "lEyebrowY" => data.lEyebrowY,
            "lEyesocketY" => data.lEyesocketY,
            "rEyebrowY" => data.rEyebrowY,
            "rEyesocketY" => data.rEyesocketY,
            "MouthTopY" => data.MouthTopY,
            "MouthBotY" => data.MouthBotY,
            "MouthLX" => data.MouthLX,
            "MouthRX" => data.MouthRX,
            "lEarZ" => data.lEarZ,
            "rEarZ" => data.rEarZ,
            "rEarX" => data.rEarZ,
            "lEarX" => data.lEarZ,

            // Use calculated properties as well
            "Roll" => data.Roll,
            "LeftEyebrowHeight" => data.LeftEyebrowHeight,
            "RightEyebrowHeight" => data.RightEyebrowHeight,
            "MouthHeight" => data.MouthHeight,
            "MouthWidth" => data.MouthWidth,
            "HeadRotation" => data.HeadRotation,
            _ => 0.0
        };
    }


    private void SimulateKeyPress(string keyName, string movementName)
    {
        if (Enum.TryParse<Key>(keyName, out var key))
        {
            Console.WriteLine($"Pressing key: {key} | Movement: {movementName}");
        }
    }
}