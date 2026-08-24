namespace AramMayhemOverlay.Models;

public sealed class GameState
{
    public string ChampionName { get; init; } = string.Empty;

    public int Level { get; init; }

    public int CurrentHealth { get; init; }

    public int MaxHealth { get; init; }

    public string MayhemModifier { get; init; } = string.Empty;

    public string StatusText { get; init; } = string.Empty;
}