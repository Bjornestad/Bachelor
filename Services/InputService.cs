using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia.Input;
using Bachelor.ViewModels;
using InputSimulatorStandard;
using InputSimulatorStandard.Native;

namespace Bachelor.Services;

public class InputService
{
    private static readonly InputSimulator _simulator = new InputSimulator();
    private Dictionary<string, bool> _keysCurrentlyDown = new Dictionary<string, bool>();
    private OutputViewModel _outputViewModel;

    public InputService(OutputViewModel outputViewModel)
    {
        _outputViewModel = outputViewModel;
    }
    
    public void SetOutputViewModel(OutputViewModel viewModel)
    {
        _outputViewModel = viewModel;
    }
    
    // macOS CoreGraphics P/Invoke declarations
    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern IntPtr CGEventCreateKeyboardEvent(IntPtr source, ushort keyCode, bool keyDown);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern void CGEventPost(uint tapLocation, IntPtr eventRef);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern void CFRelease(IntPtr cf);

    // Constants for CGEventPost
    private const uint kCGHIDEventTap = 0;
    public void SimulateKeyDown(string keyName, string movementName)
    {
        if (Enum.TryParse<Key>(keyName, out var key))
        {
            string keyIdentifier = $"{movementName}:{keyName}";

            if (!_keysCurrentlyDown.ContainsKey(keyIdentifier) || !_keysCurrentlyDown[keyIdentifier])
            {
                Console.WriteLine($"Pressing key: {keyName} | Movement: {movementName}");
                _outputViewModel?.Log($"Pressing key: {keyName} | Movement: {movementName}");
                if (OperatingSystem.IsWindows())
                {
                    VirtualKeyCode vkCode = MapAvaloniaKeyToVirtualKey(key);
                    _simulator.Keyboard.KeyDown(vkCode);
                }
                else if (OperatingSystem.IsMacOS())
                {
                    ushort macKeyCode = MapAvaloniaKeyToMacKeyCode(key);
                    SimulateMacKeyEvent(macKeyCode, true);
                }

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
                if (OperatingSystem.IsWindows())
                {
                    VirtualKeyCode vkCode = MapAvaloniaKeyToVirtualKey(key);
                    _simulator.Keyboard.KeyUp(vkCode);
                }
                else if (OperatingSystem.IsMacOS())
                {
                    ushort macKeyCode = MapAvaloniaKeyToMacKeyCode(key);
                    SimulateMacKeyEvent(macKeyCode, false);
                }

                _keysCurrentlyDown[keyIdentifier] = false;
                Console.WriteLine($"Releasing key: {keyName} | Movement: {movementName}");
                _outputViewModel?.Log($"Releasing key: {keyName} | Movement: {movementName}");
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

                ReleaseKey(keyName, movementName);
            }
        }
        Console.WriteLine("Released all keys");
    }

