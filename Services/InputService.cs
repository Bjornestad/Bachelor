using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Avalonia.Input;
using InputSimulatorStandard;
using InputSimulatorStandard.Native;

namespace Bachelor.Services;

public class InputService
{
    private static readonly InputSimulator _simulator = new InputSimulator();
    private Dictionary<string, bool> _keysCurrentlyDown = new Dictionary<string, bool>();
    private readonly TimeSpan _keyPressTimeout = TimeSpan.FromMilliseconds(200);
    
    public void SimulateKeyDown(string keyName, string movementName)
    {
        if (Enum.TryParse<Key>(keyName, out var key))
        {
            VirtualKeyCode vkCode = MapAvaloniaKeyToVirtualKey(key);
            string keyIdentifier = $"{movementName}:{keyName}";
            
            // If key isn't already being held down, press it
            if (!_keysCurrentlyDown.ContainsKey(keyIdentifier) || !_keysCurrentlyDown[keyIdentifier])
            {
                Console.WriteLine($"Pressing key: {keyName} | Movement: {movementName}");
                _simulator.Keyboard.KeyDown(vkCode);
                _keysCurrentlyDown[keyIdentifier] = true;
            }
        }
    }
    
    public void ReleaseKey(string keyName, string movementName)
    {
        if (Enum.TryParse<Key>(keyName, out var key))
        {
            string keyIdentifier = $"{movementName}:{keyName}";
            if (_keysCurrentlyDown.ContainsKey(keyIdentifier) && _keysCurrentlyDown[keyIdentifier])
            {
                VirtualKeyCode vkCode = MapAvaloniaKeyToVirtualKey(key);
                _simulator.Keyboard.KeyUp(vkCode);
                _keysCurrentlyDown[keyIdentifier] = false;
                Console.WriteLine($"Releasing key: {keyName} | Movement: {movementName}");
            }
        }
    }
    
    public void ReleaseAllKeys()
    {
        foreach (var kvp in _keysCurrentlyDown.ToList())
        {
            if (kvp.Value)
            {
                string[] parts = kvp.Key.Split(':');
                if (parts.Length != 2) continue;
                
                string movementName = parts[0];
                string keyName = parts[1];
                
                if (Enum.TryParse<Key>(keyName, out var key))
                {
                    VirtualKeyCode vkCode = MapAvaloniaKeyToVirtualKey(key);
                    _simulator.Keyboard.KeyUp(vkCode);
                    _keysCurrentlyDown[kvp.Key] = false;
                }
            }
        }
        Console.WriteLine("Released all keys");
    }
    
    private VirtualKeyCode MapAvaloniaKeyToVirtualKey(Key key)
    {
        switch (key)
        {
            case Key.A: return VirtualKeyCode.VK_A;
            case Key.D: return VirtualKeyCode.VK_D;
            case Key.W: return VirtualKeyCode.VK_W;
            case Key.S: return VirtualKeyCode.VK_S;
            case Key.Q: return VirtualKeyCode.VK_Q;
            case Key.E: return VirtualKeyCode.VK_E;
            case Key.Left: return VirtualKeyCode.LEFT;
            case Key.Right: return VirtualKeyCode.RIGHT;
            case Key.Up: return VirtualKeyCode.UP;
            case Key.Down: return VirtualKeyCode.DOWN;
            case Key.Space: return VirtualKeyCode.SPACE;
            case Key.Enter: return VirtualKeyCode.RETURN;
            default:
                if (Enum.TryParse<VirtualKeyCode>($"VK_{key}", out var vkCode))
                    return vkCode;
                return VirtualKeyCode.SPACE;
        }
    }
}