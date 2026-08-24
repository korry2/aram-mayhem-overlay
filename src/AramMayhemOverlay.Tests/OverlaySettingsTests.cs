using AramMayhemOverlay.Configuration;
using AramMayhemOverlay.Models;
using Xunit;

namespace AramMayhemOverlay.Tests;

public class OverlaySettingsTests
{
    [Fact]
    public void DefaultSettings_HaveExpectedValues()
    {
        var settings = new OverlaySettings();

        Assert.True(settings.IsVisible);

        Assert.Equal(
            0.90,
            settings.Opacity);

        Assert.Equal(
            380,
            settings.Width);

        Assert.Equal(
            250,
            settings.Height);

        Assert.Equal(
            100,
            settings.Left);

        Assert.Equal(
            100,
            settings.Top);

        Assert.Equal(
            OverlayInputMode.Interactive,
            settings.InputMode);
    }
}