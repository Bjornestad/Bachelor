using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia.Input;
using InputSimulatorStandard;
using InputSimulatorStandard.Native;

namespace Bachelor.Services;

public class InputService
{
    private static readonly InputSimulator _simulator = new InputSimulator();
    private Dictionary<string, bool> _keysCurrentlyDown = new Dictionary<string, bool>();

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

    private ushort MapAvaloniaKeyToMacKeyCode(Key key)
    {
        // Map Avalonia Keys to macOS Virtual Key Codes
        // https://developer.apple.com/documentation/coreservices/1390584-keycode
        return key switch
        {
            Key.A => 0,      // kVK_ANSI_A
            Key.S => 1,      // kVK_ANSI_S
            Key.D => 2,      // kVK_ANSI_D
            Key.W => 13,     // kVK_ANSI_W
            Key.Q => 12,     // kVK_ANSI_Q
            Key.E => 14,     // kVK_ANSI_E
            Key.Up => 126,   // kVK_UpArrow
            Key.Down => 125, // kVK_DownArrow
            Key.Left => 123, // kVK_LeftArrow
            Key.Right => 124,// kVK_RightArrow
            Key.Space => 49, // kVK_Space
            Key.Enter => 36, // kVK_Return
            _ => 49          // Default to space
        };
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