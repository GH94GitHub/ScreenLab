
namespace ScreenLab.Screen.Coordinates
{
    public interface ICoordinateAnalyzerFactory<TAnalyzer> where TAnalyzer : CoordinateAnalyzer
    {
        static abstract TAnalyzer Create(string name, Point point);
    }
}
