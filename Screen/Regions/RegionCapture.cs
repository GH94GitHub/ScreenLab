using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace ScreenLab.Screen.Regions
{
    public class RegionCapture : CaptureBase<Rectangle>, IRegionCaptureAnalyzable
    {
        private Dictionary<string, RegionAnalyzer> _localAnalyzers = new();
        private Dictionary<string, RegionAnalyzer> _globalAnalyzers = new();

        public override Rectangle DefaultValue => Rectangle.Empty;
        protected sealed override string Title => "Region";

        public RegionCapture(string localGroupName, string storageDirName, params string[] localNames)
            : base(localGroupName, Path.Combine(storageDirName, "regions.json"), localNames) { }

        protected override Dictionary<string, Rectangle> DeserializeValues(string content)
        {
            return JsonSerializer.Deserialize<Dictionary<string, Rectangle>>(content) ?? new();
        }

        protected override string SerializeValues(Dictionary<string, Rectangle> values)
        {
            return JsonSerializer.Serialize(values, new JsonSerializerOptions { WriteIndented = true });
        }

        public bool TryGetGlobalAnalyzer<TAnalyzer>(string name, [MaybeNullWhen(false)] out TAnalyzer analyzer) 
            where TAnalyzer : RegionAnalyzer, IRegionAnalyzerFactory<TAnalyzer>
        {
            Rectangle globalRegion;

            // Make sure there's a value for the Global Region Name
            if (!TryGetGlobalValue(name, out globalRegion) && globalRegion.Equals(DefaultValue))
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
                analyzer = TAnalyzer.Create(name, globalRegion);
                _globalAnalyzers.Add(name, analyzer);
                return true;
            }
        }
        public bool TryGetLocalAnalyzer<TAnalyzer>(string name, [MaybeNullWhen(false)] out TAnalyzer analyzer) 
            where TAnalyzer : RegionAnalyzer, IRegionAnalyzerFactory<TAnalyzer>
        {
            Rectangle localRegion;

            // Make sure there's a value for the local Region Name
            if(!TryGetLocalValue(name, out localRegion) && localRegion.Equals(DefaultValue))
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
                analyzer = TAnalyzer.Create(name, localRegion);
                _localAnalyzers.Add(name, analyzer);
                return true;
            }
        }

        public void StoreLocalAnalyzer(RegionAnalyzer analyzer)
        {
            _localAnalyzers[analyzer.Name] = analyzer;
            StoreLocalValue(analyzer.Name, analyzer.Selection);
        }

        public void StoreGlobalAnalyzer(RegionAnalyzer analyzer)
        {
            _globalAnalyzers[analyzer.Name] = analyzer;
            StoreGlobalValue(analyzer.Name, analyzer.Selection);
        }
    }
}
