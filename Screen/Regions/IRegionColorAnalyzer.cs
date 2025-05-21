namespace ScreenLab.Screen.Regions
{
    public interface IRegionColorAnalyzer
    {
        Color GetAverageColor();
        Bitmap GetBitmap();
        Color GetDominantColor();
        Dictionary<Color, int> GetColorCount();
        Point? FindColorInRegion(Color targetColor, int tolerance = 10);
        Point? FindClosestMatchingColor(Point referencePoint, Color targetColor, int tolerance = 10);
        List<Point> FindClosestMatchingColors(Point referencePoint, Color targetColor, int maxClosestCount, int tolerance = 10);
        List<Point> GetAllMatchingPoints(Color targetColor, int tolerance = 10);
    }
}
