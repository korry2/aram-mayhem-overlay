using System.Windows;
using AramMayhemOverlay.Configuration;
using AramMayhemOverlay.Data;
using AramMayhemOverlay.Data.Mock;

namespace AramMayhemOverlay;

public partial class App : Application
{
    protected override void OnStartup(
        StartupEventArgs e)
    {
        base.OnStartup(e);

        IGameStateProvider gameStateProvider =
            new MockGameStateProvider();

        ISettingsService settingsService =
            new SettingsService();

        OverlaySettings settings =
            settingsService.Load();

        var mainWindow =
            new MainWindow(
                gameStateProvider,
                settings,
                settingsService);

        MainWindow = mainWindow;

        mainWindow.Show();
    }
}