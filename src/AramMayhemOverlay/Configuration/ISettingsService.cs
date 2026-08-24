namespace AramMayhemOverlay.Configuration;

public interface ISettingsService
{
    OverlaySettings Load();

    void Save(OverlaySettings settings);
}