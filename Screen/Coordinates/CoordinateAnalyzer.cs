using ScreenLab.Input;

namespace ScreenLab.Screen.Coordinates
{
    public class CoordinateAnalyzer : AnalyzerBase<Point>, ICoordinateAnalyzerFactory<CoordinateAnalyzer>
    {
        public override Point DefaultValue => Point.Empty;

        public CoordinateAnalyzer(string name) :base(name) { }

        public CoordinateAnalyzer(string name, Point selection) : base(name, selection) { }

        public override void BeginSelection(Action<Point>? callback = null)
        {
            MouseInput.HookClick((point) => {
                SetSelection(point);
                callback?.Invoke(_selection);
            });
        }
        public async Task BeginSelectionAsync()
        {
            Point point = await MouseInput.HookClickAsync();
            SetSelection(point);
        }

        public static CoordinateAnalyzer Create(string name, Point point) => new(name, point);
    }
}
