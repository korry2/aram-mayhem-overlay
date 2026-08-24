using AramMayhemOverlay.Models;

namespace AramMayhemOverlay.UI;

public sealed class GameStateViewModel
{
    public string ChampionName { get; }
    public string LevelText { get; }
    public string HealthText { get; }
    public double HealthValue { get; }
    public double MaxHealthValue { get; }
    public string MayhemModifier { get; }
    public string StatusText { get; }

    public GameStateViewModel(GameState gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        ChampionName = gameState.ChampionName;
        LevelText = $"LEVEL {gameState.Level}";
        HealthText =
            $"{gameState.CurrentHealth} / {gameState.MaxHealth}";

        HealthValue = gameState.CurrentHealth;
        MaxHealthValue = gameState.MaxHealth;

        MayhemModifier = gameState.MayhemModifier;
        StatusText = gameState.StatusText;
    }
}