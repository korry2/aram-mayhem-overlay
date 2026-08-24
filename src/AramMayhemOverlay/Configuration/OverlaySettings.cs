using AramMayhemOverlay.Models;

namespace AramMayhemOverlay.Configuration;

public sealed class OverlaySettings
{
    public bool IsVisible { get; set; } = true;

    public double Opacity { get; set; } = 0.90;

    public double Width { get; set; } = 380;

    public double Height { get; set; } = 250;

    public double Left { get; set; } = 100;

    public double Top { get; set; } = 100;

    public OverlayInputMode InputMode { get; set; } =
        OverlayInputMode.Interactive;
}