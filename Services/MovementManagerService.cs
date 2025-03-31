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
            string defaultJson = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
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
        //todo make something that actually works
        
    }
    
    private double GetCoordinateValue(FacialTrackingData data, string coordinate)
    {
        return coordinate switch
        {
            "X" => data.X,
            "Y" => data.Y,
            "Z" => data.Z,
            "Roll" => data.Roll,
            "LeftEyebrow" => data.LeftEyebrowHeight,
            "RightEyebrow" => data.RightEyebrowHeight,
            "MouthWidth" => data.MouthWidth,
            "MouthHeight" => data.MouthHeight,
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