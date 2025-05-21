using ScreenLab.Extensions;
using System.Drawing.Drawing2D;

namespace ScreenLab.Forms.Overlays
{
    public partial class HeatmapOverlay : Form
    {
        private List<Point> points;
        private Font font = new Font("Arial", 20);
        private Brush textBrush = Brushes.Black;
        private int filterGroups;
        private readonly Brush firstBrush;
        private readonly Brush mostBrush;
        private readonly Brush centerBrush;
        private readonly Brush lastBrush;
        private readonly int radius;
        private readonly Point? orderReferencePoint;


        public HeatmapOverlay(List<Point> initialPoints, Color color, int radius = 4, bool use8Directions = false, int filterGroups = 0, Point? orderReferencePoint = null)
        {
            this.points = initialPoints;
            this.firstBrush = new SolidBrush(Color.Green);
            this.mostBrush = new SolidBrush(color);
            this.centerBrush = new SolidBrush(Color.White);
            this.lastBrush = new SolidBrush(Color.Black);
            this.radius = radius;

            this.filterGroups = filterGroups;
            this.orderReferencePoint = orderReferencePoint;

            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.DoubleBuffered = true;
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;
            this.Bounds = GetFullScreenBounds();
        }

        public void UpdatePoints(List<Point> newPoints)
        {
            this.points = newPoints;
            this.Invalidate();
        }

        private Rectangle GetFullScreenBounds()
        {
            return System.Windows.Forms.Screen.AllScreens
                .Select(s => s.Bounds)
                .Aggregate(Rectangle.Union);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var connectedGroups = points.FindConnectedGoups();
            connectedGroups = orderReferencePoint is Point referencePoint ? connectedGroups.OrderGroupsByProximity(referencePoint) : connectedGroups;
            if (filterGroups > 0)
                connectedGroups = connectedGroups.Where((group) => group.Count > filterGroups).ToList();


            for (int g = 0; g < connectedGroups.Count; g++)
            {
                var group = connectedGroups[g];
                if (group.Count == 0) continue;

                // Draw group label at last point of group
                var labelPoint = group[group.Count-1];
                string groupLabel = $"Group {g + 1}";
                PointF labelPosition = new PointF(labelPoint.X + radius + 2, labelPoint.Y - (font.Height / 2));
                e.Graphics.DrawString(groupLabel, font, textBrush, labelPosition);

                // Draw points in group
                for (int i = 0; i < group.Count; i++)
                {
                    Brush ellipseBrush;

                    if (i == 0)
                        ellipseBrush = firstBrush;
                    else if (i == group.Count / 2)
                        ellipseBrush = centerBrush;
                    else if (i == group.Count - 1)
                        ellipseBrush = lastBrush;
                    else
                        ellipseBrush = mostBrush;

                    e.Graphics.FillEllipse(ellipseBrush, group[i].X - radius, group[i].Y - radius, radius * 2, radius * 2);
                }
                
            }
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_EX_TRANSPARENT = 0x20;
                const int WS_EX_LAYERED = 0x80000;

                var cp = base.CreateParams;
                cp.ExStyle |= WS_EX_TRANSPARENT | WS_EX_LAYERED;
                return cp;
            }
        }
    }
}
