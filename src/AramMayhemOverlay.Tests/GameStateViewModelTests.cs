using AramMayhemOverlay.Models;
using AramMayhemOverlay.UI;
using Xunit;

namespace AramMayhemOverlay.Tests;

public class GameStateViewModelTests
{
    [Fact]
    public void HealthValue_IsClampedToMaximumHealth()
    {
        var gameState = CreateGameState(
            currentHealth: 1900);

        var viewModel =
            new GameStateViewModel(gameState);

        Assert.Equal(
            1680,
            viewModel.HealthValue);

        Assert.Equal(
            1680,
            viewModel.MaxHealthValue);

        Assert.Equal(
            "1680 / 1680",
            viewModel.HealthText);
    }

    [Fact]
    public void HealthValue_IsClampedToZeroWhenNegative()
    {
        var gameState = CreateGameState(
            currentHealth: -100);

        var viewModel =
            new GameStateViewModel(gameState);

        Assert.Equal(
            0,
            viewModel.HealthValue);

        Assert.Equal(
            1680,
            viewModel.MaxHealthValue);

        Assert.Equal(
            "0 / 1680",
            viewModel.HealthText);
    }

    [Fact]
    public void HealthValue_PreservesValidHealth()
    {
        var gameState = CreateGameState(
            currentHealth: 1420);

        var viewModel =
            new GameStateViewModel(gameState);

        Assert.Equal(
            1420,
            viewModel.HealthValue);

        Assert.Equal(
            1680,
            viewModel.MaxHealthValue);

        Assert.Equal(
            "1420 / 1680",
            viewModel.HealthText);
    }

    private static GameState CreateGameState(
        int currentHealth)
    {
        return new GameState
        {
            ChampionName = "Test Champion",
            Level = 12,
            CurrentHealth = currentHealth,
            MaxHealth = 1680,
            MayhemModifier = "Test Modifier",
            StatusText = "Test"
        };
    }
}