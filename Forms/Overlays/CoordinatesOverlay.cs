namespace ScreenLab.Forms.Overlays
{
    public partial class CoordinatesOverlay : Form
    {
        private readonly (Color color, Dictionary<string, Point> coords)[] groups;
        private readonly Dictionary<string, Point> globalCoords;
        private Font font = new Font("Arial", 10, FontStyle.Bold);
        private Brush backgroundBrush = new SolidBrush(Color.FromArgb(160, 0, 0, 0)); // Semi-transparent black

        public CoordinatesOverlay(params (Color color, Dictionary<string, Point> coords)[] groups)
        {
            this.groups = groups;

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
                // Let Windows do the default hit‐test first
                base.WndProc(ref m);

                // If the result is "client area" (HTCLIENT = 1), force it to be transparent
                if (m.Result == (IntPtr)1)
                {
                    m.Result = (IntPtr)(-1); // HTTRANSPARENT
                }
                return;
            }

            base.WndProc(ref m);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_EX_TRANSPARENT = 0x20;
                // (You may also want WS_EX_TOOLWINDOW = 0x80 to hide it from the Alt+Tab list, etc.)

                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_TRANSPARENT;
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            foreach (var group in groups)
            {
                using Brush textBrush = new SolidBrush(group.color);
                foreach (var item in group.coords)
                {
                    if (!item.Value.IsEmpty)
                        DrawLabeledPoint(g, item.Value, item.Key, font, textBrush, backgroundBrush);
                }
            }

            base.OnPaint(e);
        }

        private void DrawLabeledPoint(Graphics g, Point pt, string label, Font font, Brush textBrush, Brush bgBrush)
        {
            int radius = 4;
            g.FillEllipse(textBrush, pt.X - radius, pt.Y - radius, radius * 2, radius * 2);

            SizeF textSize = g.MeasureString(label, font);
            RectangleF bgRect = new RectangleF(pt.X + 6, pt.Y, textSize.Width, textSize.Height);

            g.FillRectangle(bgBrush, bgRect); // Draw semi-transparent background
            g.DrawString(label, font, Brushes.White, pt.X + 6, pt.Y); // White text over background
        }

        public void RefreshOverlay()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => this.Invalidate()));
            }
            else
            {
                this.Invalidate(); // Triggers OnPaint to redraw with updated coords
            }
        }
    }

}
