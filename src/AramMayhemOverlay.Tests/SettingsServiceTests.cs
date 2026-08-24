using AramMayhemOverlay.Configuration;
using AramMayhemOverlay.Models;
using Xunit;

namespace AramMayhemOverlay.Tests;

public class SettingsServiceTests
{
    [Fact]
    public void SaveAndLoad_RoundTripsSettings()
    {
        string testDirectory =
            Path.Combine(
                Path.GetTempPath(),
                "AramMayhemOverlayTests",
                Guid.NewGuid().ToString());

        string settingsFilePath =
            Path.Combine(
                testDirectory,
                "settings.json");

        try
        {
            var originalSettings =
                new OverlaySettings
                {
                    IsVisible = false,
                    Opacity = 0.65,
                    Width = 720,
                    Height = 420,
                    Left = 250,
                    Top = 180,
                    InputMode = OverlayInputMode.Passive
                };

            var settingsService =
                new SettingsService(settingsFilePath);

            settingsService.Save(originalSettings);

            OverlaySettings loadedSettings =
                settingsService.Load();

            Assert.False(
                loadedSettings.IsVisible);

            Assert.Equal(
                0.65,
                loadedSettings.Opacity);

            Assert.Equal(
                720,
                loadedSettings.Width);

            Assert.Equal(
                420,
                loadedSettings.Height);

            Assert.Equal(
                250,
                loadedSettings.Left);

            Assert.Equal(
                180,
                loadedSettings.Top);

            Assert.Equal(
                OverlayInputMode.Passive,
                loadedSettings.InputMode);
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(
                    testDirectory,
                    recursive: true);
            }
        }
    }

    [Fact]
    public void Load_ReturnsDefaultsWhenFileDoesNotExist()
    {
        string testDirectory =
            Path.Combine(
                Path.GetTempPath(),
                "AramMayhemOverlayTests",
                Guid.NewGuid().ToString());

        string settingsFilePath =
            Path.Combine(
                testDirectory,
                "missing-settings.json");

        try
        {
            var settingsService =
                new SettingsService(settingsFilePath);

            OverlaySettings settings =
                settingsService.Load();

            Assert.True(settings.IsVisible);
            Assert.Equal(0.90, settings.Opacity);
            Assert.Equal(380, settings.Width);
            Assert.Equal(250, settings.Height);
            Assert.Equal(100, settings.Left);
            Assert.Equal(100, settings.Top);
            Assert.Equal(
                OverlayInputMode.Interactive,
                settings.InputMode);
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(
                    testDirectory,
                    recursive: true);
            }
        }
    }
}