    private void SimulateMacKeyEvent(ushort keyCode, bool keyDown)
    {
        IntPtr eventRef = CGEventCreateKeyboardEvent(IntPtr.Zero, keyCode, keyDown);
        CGEventPost(kCGHIDEventTap, eventRef);
        CFRelease(eventRef);
    }
    private static readonly Dictionary<Key, ushort> _macKeyCodeMap = new Dictionary<Key, ushort>
    {
        // Letters
        { Key.A, 0 }, { Key.B, 11 }, { Key.C, 8 }, { Key.D, 2 }, { Key.E, 14 },
        { Key.F, 3 }, { Key.G, 5 }, { Key.H, 4 }, { Key.I, 34 }, { Key.J, 38 },
        { Key.K, 40 }, { Key.L, 37 }, { Key.M, 46 }, { Key.N, 45 }, { Key.O, 31 },
        { Key.P, 35 }, { Key.Q, 12 }, { Key.R, 15 }, { Key.S, 1 }, { Key.T, 17 },
        { Key.U, 32 }, { Key.V, 9 }, { Key.W, 13 }, { Key.X, 7 }, { Key.Y, 16 },
        { Key.Z, 6 },
        // Numbers
        { Key.D0, 29 }, { Key.D1, 18 }, { Key.D2, 19 }, { Key.D3, 20 }, { Key.D4, 21 },
        { Key.D5, 23 }, { Key.D6, 22 }, { Key.D7, 26 }, { Key.D8, 28 }, { Key.D9, 25 },
        // Arrow keys
        { Key.Up, 126 }, { Key.Down, 125 }, { Key.Left, 123 }, { Key.Right, 124 },
        // Special keys
        { Key.Space, 49 }, { Key.Enter, 36 }, { Key.Tab, 48 }, { Key.Escape, 53 },
    };
    private ushort MapAvaloniaKeyToMacKeyCode(Key key)
    {
        if (_macKeyCodeMap.TryGetValue(key, out ushort macKeyCode))
        {
            return macKeyCode;
        }
    
        // Log missing mapping
        Console.WriteLine($"Warning: No Mac key code mapping for {key}");
        return 49; // Default to space
    }
    private static readonly Dictionary<Key, VirtualKeyCode> _virtualKeyCodeMap = new Dictionary<Key, VirtualKeyCode>
    {
        // Letters
        { Key.A, VirtualKeyCode.VK_A }, { Key.B, VirtualKeyCode.VK_B }, { Key.C, VirtualKeyCode.VK_C },
        { Key.D, VirtualKeyCode.VK_D }, { Key.E, VirtualKeyCode.VK_E }, { Key.F, VirtualKeyCode.VK_F },
        { Key.G, VirtualKeyCode.VK_G }, { Key.H, VirtualKeyCode.VK_H }, { Key.I, VirtualKeyCode.VK_I },
        { Key.J, VirtualKeyCode.VK_J }, { Key.K, VirtualKeyCode.VK_K }, { Key.L, VirtualKeyCode.VK_L },
        { Key.M, VirtualKeyCode.VK_M }, { Key.N, VirtualKeyCode.VK_N }, { Key.O, VirtualKeyCode.VK_O },
        { Key.P, VirtualKeyCode.VK_P }, { Key.Q, VirtualKeyCode.VK_Q }, { Key.R, VirtualKeyCode.VK_R },
        { Key.S, VirtualKeyCode.VK_S }, { Key.T, VirtualKeyCode.VK_T }, { Key.U, VirtualKeyCode.VK_U },
        { Key.V, VirtualKeyCode.VK_V }, { Key.W, VirtualKeyCode.VK_W }, { Key.X, VirtualKeyCode.VK_X },
        { Key.Y, VirtualKeyCode.VK_Y }, { Key.Z, VirtualKeyCode.VK_Z },
    
        // Numbers
        { Key.D0, VirtualKeyCode.VK_0 }, { Key.D1, VirtualKeyCode.VK_1 }, { Key.D2, VirtualKeyCode.VK_2 },
        { Key.D3, VirtualKeyCode.VK_3 }, { Key.D4, VirtualKeyCode.VK_4 }, { Key.D5, VirtualKeyCode.VK_5 },
        { Key.D6, VirtualKeyCode.VK_6 }, { Key.D7, VirtualKeyCode.VK_7 }, { Key.D8, VirtualKeyCode.VK_8 },
        { Key.D9, VirtualKeyCode.VK_9 },
    
        // Arrow keys
        { Key.Up, VirtualKeyCode.UP }, { Key.Down, VirtualKeyCode.DOWN },
        { Key.Left, VirtualKeyCode.LEFT }, { Key.Right, VirtualKeyCode.RIGHT },
    
        // Special keys
        { Key.Space, VirtualKeyCode.SPACE }, { Key.Enter, VirtualKeyCode.RETURN },
        { Key.Tab, VirtualKeyCode.TAB }, { Key.Escape, VirtualKeyCode.ESCAPE }
    };
    private VirtualKeyCode MapAvaloniaKeyToVirtualKey(Key key)
    {
        if (_virtualKeyCodeMap.TryGetValue(key, out VirtualKeyCode vkCode))
        {
            return vkCode;
        }

        // Try a dynamic approach for unmapped keys
        if (Enum.TryParse<VirtualKeyCode>($"VK_{key}", out var dynamicVkCode))
        {
            return dynamicVkCode;
        }

        // Log missing mapping
        Console.WriteLine($"Warning: No virtual key code mapping for {key}");
        return VirtualKeyCode.SPACE; // Default to space
    }
}