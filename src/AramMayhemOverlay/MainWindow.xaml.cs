using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using AramMayhemOverlay.Models;
using AramMayhemOverlay.Services;

namespace AramMayhemOverlay;

public partial class MainWindow : Window
{
    private const int GWL_EXSTYLE = -20;

    private const long WS_EX_TRANSPARENT = 0x00000020L;
    private const long WS_EX_NOACTIVATE = 0x08000000L;

    private const int WM_HOTKEY = 0x0312;

    private const int HOTKEY_ID = 1001;
    private const int POSITION_HOTKEY_ID = 1002;

    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;

    private const uint VK_O = 0x4F;
    private const uint VK_P = 0x50;

    private const double TARGET_POLL_INTERVAL_MS = 100;

    private OverlayInputMode _inputMode =
        OverlayInputMode.Interactive;

    private readonly WindowPositionService _windowPositionService =
        new();

    private readonly DispatcherTimer _trackingTimer;

    private HwndSource? _hwndSource;

    private IntPtr _targetWindowHandle = IntPtr.Zero;

    public MainWindow()
    {
        InitializeComponent();

        _trackingTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(
                TARGET_POLL_INTERVAL_MS)
        };

        _trackingTimer.Tick += TrackingTimer_Tick;

        SourceInitialized += MainWindow_SourceInitialized;
        Closed += MainWindow_Closed;
    }

    private void MainWindow_SourceInitialized(
        object? sender,
        EventArgs e)
    {
        _hwndSource =
            HwndSource.FromHwnd(
                new WindowInteropHelper(this).Handle);

        if (_hwndSource is null)
        {
            throw new InvalidOperationException(
                "Overlay window handle could not be created.");
        }

        _hwndSource.AddHook(WindowProc);

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

        if (!hotkeyRegistered ||
            !positionHotkeyRegistered)
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

        if (_hwndSource is not null)
        {
            UnregisterHotKey(
                _hwndSource.Handle,
                HOTKEY_ID);

            UnregisterHotKey(
                _hwndSource.Handle,
                POSITION_HOTKEY_ID);

            _hwndSource.RemoveHook(WindowProc);
        }
    }

    private void OverlayBorder_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        if (_inputMode != OverlayInputMode.Interactive)
        {
            return;
        }

        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void ToggleInputMode()
    {
        _inputMode =
            _inputMode == OverlayInputMode.Interactive
                ? OverlayInputMode.Passive
                : OverlayInputMode.Interactive;

        ApplyInputMode();

        string message =
            _inputMode == OverlayInputMode.Interactive
                ? "Interactive mode"
                : "Passive mode";

        Title =
            $"ARAM Mayhem Overlay - {message}";
    }

    private void ApplyInputMode()
    {
        IntPtr handle =
            new WindowInteropHelper(this).Handle;

        long extendedStyle =
            GetWindowLongPtr(
                handle,
                GWL_EXSTYLE).ToInt64();

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
            new IntPtr(extendedStyle));
    }

    private void SelectTargetWindow()
    {
        IntPtr foregroundWindow =
            _windowPositionService
                .GetForegroundWindowHandle();

        IntPtr overlayHandle =
            new WindowInteropHelper(this).Handle;

        if (foregroundWindow ==
            IntPtr.Zero ||
            foregroundWindow == overlayHandle)
        {
            return;
        }

        if (!_windowPositionService
                .IsWindowValid(foregroundWindow))
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
                .IsWindowValid(_targetWindowHandle))
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
        if (!_windowPositionService.TryGetWindowBounds(
                _targetWindowHandle,
                out int left,
                out int top,
                out int width,
                out int height))
        {
            return;
        }

        Left = left;
        Top = top;

        Width = Math.Max(300, width);
        Height = Math.Max(180, height);
    }

    private IntPtr WindowProc(
        IntPtr hwnd,
        int message,
        IntPtr wParam,
        IntPtr lParam,
        ref bool handled)
    {
        if (message == WM_HOTKEY)
        {
            int hotkeyId =
                wParam.ToInt32();

            if (hotkeyId == HOTKEY_ID)
            {
                ToggleInputMode();
                handled = true;
            }
            else if (hotkeyId ==
                     POSITION_HOTKEY_ID)
            {
                SelectTargetWindow();
                handled = true;
            }
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
        EntryPoint = "SetWindowLongPtrW",
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
}