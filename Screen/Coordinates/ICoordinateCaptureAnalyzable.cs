using System.Diagnostics.CodeAnalysis;

namespace ScreenLab.Screen.Coordinates
{
    public interface ICoordinateCaptureAnalyzable
    {
        void StoreLocalAnalyzer(CoordinateAnalyzer analyzer);
        void StoreGlobalAnalyzer(CoordinateAnalyzer analyzer);
        bool TryGetLocalAnalyzer<TAnalyzer>(string name, [MaybeNullWhen(false)] out TAnalyzer analyzer)
            where TAnalyzer : CoordinateAnalyzer, ICoordinateAnalyzerFactory<TAnalyzer>;
        bool TryGetGlobalAnalyzer<TAnalyzer>(string name, [MaybeNullWhen(false)] out TAnalyzer analyzer)
            where TAnalyzer : CoordinateAnalyzer, ICoordinateAnalyzerFactory<TAnalyzer>;
    }
}
