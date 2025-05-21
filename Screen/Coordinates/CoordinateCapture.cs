using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace ScreenLab.Screen.Coordinates
{
    public class CoordinateCapture : CaptureBase<Point>, ICoordinateCaptureAnalyzable
    {

        private Dictionary<string, CoordinateAnalyzer> _localAnalyzers = new();
        private Dictionary<string, CoordinateAnalyzer> _globalAnalyzers = new();
        public override Point DefaultValue => Point.Empty;
        protected override string Title => "Coordinate";


        public CoordinateCapture(string appName, string storageDirName, params string[] keyNames)
            : base(appName, Path.Combine(storageDirName, "coordinates.json"), keyNames) {}


        protected override Dictionary<string, Point> DeserializeValues(string content)
        {
            return JsonSerializer.Deserialize<Dictionary<string, Point>>(content) ?? new();
        }

        protected override string SerializeValues(Dictionary<string, Point> values)
        {
            return JsonSerializer.Serialize(values, new JsonSerializerOptions { WriteIndented = true });
        }

        public void StoreLocalAnalyzer(CoordinateAnalyzer analyzer)
        {
            _localAnalyzers[analyzer.Name] = analyzer;
            StoreLocalValue(analyzer.Name, analyzer.Selection);
        }

        public void StoreGlobalAnalyzer(CoordinateAnalyzer analyzer)
        {
            _globalAnalyzers[analyzer.Name] = analyzer;
            StoreGlobalValue(analyzer.Name, analyzer.Selection);
        }

        public bool TryGetLocalAnalyzer<TAnalyzer>(string name, [MaybeNullWhen(false)] out TAnalyzer analyzer) 
            where TAnalyzer : CoordinateAnalyzer, ICoordinateAnalyzerFactory<TAnalyzer>
        {
            // Make sure there's a value for the local Name
            if (!TryGetLocalValue(name, out Point localPoint) && localPoint.Equals(DefaultValue))
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
                analyzer = TAnalyzer.Create(name, localPoint);
                _localAnalyzers.Add(name, analyzer);
                return true;
            }
        }

        public bool TryGetGlobalAnalyzer<TAnalyzer>(string name, [MaybeNullWhen(false)] out TAnalyzer analyzer) 
            where TAnalyzer : CoordinateAnalyzer, ICoordinateAnalyzerFactory<TAnalyzer>
        {
            // Make sure there's a value for the Global Name
            if (!TryGetGlobalValue(name, out Point globalPoint) && globalPoint.Equals(DefaultValue))
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
                analyzer = TAnalyzer.Create(name, globalPoint);
                _globalAnalyzers.Add(name, analyzer);
                return true;
            }
        }
    }

}
