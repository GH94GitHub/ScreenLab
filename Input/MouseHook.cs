using System.Diagnostics;
using System.Runtime.InteropServices;
using static ScreenLab.Input.MouseInput;

namespace ScreenLab.Input
{
    static class MouseHook
    {
        private static bool isWaitingForClick = false;
        private static LowLevelMouseProc mouseProc = HookCallback;
        private static nint hookID = nint.Zero;
        private static CustomCallback? customCallback;

        // Import Windows API functions 
        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;

        private static TaskCompletionSource<Point> clickCompletion = new();

        public static void GetClick(CustomCallback? callback = null)
        {
            if (!isWaitingForClick)
            {
                isWaitingForClick = true;
                customCallback = callback;
                hookID = SetHook(mouseProc);
            }
        }

        public static async Task<Point> GetClickAsync()
        {
            GetClick();
            return await clickCompletion.Task;
        }

        // ----------------------------------------- Imports ------------------------------------
        private static nint HookCallback(int nCode, nint wParam, nint lParam)
        {
            if (nCode >= 0 && isWaitingForClick && wParam == WM_LBUTTONDOWN)
            {
                MSLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                Point globalClickPosition = new Point(hookStruct.pt.x, hookStruct.pt.y);

                isWaitingForClick = false;

                // 🔁 Call the provided callback
                customCallback?.Invoke(globalClickPosition);
                clickCompletion.TrySetResult(globalClickPosition);
                // Clear callback and hook
                clickCompletion = new();
                customCallback = null;
                UnhookWindowsHookEx(hookID);
            }

            return CallNextHookEx(hookID, nCode, wParam, lParam);
        }


        public static void UnhookWindowsMouseWait()
        {
            UnhookWindowsHookEx(hookID);
        }

        private static nint SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern nint SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, nint hMod, uint dwThreadId);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern nint GetModuleHandle(string lpModuleName);

        private delegate nint LowLevelMouseProc(int nCode, nint wParam, nint lParam);

        [DllImport("user32.dll")]
        private static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, nint lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(nint hhk);

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public nint dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int x; public int y; }

    }
}
