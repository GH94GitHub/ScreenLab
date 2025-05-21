namespace ScreenLab.Screen.Regions
{
    public interface IRegionAnalyzerFactory<TAnalyzer> where TAnalyzer : RegionAnalyzer
    {
        static abstract TAnalyzer Create(string name, Rectangle region);
    }
}
