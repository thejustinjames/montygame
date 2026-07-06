using MontyGame.Core;

namespace MontyGame.Core.Tests;

/// <summary>
/// Verifies that a <see cref="GameEngine"/> seeded with the same <see cref="SystemRandom"/>
/// seed replays byte-for-byte identically — the guarantee the CLI simulator and any future
/// "replay this game" / save-load feature depend on (Docs/WORKSHOP-CONTEXT.md: "All randomness
/// through an injected IRandom (seedable)").
/// </summary>
public class GameDeterminismTests
{
    private const int MaxTurnsBeforeGivingUp = 2000;

    [Fact]
    public void SameSeed_ProducesIdenticalFullPlaythrough_Solo()
    {
        var runA = PlayFullGame(seed: 2024, new[] { "Solo" });
        var runB = PlayFullGame(seed: 2024, new[] { "Solo" });

        AssertIdenticalOutcome(runA, runB);
    }

    [Fact]
    public void SameSeed_ProducesIdenticalFullPlaythrough_FourPlayers()
    {
        var names = new[] { "Dino", "Cat", "Robot", "Alien" };

        var runA = PlayFullGame(seed: 777, names);
        var runB = PlayFullGame(seed: 777, names);

        AssertIdenticalOutcome(runA, runB);
    }

    [Fact]
    public void SameSeed_ProducesIdenticalWinnerAndFinalPositions()
    {
        var names = new[] { "Dino", "Cat" };

        var engineA = PlayToCompletion(seed: 555, names, out var playersA);
        var engineB = PlayToCompletion(seed: 555, names, out var playersB);

        Assert.Equal(engineA.Winner?.Id, engineB.Winner?.Id);
        Assert.Equal(playersA.Select(p => p.Position), playersB.Select(p => p.Position));
    }

    [Fact]
    public void DifferentSeeds_CanDivergeInDiceSequence()
    {
        var rollsA = PlayFullGame(seed: 1, new[] { "Solo" }).Select(r => r.DiceRoll).ToArray();
        var rollsB = PlayFullGame(seed: 2, new[] { "Solo" }).Select(r => r.DiceRoll).ToArray();

        Assert.NotEqual(rollsA, rollsB);
    }

    private static List<TurnResult> PlayFullGame(int seed, IReadOnlyList<string> playerNames)
    {
        var engine = new GameEngine(World1.CreateDinoJungleBoard(), new SystemRandom(seed), playerNames);
        return DriveToCompletion(engine);
    }

    private static GameEngine PlayToCompletion(int seed, IReadOnlyList<string> playerNames, out IReadOnlyList<GamePlayer> players)
    {
        var engine = new GameEngine(World1.CreateDinoJungleBoard(), new SystemRandom(seed), playerNames);
        DriveToCompletion(engine);
        players = engine.Players;
        return engine;
    }

    /// <summary>
    /// Drives an engine to completion, always passing any boss challenge immediately. The
    /// pass/fail decision is a shell input (not part of the injected random source), so
    /// fixing it to "always pass" keeps the test's only source of variance the seeded RNG.
    /// </summary>
    private static List<TurnResult> DriveToCompletion(GameEngine engine)
    {
        var results = new List<TurnResult>();

        while (engine.Status != GameStatus.Complete)
        {
            if (results.Count >= MaxTurnsBeforeGivingUp)
            {
                throw new InvalidOperationException(
                    $"Game did not complete within {MaxTurnsBeforeGivingUp} turns; seeded playthrough may be looping.");
            }

            var result = engine.Status == GameStatus.AwaitingBossChallenge
                ? engine.ResolveBossChallenge(challengePassed: true)
                : engine.TakeTurn();

            results.Add(result);
        }

        return results;
    }

    private static void AssertIdenticalOutcome(IReadOnlyList<TurnResult> runA, IReadOnlyList<TurnResult> runB)
    {
        Assert.Equal(runA.Count, runB.Count);

        for (var i = 0; i < runA.Count; i++)
        {
            Assert.Equal(runA[i].PlayerId, runB[i].PlayerId);
            Assert.Equal(runA[i].DiceRoll, runB[i].DiceRoll);
            Assert.Equal(runA[i].StartingPosition, runB[i].StartingPosition);
            Assert.Equal(runA[i].LandedTile.Number, runB[i].LandedTile.Number);
            Assert.Equal(runA[i].TileEffect.Outcome, runB[i].TileEffect.Outcome);
            Assert.Equal(runA[i].FinalPosition, runB[i].FinalPosition);
            Assert.Equal(runA[i].WonGame, runB[i].WonGame);
        }
    }
}
