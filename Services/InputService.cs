﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Drawing;
using Avalonia.Input;
using Bachelor.ViewModels;
using InputSimulatorStandard;
using InputSimulatorStandard.Native;
using System.Diagnostics;

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

    private IntPtr _targetWindowId = IntPtr.Zero;

    public void SetTargetWindow()
    {
        if (OperatingSystem.IsLinux())
        {
            try
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "xdotool",
                        Arguments = "getactivewindow",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit();

                if (process.ExitCode == 0 && !string.IsNullOrEmpty(output) &&
                    IntPtr.TryParse(output, out var windowId))
                {
                    _targetWindowId = windowId;
                    Console.WriteLine($"Target window set to: {_targetWindowId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get active window: {ex.Message}");
            }
        }
    }

    private bool ExecuteXdotool(string arguments)
    {
        try
        {
            Console.WriteLine($"Executing xdotool: {arguments}");

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "xdotool",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            bool exited = process.WaitForExit(500);

            if (!exited || process.ExitCode != 0)
            {
                string error = process.StandardError.ReadToEnd();
                Console.WriteLine($"xdotool error (code {process.ExitCode}): {error}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to execute xdotool: {ex.Message}");
            return false;
        }
    }

    // macOS CoreGraphics P/Invoke declarations
    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern IntPtr CGEventCreateKeyboardEvent(IntPtr source, ushort keyCode, bool keyDown);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern void CGEventPost(uint tapLocation, IntPtr eventRef);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern void CFRelease(IntPtr cf);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern IntPtr CGEventCreateScrollWheelEvent(IntPtr source, uint units, int wheelCount, int wheel1);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern CGPoint CGEventGetLocation(IntPtr evt);

    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern IntPtr
        CGEventCreateMouseEvent(IntPtr source, uint mouseType, CGPoint point, uint mouseButton);

    // macOS mouse event types
    private const uint kCGEventMouseMoved = 5;
    private const uint kCGEventLeftMouseDown = 1;
    private const uint kCGEventLeftMouseUp = 2;
    private const uint kCGEventRightMouseDown = 3;
    private const uint kCGEventRightMouseUp = 4;
    private const uint kCGEventScrollWheel = 22;
    private const uint kCGMouseButtonLeft = 0;
    private const uint kCGMouseButtonRight = 1;

    // Structure for macOS point
    [StructLayout(LayoutKind.Sequential)]
    public struct CGPoint
    {
        public double X;
        public double Y;

        public CGPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    // Move mouse relative to current position
    public void MoveMouseRelative(int deltaX, int deltaY)
    {
        if (OperatingSystem.IsWindows())
        {
            _simulator.Mouse.MoveMouseBy(deltaX, deltaY);
        }
        else if (OperatingSystem.IsMacOS())
        {
            // Get current mouse position
            IntPtr currentEvent = CGEventCreateMouseEvent(IntPtr.Zero, kCGEventMouseMoved,
                new CGPoint(0, 0), 0);
            CGPoint currentPosition = CGEventGetLocation(currentEvent);
            CFRelease(currentEvent);

            // Create and post mouse move event
            CGPoint newPosition = new CGPoint(currentPosition.X + deltaX, currentPosition.Y + deltaY);
            IntPtr moveEvent = CGEventCreateMouseEvent(IntPtr.Zero, kCGEventMouseMoved,
                newPosition, 0);
            CGEventPost(kCGHIDEventTap, moveEvent);
            CFRelease(moveEvent);
        }

        _outputViewModel?.Log($"Mouse moved: {deltaX},{deltaY}");
    }

    // Click mouse buttons
    public void MouseDown(bool rightButton = false)
    {
        if (OperatingSystem.IsWindows())
        {
            if (rightButton)
                _simulator.Mouse.RightButtonDown();
            else
                _simulator.Mouse.LeftButtonDown();
        }
        else if (OperatingSystem.IsMacOS())
        {
            // Get current position
            IntPtr currentEvent = CGEventCreateMouseEvent(IntPtr.Zero, kCGEventMouseMoved,
                new CGPoint(0, 0), 0);
            CGPoint position = CGEventGetLocation(currentEvent);
            CFRelease(currentEvent);

            // Create and post mouse down event
            uint eventType = rightButton ? kCGEventRightMouseDown : kCGEventLeftMouseDown;
            uint button = rightButton ? kCGMouseButtonRight : kCGMouseButtonLeft;
            IntPtr clickEvent = CGEventCreateMouseEvent(IntPtr.Zero, eventType, position, button);
            CGEventPost(kCGHIDEventTap, clickEvent);
            CFRelease(clickEvent);
        }

        _outputViewModel?.Log($"Mouse {(rightButton ? "right" : "left")} button down");
    }

    // Similar implementation for MouseUp method
    public void MouseUp(bool rightButton = false)
    {
        if (OperatingSystem.IsWindows())
        {
            if (rightButton)
                _simulator.Mouse.RightButtonUp();
            else
                _simulator.Mouse.LeftButtonUp();
        }
        else if (OperatingSystem.IsMacOS())
        {
            // Get current position
            IntPtr currentEvent = CGEventCreateMouseEvent(IntPtr.Zero, kCGEventMouseMoved,
                new CGPoint(0, 0), 0);
            CGPoint position = CGEventGetLocation(currentEvent);
            CFRelease(currentEvent);

            // Create and post mouse down event
            uint eventType = rightButton ? kCGEventRightMouseDown : kCGEventLeftMouseDown;
            uint button = rightButton ? kCGMouseButtonRight : kCGMouseButtonLeft;
            IntPtr clickEvent = CGEventCreateMouseEvent(IntPtr.Zero, eventType, position, button);
            CGEventPost(kCGHIDEventTap, clickEvent);
            CFRelease(clickEvent);
        }
    }

    public void ScrollMouse(int amount)
    {
        if (OperatingSystem.IsWindows())
        {
            _simulator.Mouse.VerticalScroll(amount);
        }
        else if (OperatingSystem.IsMacOS())
        {
            IntPtr scrollEvent = CGEventCreateScrollWheelEvent(IntPtr.Zero, 0, 1, amount);
            CGEventPost(kCGHIDEventTap, scrollEvent);
            CFRelease(scrollEvent);
        }

        _outputViewModel?.Log($"Mouse scrolled: {amount}");
    }

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
                else if (OperatingSystem.IsLinux())
                {
                    string xdoKey = MapAvaloniaKeyToXdotoolKey(key);
                
                    if (_targetWindowId != IntPtr.Zero)
                    {
                        // Use the specific window ID
                        ExecuteXdotool($"key --window {_targetWindowId} --delay 50 {xdoKey} down");
                    }
                    else
                    {
                        // If no specific window, try to use the currently active window
                        ExecuteXdotool($"key --delay 50 {xdoKey} down");
                    }
                }

                _keysCurrentlyDown[keyIdentifier] = true;
                _outputViewModel?.Log($"Pressing key: {keyName} | Movement: {movementName}");
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
                    // Windows implementation unchanged
                    VirtualKeyCode vkCode = MapAvaloniaKeyToVirtualKey(key);
                    _simulator.Keyboard.KeyUp(vkCode);
                }
                else if (OperatingSystem.IsMacOS())
                {
                    // macOS implementation unchanged
                    ushort macKeyCode = MapAvaloniaKeyToMacKeyCode(key);
                    SimulateMacKeyEvent(macKeyCode, false);
                }
                else if (OperatingSystem.IsLinux())
                {
                    string xdoKey = MapAvaloniaKeyToXdotoolKey(key);
                
                    if (_targetWindowId != IntPtr.Zero)
                    {
                        // Use the specific window ID
                        ExecuteXdotool($"key --window {_targetWindowId} --delay 50 {xdoKey} up");
                    }
                    else
                    {
                        // If no specific window, try to use the currently active window
                        ExecuteXdotool($"key --delay 50 {xdoKey} up");
                    }
                }

                _keysCurrentlyDown[keyIdentifier] = false;
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

    private string MapAvaloniaKeyToXdotoolKey(Key key)
    {
        switch (key)
        {
            case Key.A: return "a";
            case Key.B: return "b";
            case Key.C: return "c";
            case Key.D: return "d";
            case Key.E: return "e";
            case Key.F: return "f";
            case Key.G: return "g";
            case Key.H: return "h";
            case Key.I: return "i";
            case Key.J: return "j";
            case Key.K: return "k";
            case Key.L: return "l";
            case Key.M: return "m";
            case Key.N: return "n";
            case Key.O: return "o";
            case Key.P: return "p";
            case Key.Q: return "q";
            case Key.R: return "r";
            case Key.S: return "s";
            case Key.T: return "t";
            case Key.U: return "u";
            case Key.V: return "v";
            case Key.W: return "w";
            case Key.X: return "x";
            case Key.Y: return "y";
            case Key.Z: return "z";
            case Key.D0: return "0";
            case Key.D1: return "1";
            case Key.D2: return "2";
            case Key.D3: return "3";
            case Key.D4: return "4";
            case Key.D5: return "5";
            case Key.D6: return "6";
            case Key.D7: return "7";
            case Key.D8: return "8";
            case Key.D9: return "9";
            case Key.Space: return "space";
            case Key.Enter: return "Return";
            case Key.Tab: return "Tab";
            case Key.Escape: return "Escape";
            case Key.Delete: return "Delete";
            case Key.Back: return "BackSpace";
            case Key.Left: return "Left";
            case Key.Right: return "Right";
            case Key.Up: return "Up";
            case Key.Down: return "Down";

            default:
                string keyName = key.ToString().ToLower();
                Console.WriteLine($"Using fallback mapping for {key}: {keyName}");
                return keyName;
        }
    }
}