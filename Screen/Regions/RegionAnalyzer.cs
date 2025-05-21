using ScreenLab.Forms.Overlays;

namespace ScreenLab.Screen.Regions
{
    public class RegionAnalyzer : AnalyzerBase<Rectangle>, IRegionAnalyzerFactory<RegionAnalyzer>, IOverlayProvider
    {
        public override Rectangle DefaultValue => Rectangle.Empty;
        public bool HasSelection { get => _selection.IsEmpty ? false : true; }

        private RegionsOverlay? _overlayForm = new();
        protected virtual bool overlayVisible => _overlayForm is not null && _overlayForm.IsDisposed;

        public Point CenterPoint 
        {
            get
            {
                int centerX = Selection.X + Selection.Width / 2;
                int centerY = Selection.Y + Selection.Height / 2;
                return new Point(centerX, centerY);
            }
        }

        public RegionAnalyzer(string name) : base(name) { }
        public RegionAnalyzer(string name, Rectangle selection) : base(name, selection) { }

        public override void BeginSelection(Action<Rectangle>? callback = null)
        {
            Rectangle selectedRect = new();

            RegionSelectionOverlay selectionForm = new RegionSelectionOverlay();
            selectionForm.RegionSelected += (rect) => selectedRect = rect;
            selectionForm.ShowDialog();

            SetSelection(selectedRect);
            callback?.Invoke(_selection);
        }
        
        
        protected virtual void ShowOverlayInternal(Color color)
        {
            CloseOverlay(); // Clear any existing overlays

            var overlay = new RegionsOverlay(color, _selection, Name);
            overlay.Show();
            _overlayForm = overlay;
        }

        public virtual void ToggleOverlay(Color? pointsColor)
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

        public virtual void ShowOverlay(Color? pointsColor)
        {
            if (overlayVisible) return;

            Color color = pointsColor ?? Color.Red;
            ShowOverlayInternal(color);
        }

        public virtual void CloseOverlay()
        {
            _overlayForm?.Close();
            _overlayForm = null;
        }
        public void SetSelectionCenteredAt(Point center, int width, int height)
        {
            int x = center.X - width / 2;
            int y = center.Y - height / 2;
            _selection = new Rectangle(x, y, width, height);
        }

        public static RegionAnalyzer Create(string name, Rectangle region) => new(name, region);

    }
}
