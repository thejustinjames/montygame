using MontyGame.Core;

namespace MontyGame.Core.Tests;

public class GameEngineTests
{
    /// <summary>1 Normal, 2 Normal, 3 Boss, 4 Normal, 5 Normal/Goal — small board for controlled turn-loop tests.</summary>
    private static Board CreateBossBoard() => new(new[]
    {
        new Tile { Number = 1, Type = TileType.Normal, Name = "Start", Description = "Start", IsStart = true },
        new Tile { Number = 2, Type = TileType.Normal, Name = "Path", Description = "Path" },
        new Tile { Number = 3, Type = TileType.Boss, Name = "Boss", Description = "Boss" },
        new Tile { Number = 4, Type = TileType.Normal, Name = "Path", Description = "Path" },
        new Tile { Number = 5, Type = TileType.Normal, Name = "Goal", Description = "Goal", IsGoal = true },
    });

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void Constructor_ThrowsWhenPlayerCountOutOfRange(int playerCount)
    {
        var names = Enumerable.Range(1, playerCount).Select(i => $"Player {i}").ToList();

        Assert.Throws<ArgumentException>(() => new GameEngine(World1.CreateDinoJungleBoard(), new StubRandom(), names));
    }

    [Fact]
    public void Constructor_PlacesAllPlayersOnStartTile()
    {
        var engine = new GameEngine(World1.CreateDinoJungleBoard(), new StubRandom(), new[] { "Dino", "Cat" });

        Assert.All(engine.Players, p => Assert.Equal(1, p.Position));
        Assert.Equal(GameStatus.InProgress, engine.Status);
        Assert.Null(engine.Winner);
    }

    [Fact]
    public void TakeTurn_MovesCurrentPlayerByDiceRoll_OnNormalTile()
    {
        var engine = new GameEngine(CreateBossBoard(), new StubRandom(1), new[] { "Solo" });

        var result = engine.TakeTurn();

        Assert.Equal(1, result.DiceRoll);
        Assert.Equal(1, result.StartingPosition);
        Assert.Equal(2, result.FinalPosition);
        Assert.False(result.WonGame);
        Assert.False(result.AwaitingBossChallenge);
        Assert.Equal(GameStatus.InProgress, engine.Status);
    }

    [Fact]
    public void TakeTurn_ReachingGoalTile_WinsTheGame()
    {
        var engine = new GameEngine(CreateBossBoard(), new StubRandom(4), new[] { "Solo" });

        var result = engine.TakeTurn();

        Assert.Equal(5, result.FinalPosition);
        Assert.True(result.WonGame);
        Assert.Equal(GameStatus.Complete, engine.Status);
        Assert.Equal("Solo", engine.Winner?.Name);
    }

    [Fact]
    public void TakeTurn_AlternatesCurrentPlayerAcrossTurns()
    {
        var engine = new GameEngine(CreateBossBoard(), new StubRandom(1, 1), new[] { "Dino", "Cat" });

        Assert.Equal("Dino", engine.CurrentPlayer.Name);
        engine.TakeTurn();
        Assert.Equal("Cat", engine.CurrentPlayer.Name);
        engine.TakeTurn();
        Assert.Equal("Dino", engine.CurrentPlayer.Name);
    }

    [Fact]
    public void TakeTurn_RotatesThroughAllThreePlayersAndWrapsAround()
    {
        var engine = new GameEngine(CreateBossBoard(), new StubRandom(1, 1, 1, 1), new[] { "Dino", "Cat", "Robot" });

        var order = new List<string>();
        for (var i = 0; i < 4; i++)
        {
            order.Add(engine.CurrentPlayer.Name);
            engine.TakeTurn();
        }

        Assert.Equal(new[] { "Dino", "Cat", "Robot", "Dino" }, order);
    }

    [Fact]
    public void TakeTurn_RotatesThroughAllFourPlayersAndWrapsAround()
    {
        var engine = new GameEngine(CreateBossBoard(), new StubRandom(1, 1, 1, 1, 1), new[] { "Dino", "Cat", "Robot", "Alien" });

        var order = new List<string>();
        for (var i = 0; i < 5; i++)
        {
            order.Add(engine.CurrentPlayer.Name);
            engine.TakeTurn();
        }

        Assert.Equal(new[] { "Dino", "Cat", "Robot", "Alien", "Dino" }, order);
    }

    [Fact]
    public void TakeTurn_EachPlayerMovesIndependentlyWhileOthersWait()
    {
        // Dino and Robot roll 1 (Path); Cat rolls 2, reaching the Boss tile and pausing the loop.
        var engine = new GameEngine(CreateBossBoard(), new StubRandom(1, 2, 1), new[] { "Dino", "Cat", "Robot" });

        engine.TakeTurn();
        var catResult = engine.TakeTurn();

        Assert.Equal(2, engine.Players[0].Position);
        Assert.Equal(3, catResult.FinalPosition);
        Assert.True(catResult.AwaitingBossChallenge);
        Assert.Equal("Cat", engine.CurrentPlayer.Name);

        engine.ResolveBossChallenge(challengePassed: true);
        Assert.Equal("Robot", engine.CurrentPlayer.Name);
        engine.TakeTurn();
        Assert.Equal(2, engine.Players[2].Position);
    }

