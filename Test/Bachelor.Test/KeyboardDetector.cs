using System.Runtime.InteropServices;
using InputSimulatorStandard.Native;

public class KeyboardDetector : IDisposable
{
    private IntPtr hookId = IntPtr.Zero;
    private bool keyDetected = false;
    private VirtualKeyCode targetKey;
    
    [DllImport("user32.dll")]
    static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc callback, IntPtr hInstance, uint threadId);
    
    [DllImport("user32.dll")]
    static extern bool UnhookWindowsHookEx(IntPtr hookId);
    
    [DllImport("user32.dll")]
    static extern IntPtr CallNextHookEx(IntPtr hookId, int nCode, IntPtr wParam, IntPtr lParam);
    
    [DllImport("kernel32.dll")]
    static extern IntPtr GetModuleHandle(string lpModuleName);
    
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    private LowLevelKeyboardProc proc;
    
    public KeyboardDetector(VirtualKeyCode keyToDetect)
    {
        targetKey = keyToDetect;
        proc = HookCallback;
        hookId = SetKeyboardHook();
    }
    
    public bool WasKeyDetected() => keyDetected;
    
    private IntPtr SetKeyboardHook()
    {
        const int WH_KEYBOARD_LL = 13;
        return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(null), 0);
    }
    
    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        const int WM_KEYDOWN = 0x0100;
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            if (vkCode == (int)targetKey)
            {
                keyDetected = true;
            }
        }
        return CallNextHookEx(hookId, nCode, wParam, lParam);
    }
    
    public void Dispose()
    {
        if (hookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(hookId);
            hookId = IntPtr.Zero;
        }
    }
}