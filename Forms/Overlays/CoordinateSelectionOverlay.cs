namespace ScreenLab.Forms.Overlays
{
    public partial class CoordinateSelectionOverlay : Form
    {
        private Point? selectedPoint;

        public CoordinateSelectionOverlay()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.Manual;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.DoubleBuffered = true;

            // Fullscreen on primary screen (adjust if multi-monitor needed)
            this.Bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;

            // Semi-transparent dark background
            this.BackColor = Color.Black;  // Solid black
            this.Opacity = 0.5;            // Works without per-pixel alpha

            this.MouseClick += Overlay_MouseClick;
        }
        private void Overlay_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                selectedPoint = e.Location; // Capture the clicked point
                this.DialogResult = DialogResult.OK;
                this.Close(); // Close overlay after selection
            }
        }

        public Point? GetSelectedPoint()
        {
            return selectedPoint;
        }
    }

}
