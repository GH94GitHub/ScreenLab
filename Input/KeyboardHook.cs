using System.Diagnostics;
using System.Runtime.InteropServices;
namespace ScreenLab.Input
{
    static class KeyboardHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        // Stores the delegate so GC won't collect it
        private static LowLevelKeyboardProc _proc;
        private static nint _hookID = nint.Zero;

        // The specific key we want to listen for, plus the callback to run
        private static int _targetVKey;
        private static Action _onTargetKeyDown;

        private delegate nint LowLevelKeyboardProc(int nCode, nint wParam, nint lParam);

        /// <summary>
        /// Installs a global low-level keyboard hook for one specific virtual key (e.g., 0xC0 for tilde/backtick).
        /// When that key is pressed, <paramref name="onKeyDown"/> is invoked.
        /// </summary>
        public static void HookKeyboard(int virtualKey, Action onKeyDown)
        {
            // Store the user’s key and callback in static fields.
            _targetVKey = virtualKey;
            _onTargetKeyDown = onKeyDown;

            // The hook callback
            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }


        /// <summary>
        /// Removes the global low-level keyboard hook to free resources.
        /// </summary>
        public static void UnhookKeyboard()
        {
            if (_hookID != nint.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = nint.Zero;
                _proc = null;
                _onTargetKeyDown = null;
            }
        }

        private static nint HookCallback(int nCode, nint wParam, nint lParam)
        {
            if (nCode >= 0)
            {
                // Check for WM_KEYDOWN or WM_SYSKEYDOWN
                if (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN)
                {
                    int vkCode = Marshal.ReadInt32(lParam);

                    // For debugging: see which VK is actually coming in

                    if (vkCode == _targetVKey)
                    {
                        _onTargetKeyDown?.Invoke();
                    }
                }
            }

            // Pass to the next hook in the chain
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static nint SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                nint moduleHandle = GetModuleHandle(curModule.ModuleName);
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, moduleHandle, 0);
            }
        }

        #region WinAPI Imports

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern nint SetWindowsHookEx(int idHook,
                                                     LowLevelKeyboardProc lpfn,
                                                     nint hMod,
                                                     uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(nint hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern nint CallNextHookEx(nint hhk,
                                                    int nCode,
                                                    nint wParam,
                                                    nint lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern nint GetModuleHandle(string lpModuleName);

        #endregion
    }
}
