using AramMayhemOverlay.Models;

namespace AramMayhemOverlay.Data;

public interface IGameStateProvider
{
    GameState GetCurrentGameState();
}