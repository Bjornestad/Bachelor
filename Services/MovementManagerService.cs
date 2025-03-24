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
        // Load settings from JSON
        string json = File.ReadAllText("Assets/DefaultSettings/DefaultSetting.json");
        _settings = JsonSerializer.Deserialize<Dictionary<string, MovementSetting>>(json);

        // Add coordinate and direction information 
        SetDefaultDirectionsAndCoordinates();
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