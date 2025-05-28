namespace ScreenLab.Extensions
{
    public static class ColorExtensions
    {
        public static bool IsColorMatch(this Color c1, Color c2, int tolerance)
        {
            return Math.Abs(c1.R - c2.R) <= tolerance &&
                   Math.Abs(c1.G - c2.G) <= tolerance &&
                   Math.Abs(c1.B - c2.B) <= tolerance;
        }
        public static bool IsColorMatch(this Color c1, Color c2, int redTolerance, int greenTolerance, int blueTolerance)
        {
            return Math.Abs(c1.R - c2.R) <= redTolerance &&
                   Math.Abs(c1.G - c2.G) <= greenTolerance &&
                   Math.Abs(c1.B - c2.B) <= blueTolerance;
        }
    }
}
