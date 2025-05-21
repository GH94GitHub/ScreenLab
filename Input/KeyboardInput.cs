using System.Runtime.InteropServices;

namespace ScreenLab.Input
{
    public struct KEYEVENTS
    {
        public const byte VK_SPACE = 0x20;
        public const byte VK_ESCAPE = 0x1B;
        public const int VK_LSHIFT = 0xA0;
        public const byte VK_UP = 0x26;
        public const byte VK_DOWN = 0x28;
        public const byte VK_LEFT = 0x25;
        public const byte VK_RIGHT = 0x27;
        public const int KEYEVENTF_EXTENDEDKEY = 0x0001;
        public const int KEYEVENTF_KEYDOWN = 0x0000;
        public const int KEYEVENTF_KEYUP = 0x0002;
        public const int KEY_1 = 0x31;
        public const int KEY_2 = 0x32;
        public const int KEY_3 = 0x33;
        public const int KEY_4 = 0x34;
        public const int KEY_TILDE = 0xC0;  // VK_OEM_3
    }
    public static class KeyboardInput
    {
        private static Random rand = new Random();

        // Import user32.dll for sending key inputs
        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        /// <summary>
        /// Sends a Spacebar key press event
        /// </summary>
        public static void SendSpacebar()
        {
            keybd_event(KEYEVENTS.VK_SPACE, 0, KEYEVENTS.KEYEVENTF_KEYDOWN, 0); // Press Spacebar
            BetweenKeyDelay(); // Small delay to simulate real key press
            keybd_event(KEYEVENTS.VK_SPACE, 0, KEYEVENTS.KEYEVENTF_KEYUP, 0);   // Release Spacebar
        }

        /// <summary>
        /// Sends an Escape (Esc) key press event.
        /// </summary>
        public static void SendEscape()
        {
            keybd_event(KEYEVENTS.VK_ESCAPE, 0, KEYEVENTS.KEYEVENTF_KEYDOWN, 0); // Press Esc
            BetweenKeyDelay(); // Small delay to simulate real key press
            keybd_event(KEYEVENTS.VK_ESCAPE, 0, KEYEVENTS.KEYEVENTF_KEYUP, 0);   // Release Esc
        }
        public static void SendKey1()
        {
            keybd_event(KEYEVENTS.KEY_1, 0, KEYEVENTS.KEYEVENTF_KEYDOWN, 0);
            BetweenKeyDelay(); // Small delay to simulate real key press
            keybd_event(KEYEVENTS.KEY_1, 0, KEYEVENTS.KEYEVENTF_KEYUP, 0);
        }
        public static void SendKey2()
        {
            keybd_event(KEYEVENTS.KEY_2, 0, KEYEVENTS.KEYEVENTF_KEYDOWN, 0);
            BetweenKeyDelay(); // Small delay to simulate real key press
            keybd_event(KEYEVENTS.KEY_2, 0, KEYEVENTS.KEYEVENTF_KEYUP, 0);
        }
        public static void SendKey3()
        {
            keybd_event(KEYEVENTS.KEY_3, 0, KEYEVENTS.KEYEVENTF_KEYDOWN, 0);
            BetweenKeyDelay(); // Small delay to simulate real key press
            keybd_event(KEYEVENTS.KEY_3, 0, KEYEVENTS.KEYEVENTF_KEYUP, 0);
        }
        public static void SendKey4()
        {
            keybd_event(KEYEVENTS.KEY_4, 0, KEYEVENTS.KEYEVENTF_KEYDOWN, 0);
            BetweenKeyDelay(); // Small delay to simulate real key press
            keybd_event(KEYEVENTS.KEY_4, 0, KEYEVENTS.KEYEVENTF_KEYUP, 0);
        }
        public static void SendUpKey(int holdTime = 45) 
        {
            keybd_event(KEYEVENTS.VK_UP, 0, KEYEVENTS.KEYEVENTF_KEYDOWN, 0);
            Thread.Sleep(holdTime);
            keybd_event(KEYEVENTS.VK_UP, 0, KEYEVENTS.KEYEVENTF_KEYUP, 0);
        }
        public static void SendDownKey(int holdTime = 45)
        {
            keybd_event(KEYEVENTS.VK_DOWN, 0, KEYEVENTS.KEYEVENTF_KEYDOWN, 0);
            Thread.Sleep(holdTime);
            keybd_event(KEYEVENTS.VK_DOWN, 0, KEYEVENTS.KEYEVENTF_KEYUP, 0);
        }
        public static void SendLeftKey(int holdTime = 45)
        {
            keybd_event(KEYEVENTS.VK_LEFT, 0, KEYEVENTS.KEYEVENTF_KEYDOWN, 0);
            Thread.Sleep(holdTime);
            keybd_event(KEYEVENTS.VK_LEFT, 0, KEYEVENTS.KEYEVENTF_KEYUP, 0);
        }
        public static void SendRightKey(int holdTime = 45)
        {
            keybd_event(KEYEVENTS.VK_RIGHT, 0, KEYEVENTS.KEYEVENTF_KEYDOWN, 0);
            Thread.Sleep(holdTime);
            keybd_event(KEYEVENTS.VK_RIGHT, 0, KEYEVENTS.KEYEVENTF_KEYUP, 0);
        }
        private static void BetweenKeyDelay()
        {
            Thread.Sleep(rand.Next(35, 50));
        }

        public static void HoldShiftDown()
        {
            // Press down left-shift (no extended flag, scancode 0x2A)
            keybd_event(KEYEVENTS.VK_LSHIFT, 0x2A, KEYEVENTS.KEYEVENTF_KEYDOWN, 0);
        }

        public static void ReleaseShiftKey()
        {
            // Release left-shift
            keybd_event(KEYEVENTS.VK_LSHIFT, 0x2A, KEYEVENTS.KEYEVENTF_KEYUP, 0);
        }

        public static void HookKeyboard(int keyEvent, Action action)
        {
            // Listen for KEYEVENT presses globally
            KeyboardHook.HookKeyboard(keyEvent, action);
        }

        public static void UnhookKeyboard()
        {
            KeyboardHook.UnhookKeyboard();
        }
        public static bool IsShiftPressed =>
            (GetAsyncKeyState(KEYEVENTS.VK_LSHIFT) & 0x8000) != 0;

        public static bool IsAltPressed =>
            (GetAsyncKeyState((int)Keys.Menu) & 0x8000) != 0; // VK_MENU = Alt

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);
    }
}
