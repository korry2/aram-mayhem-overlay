using System;
using System.Runtime.InteropServices;

namespace AramMayhemOverlay.Services;

public sealed class WindowPositionService
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(
        IntPtr hWnd,
        out RECT lpRect);

    public IntPtr GetForegroundWindowHandle()
    {
        return GetForegroundWindow();
    }

    public bool IsWindowValid(IntPtr windowHandle)
    {
        return windowHandle != IntPtr.Zero &&
               IsWindow(windowHandle);
    }

    public bool TryGetWindowBounds(
        IntPtr windowHandle,
        out int left,
        out int top,
        out int width,
        out int height)
    {
        left = 0;
        top = 0;
        width = 0;
        height = 0;

        if (!IsWindowValid(windowHandle))
        {
            return false;
        }

        if (!GetWindowRect(
                windowHandle,
                out RECT rect))
        {
            return false;
        }

        left = rect.Left;
        top = rect.Top;
        width = rect.Right - rect.Left;
        height = rect.Bottom - rect.Top;

        return width > 0 && height > 0;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}