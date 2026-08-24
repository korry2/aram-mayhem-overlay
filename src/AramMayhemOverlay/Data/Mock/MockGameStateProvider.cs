using AramMayhemOverlay.Models;

namespace AramMayhemOverlay.Data.Mock;

public sealed class MockGameStateProvider : IGameStateProvider
{
    public GameState GetCurrentGameState()
    {
        return new GameState
        {
            ChampionName = "Ahri",
            Level = 12,
            CurrentHealth = 1420,
            MaxHealth = 1680,
            MayhemModifier = "Chaos Amplifier",
            StatusText = "Mock game state"
        };
    }
}