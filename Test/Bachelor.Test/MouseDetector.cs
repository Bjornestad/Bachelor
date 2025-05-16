using System.Runtime.InteropServices;
using Xunit.Abstractions;

namespace Bachelor.Test
{
    public class MouseDetector : IDisposable
    {
        private IntPtr hookId = IntPtr.Zero;
        private bool mouseMovementDetected = false;
        private Point lastPosition;
        private Point initialPosition; // Add to track initial position
        private LowLevelMouseProc proc;
        private readonly ITestOutputHelper _testOutputHelper;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int X;
            public int Y;
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        public MouseDetector(ITestOutputHelper testOutputHelper = null)
        {
            _testOutputHelper = testOutputHelper;
            GetCursorPos(out lastPosition);
            initialPosition = lastPosition; 
            proc = HookCallback;
            hookId = SetMouseHook();
            _testOutputHelper?.WriteLine($"Initial position: {lastPosition.X}, {lastPosition.Y}");
        }

        public void ResetDetection()
        {
            mouseMovementDetected = false;
            GetCursorPos(out initialPosition); 
            _testOutputHelper?.WriteLine($"Reset mouse detection at {DateTime.Now}");
        }

        public bool CheckForPositionChange()
        {
            Point currentPos;
            GetCursorPos(out currentPos);
            bool changed = currentPos.X != lastPosition.X || currentPos.Y != lastPosition.Y;
            _testOutputHelper?.WriteLine($"Position changed: {changed} (from {lastPosition.X},{lastPosition.Y} to {currentPos.X},{currentPos.Y})");
            lastPosition = currentPos;
            return changed;
        }

        public bool WasMouseMovementDetected()
        {
            // First check if the hook detected movement
            if (mouseMovementDetected)
                return true;
                
            // Fallback: check if current position differs from initial position
            Point currentPos;
            GetCursorPos(out currentPos);
            bool positionDiffers = currentPos.X != initialPosition.X || currentPos.Y != initialPosition.Y;
            
            if (positionDiffers)
                _testOutputHelper?.WriteLine($"Movement detected by position comparison: initial ({initialPosition.X},{initialPosition.Y}), current ({currentPos.X},{currentPos.Y})");
                
            return positionDiffers;
        }

        private IntPtr SetMouseHook()
        {
            const int WH_MOUSE_LL = 14;
            IntPtr hInstance = GetModuleHandle(null);
            return SetWindowsHookEx(WH_MOUSE_LL, proc, hInstance, 0);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            const int WM_MOUSEMOVE = 0x0200;
            if (nCode >= 0 && wParam == (IntPtr)WM_MOUSEMOVE)
            {
                Point currentPos;
                if (GetCursorPos(out currentPos))
                {
                    if (currentPos.X != lastPosition.X || currentPos.Y != lastPosition.Y)
                    {
                        _testOutputHelper?.WriteLine($"Hook detected movement: ({currentPos.X}, {currentPos.Y}) vs ({lastPosition.X}, {lastPosition.Y})");
                        mouseMovementDetected = true;
                        lastPosition = currentPos;
                    }
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
}