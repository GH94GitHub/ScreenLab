namespace ScreenLab.Screen.Colors
{
    public interface IColorAnalyzerFactory<TAnalyzer> where TAnalyzer : ColorAnalyzer
    {
        static abstract TAnalyzer Create(string name, Color color);
    }
}
