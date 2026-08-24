using System;
using System.IO;
using System.Text.Json;

namespace AramMayhemOverlay.Configuration;

public sealed class SettingsService : ISettingsService
{
    private const string ApplicationFolderName =
        "AramMayhemOverlay";

    private const string SettingsFileName =
        "settings.json";

    private readonly string _settingsFilePath;

    private readonly JsonSerializerOptions _jsonOptions =
        new()
        {
            WriteIndented = true
        };

    public SettingsService()
    {
        string localApplicationData =
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);

        string applicationFolder =
            Path.Combine(
                localApplicationData,
                ApplicationFolderName);

        Directory.CreateDirectory(applicationFolder);

        _settingsFilePath =
            Path.Combine(
                applicationFolder,
                SettingsFileName);
    }

    public SettingsService(string settingsFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            settingsFilePath);

        _settingsFilePath = settingsFilePath;

        string? directory =
            Path.GetDirectoryName(_settingsFilePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public OverlaySettings Load()
    {
        if (!File.Exists(_settingsFilePath))
        {
            return new OverlaySettings();
        }

        try
        {
            string json =
                File.ReadAllText(_settingsFilePath);

            OverlaySettings? settings =
                JsonSerializer.Deserialize<OverlaySettings>(
                    json,
                    _jsonOptions);

            return settings ??
                   new OverlaySettings();
        }
        catch
        {
            return new OverlaySettings();
        }
    }

    public void Save(OverlaySettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        string json =
            JsonSerializer.Serialize(
                settings,
                _jsonOptions);

        File.WriteAllText(
            _settingsFilePath,
            json);
    }
}