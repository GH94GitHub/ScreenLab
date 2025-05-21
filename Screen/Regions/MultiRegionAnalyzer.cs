using ScreenLab.Extensions;
using ScreenLab.Forms.Overlays;

namespace ScreenLab.Screen.Regions
{
    public class MultiRegionAnalyzer : RegionAnalyzer, IRegionAnalyzerFactory<MultiRegionAnalyzer>
    {
        private readonly List<Rectangle> _rectExclusions = new();
        private List<RegionsOverlay?> _overlayForms = new();
        protected override bool overlayVisible => _overlayForms.Any(o => o is not null && o.IsDisposed);
        public MultiRegionAnalyzer(string name) : base(name)
        {}

        public MultiRegionAnalyzer(string name, Rectangle selection) : base(name, selection)
        {}
        public MultiRegionAnalyzer(string name, Rectangle selection, params Rectangle[] exclusions) 
            : base(name, selection)
        {
            this._rectExclusions = new(exclusions);
        }

        public virtual void AddExclusion(params Rectangle[] exclusions)
        {
            foreach (Rectangle item in exclusions)
            {
                if (_rectExclusions.Contains(item))
                    continue;
                _rectExclusions.Add(item);
            }
        }
        public List<Rectangle> GetEffectiveRegion()
        {
            var result = new List<Rectangle> { _selection };

            foreach (var exclusion in _rectExclusions)
            {
                var updated = new List<Rectangle>();

                foreach (var rect in result)
                {
                    updated.AddRange(rect.SubtractRectangle(exclusion));
                }

                result = updated;
            }

            return result;
        }

        protected override void ShowOverlayInternal(Color color)
        {
            CloseOverlay(); // Clear any existing overlays

            var regionsToRender = _rectExclusions.Count == 0
                ? new List<Rectangle> { Selection }
                : GetEffectiveRegion();

            foreach (var rect in regionsToRender)
            {
                var overlay = new RegionsOverlay(color, rect, Name);
                overlay.Show();
                _overlayForms.Add(overlay);
            }
        }

        public override void ToggleOverlay(Color? pointsColor)
        {
            Color color = pointsColor ?? Color.Red;

            if (!HasSelection)
                BeginSelection();

            if (overlayVisible)
            {
                CloseOverlay();
            }
            else
            {
                ShowOverlayInternal(color);
            }
        }

        public override void ShowOverlay(Color? pointsColor)
        {
            if (overlayVisible) return;

            Color color = pointsColor ?? Color.Red;
            ShowOverlayInternal(color);
        }

        public override void CloseOverlay()
        {
            foreach (var overlay in _overlayForms)
            {
                overlay?.Close();
            }
            _overlayForms.Clear();
        }

        static MultiRegionAnalyzer IRegionAnalyzerFactory<MultiRegionAnalyzer>.Create(string name, Rectangle region) => new(name, region);
    }
}
