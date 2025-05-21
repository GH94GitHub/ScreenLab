using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
namespace ScreenLab.Screen.Colors
{
    public class ColorCapture : CaptureBase<Color>, IColorCaptureAnalyzable
    {
        public override Color DefaultValue => Color.Empty;

        protected override string Title => "Color";
        private Dictionary<string, ColorAnalyzer> _localAnalyzers = new();
        private Dictionary<string, ColorAnalyzer> _globalAnalyzers = new();

        public ColorCapture(string localGroupName, string storageDirName, params string[] localColorNames)
            : base(localGroupName, Path.Combine(storageDirName, "colors.json"), localColorNames)
        { }

        protected override Dictionary<string, Color> DeserializeValues(string content)
        {
            var raw = JsonSerializer.Deserialize<Dictionary<string, string>>(content) ?? new();
            return raw.ToDictionary(kvp => kvp.Key, kvp => ColorTranslator.FromHtml(kvp.Value));
        }

        protected override string SerializeValues(Dictionary<string, Color> values)
        {
            var raw = values.ToDictionary(kvp => kvp.Key, kvp => ColorTranslator.ToHtml(kvp.Value));
            return JsonSerializer.Serialize(raw, new JsonSerializerOptions { WriteIndented = true });
        }

        public void StoreLocalAnalyzer(ColorAnalyzer analyzer)
        {
            _localAnalyzers[analyzer.Name] = analyzer;
            StoreLocalValue(analyzer.Name, analyzer.Selection);
        }

        public void StoreGlobalAnalyzer(ColorAnalyzer analyzer)
        {
            _globalAnalyzers[analyzer.Name] = analyzer;
            StoreGlobalValue(analyzer.Name, analyzer.Selection);
        }

        public bool TryGetLocalAnalyzer<TAnalyzer>(string name, [MaybeNullWhen(false)] out TAnalyzer analyzer) 
            where TAnalyzer : ColorAnalyzer, IColorAnalyzerFactory<TAnalyzer>
        {
            // Make sure there's a value for the local Name
            if (!TryGetLocalValue(name, out Color localColor) && localColor.Equals(DefaultValue))
            {
                analyzer = null;
                return false;
            }

            // If an analyzer has been created return it otherwise create one
            if (_localAnalyzers.TryGetValue(name, out var localAnalyzer))
            {
                analyzer = localAnalyzer as TAnalyzer ?? TAnalyzer.Create(name, localAnalyzer.Selection);
                return true;
            }
            else
            {
                analyzer = TAnalyzer.Create(name, localColor);
                _localAnalyzers.Add(name, analyzer);
                return true;
            }
        }

        public bool TryGetGlobalAnalyzer<TAnalyzer>(string name, [MaybeNullWhen(false)] out TAnalyzer analyzer) 
            where TAnalyzer : ColorAnalyzer, IColorAnalyzerFactory<TAnalyzer>
        {
            // Make sure there's a value for the Global Name
            if (!TryGetGlobalValue(name, out Color globalColor) && globalColor.Equals(DefaultValue))
            {
                analyzer = null;
                return false;
            }

            // If an analyzer has been created return it otherwise create one
            if (_globalAnalyzers.TryGetValue(name, out var globalAnalyzer))
            {
                analyzer = globalAnalyzer as TAnalyzer ?? TAnalyzer.Create(name, globalAnalyzer.Selection);
                return true;
            }
            else
            {
                analyzer = TAnalyzer.Create(name, globalColor);
                _globalAnalyzers.Add(name, analyzer);
                return true;
            }
        }
    }
}
