using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia.Input;
using Bachelor.Models;

namespace Bachelor.Services;

public class MovementManagerService
{
    private readonly Dictionary<string, MovementSetting> _settings;
    private FacialTrackingData _previousData;
    
    public class MovementSetting
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public double Threshold { get; set; }
        public double Sensitivity { get; set; }
        public string Coordinate { get; set; } // X, Y, Z, LeftEyebrow, RightEyebrow, MouthWidth, MouthHeight
        public string Direction { get; set; }
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
            _settings = CreateDefaultSettings();
            string defaultJson = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsPath, defaultJson);
        }
        else
        {
            // Load settings from JSON
            string json = File.ReadAllText(settingsPath);
            _settings = JsonSerializer.Deserialize<Dictionary<string, MovementSetting>>(json);
        }

        // Add coordinate and direction information
        SetDefaultDirectionsAndCoordinates();
    }

    private Dictionary<string, MovementSetting> CreateDefaultSettings()
    {
        return new Dictionary<string, MovementSetting>
        {
            ["HeadLeft"] = new MovementSetting { Id = 1, Key = "Left", Threshold = 0.15, Sensitivity = 1.0 },
            ["HeadRight"] = new MovementSetting { Id = 2, Key = "Right", Threshold = 0.15, Sensitivity = 1.0 },
            ["LeftEyebrowRaise"] = new MovementSetting { Id = 3, Key = "Up", Threshold = 0.1, Sensitivity = 1.0 },
            ["RightEyebrowRaise"] = new MovementSetting { Id = 4, Key = "Down", Threshold = 0.1, Sensitivity = 1.0 },
            ["MouthOpen"] = new MovementSetting { Id = 5, Key = "Space", Threshold = 0.2, Sensitivity = 1.0 },
            ["MouthWide"] = new MovementSetting { Id = 6, Key = "Enter", Threshold = 0.2, Sensitivity = 1.0 }
        };
    }
    private void SetDefaultDirectionsAndCoordinates()
    {
        // Head movements (existing)
        if (_settings.ContainsKey("HeadLeft")) {
            _settings["HeadLeft"].Coordinate = "X";
            _settings["HeadLeft"].Direction = "Negative";
        }
        
        if (_settings.ContainsKey("HeadRight")) {
            _settings["HeadRight"].Coordinate = "X";
            _settings["HeadRight"].Direction = "Positive";
        }
        
        // Add eyebrow movements
        if (_settings.ContainsKey("LeftEyebrowRaise")) {
            _settings["LeftEyebrowRaise"].Coordinate = "LeftEyebrow";
            _settings["LeftEyebrowRaise"].Direction = "Positive";
        }
        
        if (_settings.ContainsKey("RightEyebrowRaise")) {
            _settings["RightEyebrowRaise"].Coordinate = "RightEyebrow";
            _settings["RightEyebrowRaise"].Direction = "Positive";
        }
        
        // Add mouth movements
        if (_settings.ContainsKey("MouthOpen")) {
            _settings["MouthOpen"].Coordinate = "MouthHeight";
            _settings["MouthOpen"].Direction = "Positive";
        }
        
        if (_settings.ContainsKey("MouthWide")) {
            _settings["MouthWide"].Coordinate = "MouthWidth";
            _settings["MouthWide"].Direction = "Positive";
        }
    }
    public void ProcessFacialData(FacialTrackingData data)
    {
        if (_previousData == null)
        {
            _previousData = data;
            return;
        }

        foreach (var settingPair in _settings)
        {
            var setting = settingPair.Value;

            double currentValue = GetCoordinateValue(data, setting.Coordinate);
            double previousValue = GetCoordinateValue(_previousData, setting.Coordinate);
            double difference = currentValue - previousValue;

            bool shouldTrigger = setting.Direction == "Positive"
                ? difference > setting.Threshold * setting.Sensitivity
                : difference < -setting.Threshold * setting.Sensitivity;

            if (shouldTrigger)
            {
                SimulateKeyPress(setting.Key);
            }
        }

        _previousData = data;
    }
    private double GetCoordinateValue(FacialTrackingData data, string coordinate)
    {
        return coordinate switch
        {
            "X" => data.X,
            "Y" => data.Y,
            "Z" => data.Z,
            "LeftEyebrow" => data.LeftEyebrowHeight,
            "RightEyebrow" => data.RightEyebrowHeight,
            "MouthWidth" => data.MouthWidth,
            "MouthHeight" => data.MouthHeight,
            _ => 0.0
        };
    }
    private void SimulateKeyPress(string keyName)
    {
        if (Enum.TryParse<Key>(keyName, out var key))
        {
            Console.WriteLine($"Pressing key: {key}");
        }
    }
}