using System;
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

        double safeMaxHealth =
            Math.Max(0, gameState.MaxHealth);

        double safeCurrentHealth =
            Math.Clamp(
                gameState.CurrentHealth,
                0,
                safeMaxHealth);

        HealthText =
            $"{safeCurrentHealth:0} / {safeMaxHealth:0}";

        HealthValue = safeCurrentHealth;
        MaxHealthValue = safeMaxHealth;

        MayhemModifier = gameState.MayhemModifier;
        StatusText = gameState.StatusText;
    }
}