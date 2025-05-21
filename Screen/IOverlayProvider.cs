namespace ScreenLab.Screen
{
    internal interface IOverlayProvider
    {
        void CloseOverlay();
        void ShowOverlay(Color? pointsColor);
        void ToggleOverlay(Color? pointsColor);
    }
}
