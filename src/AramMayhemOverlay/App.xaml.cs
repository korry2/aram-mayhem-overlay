using System.Windows;
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

        var mainWindow =
            new MainWindow(gameStateProvider);

        MainWindow = mainWindow;

        mainWindow.Show();
    }
}