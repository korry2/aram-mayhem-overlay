using System;
using System.Collections.Generic;
using System.Windows;
using AramMayhemOverlay.Configuration;
using AramMayhemOverlay.Models;

namespace AramMayhemOverlay;

public partial class SettingsWindow : Window
{
    private readonly OverlaySettings _currentSettings;

    private readonly Action<OverlaySettings> _saveSettings;

    public SettingsWindow(
        OverlaySettings currentSettings,
        Action<OverlaySettings> saveSettings)
    {
        ArgumentNullException.ThrowIfNull(
            currentSettings);

        ArgumentNullException.ThrowIfNull(
            saveSettings);

        _currentSettings =
            currentSettings;

        _saveSettings =
            saveSettings;

        InitializeComponent();

        InputModeComboBox.ItemsSource =
            new List<OverlayInputMode>
            {
                OverlayInputMode.Interactive,
                OverlayInputMode.Passive
            };

        LoadSettings(
            currentSettings);
    }

    private void LoadSettings(
        OverlaySettings settings)
    {
        OpacitySlider.Value =
            Math.Clamp(
                settings.Opacity,
                0.10,
                1.00);

        WidthTextBox.Text =
            settings.Width.ToString(
                "0");

        HeightTextBox.Text =
            settings.Height.ToString(
                "0");

        InputModeComboBox.SelectedItem =
            settings.InputMode;

        UpdateOpacityText();
    }

    private void UpdateOpacityText()
    {
        if (OpacityValueText is null ||
            OpacitySlider is null)
        {
            return;
        }

        OpacityValueText.Text =
            $"{OpacitySlider.Value:P0}";
    }

    private void OpacitySlider_ValueChanged(
        object sender,
        RoutedPropertyChangedEventArgs<double> e)
    {
        UpdateOpacityText();
    }

    private void SaveButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        StatusText.Text =
            string.Empty;

        if (!double.TryParse(
                WidthTextBox.Text,
                out double width))
        {
            StatusText.Text =
                "Width must be a valid number.";

            WidthTextBox.Focus();

            return;
        }

        if (!double.TryParse(
                HeightTextBox.Text,
                out double height))
        {
            StatusText.Text =
                "Height must be a valid number.";

            HeightTextBox.Focus();

            return;
        }

        if (width < 320)
        {
            StatusText.Text =
                "Width cannot be smaller than 320.";

            WidthTextBox.Focus();

            return;
        }

        if (height < 220)
        {
            StatusText.Text =
                "Height cannot be smaller than 220.";

            HeightTextBox.Focus();

            return;
        }

        if (InputModeComboBox.SelectedItem
            is not OverlayInputMode inputMode)
        {
            StatusText.Text =
                "Please select an input mode.";

            InputModeComboBox.Focus();

            return;
        }

        var settings =
            new OverlaySettings
            {
                IsVisible =
                    _currentSettings.IsVisible,

                Opacity =
                    OpacitySlider.Value,

                Width =
                    width,

                Height =
                    height,

                Left =
                    _currentSettings.Left,

                Top =
                    _currentSettings.Top,

                InputMode =
                    inputMode
            };

        _saveSettings(
            settings);

        Close();
    }

    private void ResetButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        LoadSettings(
            new OverlaySettings());

        StatusText.Text =
            "Defaults loaded. Press Save to apply them.";
    }

    private void CancelButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        Close();
    }
}