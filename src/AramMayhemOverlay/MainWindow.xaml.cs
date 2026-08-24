using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using AramMayhemOverlay.Configuration;
using AramMayhemOverlay.Data;
using AramMayhemOverlay.Models;
using AramMayhemOverlay.Services;
using AramMayhemOverlay.UI;

namespace AramMayhemOverlay;

public partial class MainWindow : Window
{
    private const int GWL_EXSTYLE = -20;

    private const long WS_EX_TRANSPARENT = 0x00000020L;
    private const long WS_EX_NOACTIVATE = 0x08000000L;

    private const int WM_HOTKEY = 0x0312;

    private const int HOTKEY_ID = 1001;
    private const int POSITION_HOTKEY_ID = 1002;
    private const int SETTINGS_HOTKEY_ID = 1003;

    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;

    private const uint VK_O = 0x4F;
    private const uint VK_P = 0x50;
    private const uint VK_S = 0x53;

    private const uint WM_NCLBUTTONDOWN = 0x00A1;
    private const int HTBOTTOMRIGHT = 17;

    private const double TARGET_POLL_INTERVAL_MS = 100;

    private OverlayInputMode _inputMode =
        OverlayInputMode.Interactive;

    private readonly IGameStateProvider _gameStateProvider;

    private readonly WindowPositionService _windowPositionService =
        new();

    private readonly ISettingsService _settingsService;

    private readonly OverlaySettings _settings;

    private readonly DispatcherTimer _trackingTimer;

    private HwndSource? _hwndSource;

    private IntPtr _targetWindowHandle =
        IntPtr.Zero;

    private SettingsWindow? _settingsWindow;

    public MainWindow(
        IGameStateProvider gameStateProvider,
        OverlaySettings settings,
        ISettingsService settingsService)
    {
        ArgumentNullException.ThrowIfNull(
            gameStateProvider);

        ArgumentNullException.ThrowIfNull(
            settings);

        ArgumentNullException.ThrowIfNull(
            settingsService);

        _gameStateProvider =
            gameStateProvider;

        _settings =
            settings;

        _settingsService =
            settingsService;

        _inputMode =
            _settings.InputMode;

        InitializeComponent();

        ApplySavedWindowSettings();

        _trackingTimer =
            new DispatcherTimer
            {
                Interval =
                    TimeSpan.FromMilliseconds(
                        TARGET_POLL_INTERVAL_MS)
            };

        _trackingTimer.Tick +=
            TrackingTimer_Tick;

        SourceInitialized +=
            MainWindow_SourceInitialized;

        Closed +=
            MainWindow_Closed;

        LoadGameStateViewModel();
    }

    private void ApplySavedWindowSettings()
    {
        Opacity =
            Math.Clamp(
                _settings.Opacity,
                0.1,
                1.0);

        Width =
            Math.Max(
                MinWidth,
                _settings.Width);

        Height =
            Math.Max(
                MinHeight,
                _settings.Height);

        Left =
            _settings.Left;

        Top =
            _settings.Top;

        if (_settings.IsVisible)
        {
            Visibility =
                Visibility.Visible;
        }
        else
        {
            Visibility =
                Visibility.Hidden;
        }
    }

    private void LoadGameStateViewModel()
    {
        GameState gameState =
            _gameStateProvider
                .GetCurrentGameState();

        DataContext =
            new GameStateViewModel(
                gameState);
    }

