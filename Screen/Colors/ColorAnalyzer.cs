using ScreenLab.Input;
using ScreenLab.Utility;

namespace ScreenLab.Screen.Colors
{
    public class ColorAnalyzer : AnalyzerBase<Color>, IColorAnalyzerFactory<ColorAnalyzer>
    {
        public override Color DefaultValue => Color.Empty;

        public ColorAnalyzer(string name) : base(name)  { }
        public ColorAnalyzer(string name, Color selection) : base(name, selection) { }
        public override void BeginSelection(Action<Color>? callback = null)
        {
            MouseInput.HookClick((point) =>
            {
                SetSelection(RgbScreenUtil.GetPixelColor(point.X, point.Y));
                callback?.Invoke(_selection);
            });
        }
        public async Task BeginSelectionAsync()
        {
            Point point = await MouseInput.HookClickAsync();
            SetSelection(RgbScreenUtil.GetPixelColor(point.X, point.Y));
        }

        public static ColorAnalyzer Create(string name, Color color) => new(name, color);
    }
}
