using System.Diagnostics.CodeAnalysis;

namespace ScreenLab.Screen.Regions
{
    public interface IRegionCaptureAnalyzable
    {
        void StoreLocalAnalyzer(RegionAnalyzer analyzer);
        void StoreGlobalAnalyzer(RegionAnalyzer analyzer);
        bool TryGetLocalAnalyzer<TAnalyzer>(string name, [MaybeNullWhen(false)] out TAnalyzer analyzer)
            where TAnalyzer : RegionAnalyzer, IRegionAnalyzerFactory<TAnalyzer>;
        bool TryGetGlobalAnalyzer<TAnalyzer>(string name, [MaybeNullWhen(false)] out TAnalyzer analyzer)
            where TAnalyzer : RegionAnalyzer, IRegionAnalyzerFactory<TAnalyzer>;
    }
}
