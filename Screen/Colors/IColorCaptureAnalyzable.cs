using System.Diagnostics.CodeAnalysis;

namespace ScreenLab.Screen.Colors
{
    public interface IColorCaptureAnalyzable
    {
        void StoreLocalAnalyzer(ColorAnalyzer analyzer);
        void StoreGlobalAnalyzer(ColorAnalyzer analyzer);
        bool TryGetLocalAnalyzer<TAnalyzer>(string name, [MaybeNullWhen(false)] out TAnalyzer analyzer)
            where TAnalyzer : ColorAnalyzer, IColorAnalyzerFactory<TAnalyzer>;
        bool TryGetGlobalAnalyzer<TAnalyzer>(string name, [MaybeNullWhen(false)] out TAnalyzer analyzer)
            where TAnalyzer : ColorAnalyzer, IColorAnalyzerFactory<TAnalyzer>;
    }
}