    private void MainWindow_SourceInitialized(
        object? sender,
        EventArgs e)
    {
        _hwndSource =
            HwndSource.FromHwnd(
                new WindowInteropHelper(this)
                    .Handle);

        if (_hwndSource is null)
        {
            throw new InvalidOperationException(
                "Overlay window handle could not be created.");
        }

        _hwndSource.AddHook(
            WindowProc);

        bool hotkeyRegistered =
            RegisterHotKey(
                _hwndSource.Handle,
                HOTKEY_ID,
                MOD_CONTROL | MOD_SHIFT,
                VK_O);

        bool positionHotkeyRegistered =
            RegisterHotKey(
                _hwndSource.Handle,
                POSITION_HOTKEY_ID,
                MOD_CONTROL | MOD_SHIFT,
                VK_P);

        bool settingsHotkeyRegistered =
            RegisterHotKey(
                _hwndSource.Handle,
                SETTINGS_HOTKEY_ID,
                MOD_CONTROL | MOD_SHIFT,
                VK_S);

        if (!hotkeyRegistered ||
            !positionHotkeyRegistered ||
            !settingsHotkeyRegistered)
        {
            MessageBox.Show(
                "One or more overlay hotkeys could not be registered.",
                "ARAM Mayhem Overlay",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        ApplyInputMode();
    }

    private void MainWindow_Closed(
        object? sender,
        EventArgs e)
    {
        _trackingTimer.Stop();

        _settingsWindow?.Close();

        SaveCurrentSettings();

        if (_hwndSource is not null)
        {
            UnregisterHotKey(
                _hwndSource.Handle,
                HOTKEY_ID);

            UnregisterHotKey(
                _hwndSource.Handle,
                POSITION_HOTKEY_ID);

            UnregisterHotKey(
                _hwndSource.Handle,
                SETTINGS_HOTKEY_ID);

            _hwndSource.RemoveHook(
                WindowProc);
        }
    }

    private void SaveCurrentSettings()
    {
        _settings.Opacity =
            Opacity;

        _settings.Width =
            Width;

        _settings.Height =
            Height;

        _settings.Left =
            Left;

        _settings.Top =
            Top;

        _settings.InputMode =
            _inputMode;

        _settingsService.Save(
            _settings);
    }

    private void OverlayBorder_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        if (_inputMode !=
            OverlayInputMode.Interactive)
        {
            return;
        }

        if (e.LeftButton ==
            MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void ResizeGrip_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        if (_inputMode !=
            OverlayInputMode.Interactive)
        {
            return;
        }

        if (_hwndSource is null)
        {
            return;
        }

        IntPtr handle =
            _hwndSource.Handle;

        ReleaseCapture();

        SendMessage(
            handle,
            WM_NCLBUTTONDOWN,
            new IntPtr(
                HTBOTTOMRIGHT),
            IntPtr.Zero);

        e.Handled = true;
    }

    private void ToggleInputMode()
    {
        _inputMode =
            _inputMode ==
            OverlayInputMode.Interactive
                ? OverlayInputMode.Passive
                : OverlayInputMode.Interactive;

        ApplyInputMode();

        _settings.InputMode =
            _inputMode;

        SaveCurrentSettings();

        string message =
            _inputMode ==
            OverlayInputMode.Interactive
                ? "Interactive mode"
                : "Passive mode";

        Title =
            $"ARAM Mayhem Overlay - {message}";
    }

    private void ApplyInputMode()
    {
        IntPtr handle =
            new WindowInteropHelper(this)
                .Handle;

        long extendedStyle =
            GetWindowLongPtr(
                handle,
                GWL_EXSTYLE)
            .ToInt64();

        if (_inputMode ==
            OverlayInputMode.Passive)
        {
            extendedStyle |=
                WS_EX_TRANSPARENT;

            extendedStyle |=
                WS_EX_NOACTIVATE;
        }
        else
        {
            extendedStyle &=
                ~WS_EX_TRANSPARENT;

            extendedStyle &=
                ~WS_EX_NOACTIVATE;
        }

        SetWindowLongPtr(
            handle,
            GWL_EXSTYLE,
            new IntPtr(
                extendedStyle));
    }

    private void SelectTargetWindow()
    {
        IntPtr foregroundWindow =
            _windowPositionService
                .GetForegroundWindowHandle();

        IntPtr overlayHandle =
            new WindowInteropHelper(this)
                .Handle;

        if (foregroundWindow ==
                IntPtr.Zero ||
            foregroundWindow ==
                overlayHandle)
        {
            return;
        }

        if (!_windowPositionService
                .IsWindowValid(
                    foregroundWindow))
        {
            return;
        }

        _targetWindowHandle =
            foregroundWindow;

        UpdateOverlayPosition();

        _trackingTimer.Start();
    }

    private void TrackingTimer_Tick(
        object? sender,
        EventArgs e)
    {
        if (_targetWindowHandle ==
            IntPtr.Zero)
        {
            _trackingTimer.Stop();
            return;
        }

        if (!_windowPositionService
                .IsWindowValid(
                    _targetWindowHandle))
        {
            _targetWindowHandle =
                IntPtr.Zero;

            _trackingTimer.Stop();

            return;
        }

        UpdateOverlayPosition();
    }

    private void UpdateOverlayPosition()
    {
        if (!_windowPositionService
                .TryGetWindowBounds(
                    _targetWindowHandle,
                    out int left,
                    out int top,
                    out int width,
                    out int height))
        {
            return;
        }

        Left =
            left;

        Top =
            top;

        Width =
            Math.Max(
                MinWidth,
                width);

        Height =
            Math.Max(
                MinHeight,
                height);
    }

    private void OpenSettingsWindow()
    {
        if (_settingsWindow is not null)
        {
            if (_settingsWindow.WindowState ==
                WindowState.Minimized)
            {
                _settingsWindow.WindowState =
                    WindowState.Normal;
            }

            _settingsWindow.Activate();

            return;
        }

        _settingsWindow =
            new SettingsWindow(
                _settings,
                ApplySettingsFromWindow);

        _settingsWindow.Closed +=
            SettingsWindow_Closed;

        _settingsWindow.Topmost = true;

        _settingsWindow.Show();
    }

    private void SettingsWindow_Closed(
        object? sender,
        EventArgs e)
    {
        if (_settingsWindow is not null)
        {
            _settingsWindow.Closed -=
                SettingsWindow_Closed;
        }

        _settingsWindow = null;
    }

    private void ApplySettingsFromWindow(
        OverlaySettings newSettings)
    {
        _settings.Opacity =
            Math.Clamp(
                newSettings.Opacity,
                0.1,
                1.0);

        _settings.Width =
            Math.Max(
                MinWidth,
                newSettings.Width);

        _settings.Height =
            Math.Max(
                MinHeight,
                newSettings.Height);

        _settings.Left =
            newSettings.Left;

        _settings.Top =
            newSettings.Top;

        _settings.InputMode =
            newSettings.InputMode;

        Opacity =
            _settings.Opacity;

        Width =
            _settings.Width;

        Height =
            _settings.Height;

        Left =
            _settings.Left;

        Top =
            _settings.Top;

        _inputMode =
            _settings.InputMode;

        ApplyInputMode();

        _settingsService.Save(
            _settings);
    }

    private IntPtr WindowProc(
        IntPtr hwnd,
        int message,
        IntPtr wParam,
        IntPtr lParam,
        ref bool handled)
    {
        if (message != WM_HOTKEY)
        {
            return IntPtr.Zero;
        }

        int hotkeyId =
            wParam.ToInt32();

        switch (hotkeyId)
        {
            case HOTKEY_ID:
                ToggleInputMode();
                handled = true;
                break;

            case POSITION_HOTKEY_ID:
                SelectTargetWindow();
                handled = true;
                break;

            case SETTINGS_HOTKEY_ID:
                OpenSettingsWindow();
                handled = true;
                break;
        }

        return IntPtr.Zero;
    }

    [DllImport(
        "user32.dll",
        EntryPoint = "GetWindowLongPtrW",
        SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr(
        IntPtr hWnd,
        int nIndex);

    [DllImport(
        "user32.dll",
        SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr(
        IntPtr hWnd,
        int nIndex,
        IntPtr dwNewLong);

    [DllImport(
        "user32.dll",
        SetLastError = true)]
    private static extern bool RegisterHotKey(
        IntPtr hWnd,
        int id,
        uint fsModifiers,
        uint vk);

    [DllImport(
        "user32.dll",
        SetLastError = true)]
    private static extern bool UnregisterHotKey(
        IntPtr hWnd,
        int id);

    [DllImport(
        "user32.dll",
        SetLastError = true)]
    private static extern bool ReleaseCapture();

    [DllImport(
        "user32.dll",
        SetLastError = true)]
    private static extern IntPtr SendMessage(
        IntPtr hWnd,
        uint message,
        IntPtr wParam,
        IntPtr lParam);
}