namespace ScreenLab.Forms.Overlays
{
    public partial class RegionSelectionOverlay : Form
    {
        private Point startPoint;
        private Rectangle selectedRegion;
        private bool isSelecting = false;

        public Rectangle SelectedRegion => selectedRegion;
        public event Action<Rectangle> RegionSelected;

        public RegionSelectionOverlay()
        {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            TopMost = true;  // Keep the form on top
            DoubleBuffered = true;

            // Form remains invisible but still captures mouse events
            BackColor = Color.Gray;
            Opacity = 0.3; // Fully transparent but captures input

            MouseDown += ScreenSelectionForm_MouseDown;
            MouseMove += ScreenSelectionForm_MouseMove;
            MouseUp += ScreenSelectionForm_MouseUp;
            KeyDown += ScreenSelectionForm_KeyDown;
        }

        private void ScreenSelectionForm_MouseDown(object sender, MouseEventArgs e)
        {
            isSelecting = true;
            startPoint = e.Location;
            selectedRegion = new Rectangle(startPoint, new Size(0, 0));
            Invalidate();
        }

        private void ScreenSelectionForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting)
            {
                int width = Math.Abs(e.X - startPoint.X);
                int height = Math.Abs(e.Y - startPoint.Y);
                selectedRegion = new Rectangle(
                    Math.Min(e.X, startPoint.X),
                    Math.Min(e.Y, startPoint.Y),
                    width,
                    height
                );
                Invalidate(); // Redraws selection box
            }
        }

        private void ScreenSelectionForm_MouseUp(object sender, MouseEventArgs e)
        {
            isSelecting = false;
            RegionSelected?.Invoke(selectedRegion);
            Close();
        }

        private void ScreenSelectionForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close(); // Close form when Esc is pressed
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true; // Mark key as handled
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!isSelecting) return;

            // Determine border color based on brightness of the captured screen
            Color borderColor = Color.Red; // Yellow stands out in both light and dark themes

            using (Pen borderPen = new Pen(borderColor, 4)) // Thicker border for visibility
            {
                borderPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash; // Dashed line

                using (Brush fillBrush = new SolidBrush(Color.FromArgb(80, Color.Blue))) // More opaque fill
                {
                    e.Graphics.FillRectangle(fillBrush, selectedRegion); // Fill the selection area
                    e.Graphics.DrawRectangle(borderPen, selectedRegion); // Draw a dashed border
                }
            }
        }
    }
}
