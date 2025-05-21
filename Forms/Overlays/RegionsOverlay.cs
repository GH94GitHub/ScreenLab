namespace ScreenLab.Forms.Overlays
{
    public partial class RegionsOverlay : Form
    {
        private readonly List<(Color color, Dictionary<string, Rectangle> regions)> groups;

        private Brush backgroundBrush = new SolidBrush(Color.FromArgb(160, 0, 0, 0)); // Semi-transparent black
        private Font font = new Font("Arial", 10, FontStyle.Bold);
        private static Color defaultColor = Color.Red;

        public RegionsOverlay(Rectangle rectangle, string label) : this(defaultColor, rectangle, label) { }
        public RegionsOverlay(Color color, Rectangle rectangle, string label) :
            this((color, new Dictionary<string, Rectangle>() { { label, rectangle } }))
        { }
        public RegionsOverlay(params (Color color, Dictionary<string, Rectangle>)[] groups)
        {
            this.groups = groups.ToList();

            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.BackColor = Color.Lime;
            this.TransparencyKey = Color.Lime;
            this.WindowState = FormWindowState.Maximized;
            this.DoubleBuffered = true;

            this.Load += (s, e) => this.BringToFront();
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x84;

            if (m.Msg == WM_NCHITTEST)
            {
                base.WndProc(ref m);

                if (m.Result == (IntPtr)1) // HTCLIENT
                    m.Result = (IntPtr)(-1); // HTTRANSPARENT
                return;
            }

            base.WndProc(ref m);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_EX_TRANSPARENT = 0x20;
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_TRANSPARENT;
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            foreach(var regions in groups)
            {
                using Brush textBrush = new SolidBrush(regions.color);
                foreach (var region in regions.regions)
                {
                    if (!region.Value.IsEmpty)
                        DrawLabeledRegion(g, region.Value, region.Key, font, textBrush, backgroundBrush);
                }
            }
            ;

            base.OnPaint(e);
        }

        private void DrawLabeledRegion(Graphics g, Rectangle rect, string label, Font font, Brush borderBrush, Brush bgBrush)
        {
            // Draw rectangle border
            using (Pen pen = new Pen(borderBrush, 2))
            {
                g.DrawRectangle(pen, rect);
            }

            // Measure label size
            SizeF textSize = g.MeasureString(label, font);

            // Determine label position
            float labelX = rect.Left;
            float labelY;

            if (rect.Top - textSize.Height - 2 < 0)
            {
                // Not enough space above, place inside
                labelY = rect.Top + 2; // a bit below top edge inside
            }
            else
            {
                // Place above the rectangle
                labelY = rect.Top - textSize.Height - 2;
            }

            // Draw label background and text
            RectangleF bgRect = new RectangleF(labelX, labelY, textSize.Width, textSize.Height);
            g.FillRectangle(bgBrush, bgRect);
            g.DrawString(label, font, Brushes.White, labelX, labelY);
        }

        public void AddRegion(Rectangle rectangle, string label) => 
            AddRegion(defaultColor, rectangle, label);

        public void AddRegion(Color color, Rectangle rectangle, string label) => 
            AddRegion((color, new Dictionary<string, Rectangle>() { { label, rectangle } }));

        public void AddRegion(params (Color color, Dictionary<string, Rectangle>)[] groups)
        {
            foreach (var group in groups)
            {
                this.groups.Add(group);
            }
            RefreshOverlay();
        }
        private void RefreshOverlay()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => this.Invalidate()));
            }
            else
            {
                this.Invalidate();
            }
        }
    }
}
