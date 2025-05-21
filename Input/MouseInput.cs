using System.Runtime.InteropServices;

namespace ScreenLab.Input
{
    struct MOUSE_EVENTS
    {
        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        public const int MOUSEEVENTF_RIGHTUP = 0x10;
        public const int MOUSEEVENTF_WHEEL = 0x0800;
    };
    [StructLayout(LayoutKind.Sequential)]
    struct INPUT
    {
        public int type;
        public MOUSEINPUT mi;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    public static class MouseInput
    {
        private static Random rand = new Random();

        public static void DoMouseClick()
        {
            //Call the imported function with the cursor's current position
            int X = Cursor.Position.X;
            int Y = Cursor.Position.Y;
            MouseHook.mouse_event(MOUSE_EVENTS.MOUSEEVENTF_LEFTDOWN | MOUSE_EVENTS.MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
        }

        public static void ScrollDown(int times)
        {
            const int SCROLL_DELTA = 120;
            for (int i = 0; i < times; i++)
            {
                INPUT[] input = new INPUT[1];
                input[0].type = 0; // 0 is Mouse input
                input[0].mi = new MOUSEINPUT
                {
                    dx = 0,
                    dy = 0,
                    mouseData = unchecked((uint)(-SCROLL_DELTA)),
                    dwFlags = MOUSE_EVENTS.MOUSEEVENTF_WHEEL,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                };

                SendInput(1, input, Marshal.SizeOf(typeof(INPUT)));
                Thread.Sleep(new Random().Next(20, 50));
            }
        }
        public static void ScrollUp(int times)
        {
            const int SCROLL_DELTA = 120;

            for (int i = 0; i < times; i++)
            {
                INPUT[] input = new INPUT[1];
                input[0].type = 0; // 0 is Mouse input
                input[0].mi = new MOUSEINPUT
                {
                    dx = 0,
                    dy = 0,
                    mouseData = (uint)(SCROLL_DELTA),
                    dwFlags = MOUSE_EVENTS.MOUSEEVENTF_WHEEL,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                };

                SendInput(1, input, Marshal.SizeOf(typeof(INPUT)));
                Thread.Sleep(new Random().Next(20, 50));
            }
        }

        public static void LinearSmoothMove(Point goalPosition, int steps, int offsetAmount = 0)
        {
            LinearSmoothMove(goalPosition, steps, offsetAmount, offsetAmount, offsetAmount, offsetAmount);
        }

        public static void LinearSmoothMove(Point goalPosition, int steps, int offsetRight, int offsetBottom, int offsetLeft, int offsetTop)
        {
            if (!(offsetLeft * -1 <= offsetRight) || !(offsetTop * -1 <= offsetBottom))
                throw new InvalidDataException("Offsets don't make sense");

            Point newPosition = new Point(goalPosition.X + rand.Next(offsetLeft * -1, offsetRight), goalPosition.Y + rand.Next(offsetTop * -1, offsetBottom));

            Point start = Cursor.Position;
            PointF iterPoint = start;

            // Find the slope of the line segment defined by start and newPosition
            PointF slope = new PointF(newPosition.X - start.X, newPosition.Y - start.Y);

            // Divide by the number of steps
            slope.X = slope.X / steps;
            slope.Y = slope.Y / steps;

            // Move the mouse to each iterative point.
            for (int i = 0; i < steps; i++)
            {
                iterPoint = new PointF(iterPoint.X + slope.X, iterPoint.Y + slope.Y);
                Cursor.Position = Point.Round(iterPoint);
                Thread.Sleep(rand.Next(5, 7));
            }

            // Move the mouse to the final destination.
            Cursor.Position = newPosition;
        }

        public delegate void CustomCallback(Point point);
        public static void HookClick(CustomCallback callback)
        {
            MouseHook.GetClick(callback);
        }
        public static async Task<Point> HookClickAsync()
        {
            return await MouseHook.GetClickAsync();
        }
        public static void UnhookClick()
        {
            MouseHook.UnhookWindowsMouseWait();
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
    }
}