    [Fact]
    public void TakeTurn_LandingOnBossTile_PausesTurnLoopOnCurrentPlayer()
    {
        var engine = new GameEngine(CreateBossBoard(), new StubRandom(2), new[] { "Dino", "Cat" });

        var result = engine.TakeTurn();

        Assert.True(result.AwaitingBossChallenge);
        Assert.Equal(3, result.FinalPosition);
        Assert.Equal(GameStatus.AwaitingBossChallenge, engine.Status);
        Assert.Equal("Dino", engine.CurrentPlayer.Name);
    }

    [Fact]
    public void TakeTurn_ThrowsWhileBossChallengeIsPending()
    {
        var engine = new GameEngine(CreateBossBoard(), new StubRandom(2), new[] { "Solo" });
        engine.TakeTurn();

        Assert.Throws<InvalidOperationException>(() => engine.TakeTurn());
    }

    [Fact]
    public void ResolveBossChallenge_ThrowsWhenNoneIsPending()
    {
        var engine = new GameEngine(CreateBossBoard(), new StubRandom(), new[] { "Solo" });

        Assert.Throws<InvalidOperationException>(() => engine.ResolveBossChallenge(true));
    }

    [Fact]
    public void ResolveBossChallenge_WhenFailed_KeepsSamePlayerAwaitingRetry()
    {
        var engine = new GameEngine(CreateBossBoard(), new StubRandom(2), new[] { "Dino", "Cat" });
        engine.TakeTurn();

        var result = engine.ResolveBossChallenge(challengePassed: false);

        Assert.True(result.AwaitingBossChallenge);
        Assert.False(result.WonGame);
        Assert.Equal(3, result.FinalPosition);
        Assert.Equal(GameStatus.AwaitingBossChallenge, engine.Status);
        Assert.Equal("Dino", engine.CurrentPlayer.Name);
    }

    [Fact]
    public void ResolveBossChallenge_WhenPassed_AdvancesPastBossAndResumesTurnLoop()
    {
        var engine = new GameEngine(CreateBossBoard(), new StubRandom(2), new[] { "Dino", "Cat" });
        engine.TakeTurn();

        var result = engine.ResolveBossChallenge(challengePassed: true);

        Assert.False(result.AwaitingBossChallenge);
        Assert.Equal(4, result.FinalPosition);
        Assert.Equal(GameStatus.InProgress, engine.Status);
        Assert.Equal("Cat", engine.CurrentPlayer.Name);
    }

    [Fact]
    public void ResolveBossChallenge_PassingOntoGoalTile_WinsTheGame()
    {
        // Boss sits one tile before the goal on this tiny 5-tile board.
        var board = new Board(new[]
        {
            new Tile { Number = 1, Type = TileType.Normal, Name = "Start", Description = "Start", IsStart = true },
            new Tile { Number = 2, Type = TileType.Normal, Name = "Path", Description = "Path" },
            new Tile { Number = 3, Type = TileType.Normal, Name = "Path", Description = "Path" },
            new Tile { Number = 4, Type = TileType.Boss, Name = "Boss", Description = "Boss" },
            new Tile { Number = 5, Type = TileType.Normal, Name = "Goal", Description = "Goal", IsGoal = true },
        });
        var engine = new GameEngine(board, new StubRandom(3), new[] { "Solo" });
        engine.TakeTurn();

        var result = engine.ResolveBossChallenge(challengePassed: true);

        Assert.True(result.WonGame);
        Assert.Equal(5, result.FinalPosition);
        Assert.Equal(GameStatus.Complete, engine.Status);
    }

    [Fact]
    public void TakeTurn_ThrowsWhenGameIsAlreadyComplete()
    {
        var engine = new GameEngine(CreateBossBoard(), new StubRandom(4), new[] { "Solo" });
        engine.TakeTurn();

        Assert.Throws<InvalidOperationException>(() => engine.TakeTurn());
    }

    [Fact]
    public void Constructor_ThrowsWhenBoardIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new GameEngine(null!, new StubRandom(), new[] { "Solo" }));
    }

    [Fact]
    public void Constructor_ThrowsWhenRandomIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new GameEngine(CreateBossBoard(), null!, new[] { "Solo" }));
    }

    [Fact]
    public void Constructor_ThrowsWhenPlayerNamesIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new GameEngine(CreateBossBoard(), new StubRandom(), null!));
    }
}
