using static ScreenLab.Models.Directions;

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
        List<Point> GetAllMatchingPoints(Point referencePoint, Color targetColor, Direction[] directions, int tolerance = 10);
        List<Point> GetAllMatchingPoints(Point referencePoint, Color targetColor, Direction[] directions, int tolerance = 10, double proximity = double.MaxValue);
        Direction? DirectionFromPoint(Point reference, Point target);
        bool IsInDirectionalCone(Point reference, Point target, Direction direction, double coneThresholdCos = 0.707); // ~45°
    }
}
