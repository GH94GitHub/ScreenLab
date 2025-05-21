using ScreenLab.Forms.Overlays;

namespace ScreenLab.Utility
{
    public static class HeatMapUtil
    {
        private static HeatmapOverlay? heatmapForm;

        /// <summary>
        /// Toggles the heatmap overlay on or off.
        /// </summary>
        /// <param name="matchingPoints">Points to visualize on the screen.</param>
        /// <param name="color">Color to draw the heatmap points with. Defaults to red.</param>
        /// <param name="radius">Radius of each dot in pixels. Defaults to 3.</param>
        public static void ToggleHeatmap(List<Point> matchingPoints, Color? color = null, int radius = 3, int filterGroups = 0, Point? orderReferencePoint = null)
        {
            if (heatmapForm == null || heatmapForm.IsDisposed)
            {
                heatmapForm = new HeatmapOverlay(matchingPoints, color ?? Color.Red, radius, filterGroups: filterGroups, orderReferencePoint: orderReferencePoint);
                heatmapForm.Show();
            }
            else
            {
                heatmapForm.Close();
                heatmapForm = null;
            }
        }

        /// <summary>
        /// Updates the existing heatmap overlay with new points.
        /// </summary>
        /// <param name="newPoints">Updated list of points to visualize.</param>
        public static void UpdateHeatmap(List<Point> newPoints)
        {
            if (heatmapForm is not null && !heatmapForm.IsDisposed)
            {
                heatmapForm.UpdatePoints(newPoints);
            }
        }

        /// <summary>
        /// Closes the heatmap overlay if it's currently shown.
        /// </summary>
        public static void CloseHeatmap()
        {
            if (heatmapForm is not null && !heatmapForm.IsDisposed)
            {
                heatmapForm.Close();
                heatmapForm = null;
            }
        }

        /// <summary>
        /// Returns whether the heatmap is currently visible.
        /// </summary>
        public static bool IsHeatmapVisible => heatmapForm is not null && !heatmapForm.IsDisposed;
    }
}
