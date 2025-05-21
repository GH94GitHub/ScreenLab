using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace ScreenLab.Utility
{
    public static class RgbScreenUtil
    {
        /// <summary>
        /// Gets the color of a specific pixel.
        /// </summary>
        public static Color GetPixelColor(int x, int y)
        {
            nint hdc = GetDC(nint.Zero);
            uint pixel = GetPixel(hdc, x, y);
            ReleaseDC(nint.Zero, hdc);
            return Color.FromArgb((int)(pixel & 0x000000FF),
                                  (int)(pixel & 0x0000FF00) >> 8,
                                  (int)(pixel & 0x00FF0000) >> 16);
        }

        [DllImport("user32.dll")] public static extern nint GetDC(nint hwnd);
        [DllImport("user32.dll")] public static extern int ReleaseDC(nint hwnd, nint hdc);
        [DllImport("gdi32.dll")] public static extern uint GetPixel(nint hdc, int x, int y);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="colorString"></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static Color ParseColorString(string colorString)
        {
            // Regex to extract A, R, G, B values
            Regex regex = new Regex(@"A=(\d+),\s*R=(\d+),\s*G=(\d+),\s*B=(\d+)");
            Match match = regex.Match(colorString);

            if (match.Success)
            {
                int a = int.Parse(match.Groups[1].Value);
                int r = int.Parse(match.Groups[2].Value);
                int g = int.Parse(match.Groups[3].Value);
                int b = int.Parse(match.Groups[4].Value);
                return Color.FromArgb(a, r, g, b);
            }
            else
            {
                throw new FormatException("Invalid color string format.");
            }
        }
    }

}
