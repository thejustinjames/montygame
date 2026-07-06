using MontyGame.Core;

namespace MontyGame.Core.Tests;

public class TileEffectsTests
{
    private static Board CreateBoard() => World1.CreateDinoJungleBoard();

    [Fact]
    public void Resolve_NormalTile_DoesNotMovePlayer()
    {
        var engine = new TileEffectsEngine(CreateBoard(), new StubRandom());

        var result = engine.Resolve(CreateBoard().GetTile(2), landedPosition: 2);

        Assert.Equal(TileEffectOutcome.None, result.Outcome);
        Assert.Equal(2, result.Position);
    }

    [Fact]
    public void Resolve_TimePortal_WarpsForwardToEffectTarget()
    {
        var board = CreateBoard();
        var engine = new TileEffectsEngine(board, new StubRandom());

        var result = engine.Resolve(board.GetTile(5), landedPosition: 5);

        Assert.Equal(TileEffectOutcome.Moved, result.Outcome);
        Assert.Equal(9, result.Position);
    }

    [Fact]
    public void Resolve_Whirlpool_PullsBackwardToEffectTarget()
    {
        var board = CreateBoard();
        var engine = new TileEffectsEngine(board, new StubRandom());

        var result = engine.Resolve(board.GetTile(8), landedPosition: 8);

        Assert.Equal(TileEffectOutcome.Moved, result.Outcome);
        Assert.Equal(5, result.Position);
    }

    [Fact]
    public void Resolve_Elevator_RisesToEffectTarget()
    {
        var board = CreateBoard();
        var engine = new TileEffectsEngine(board, new StubRandom());

        var result = engine.Resolve(board.GetTile(12), landedPosition: 12);

        Assert.Equal(TileEffectOutcome.Moved, result.Outcome);
        Assert.Equal(15, result.Position);
    }

    [Fact]
    public void Resolve_HyperspaceJump_WarpsForwardWhenRandomPicksZero()
    {
        var board = CreateBoard();
        var engine = new TileEffectsEngine(board, new StubRandom(0));

        var result = engine.Resolve(board.GetTile(18), landedPosition: 18);

        Assert.Equal(TileEffectOutcome.Moved, result.Outcome);
        Assert.Equal(21, result.Position);
    }

    [Fact]
    public void Resolve_HyperspaceJump_WarpsBackwardWhenRandomPicksOne()
    {
        var board = CreateBoard();
        var engine = new TileEffectsEngine(board, new StubRandom(1));

        var result = engine.Resolve(board.GetTile(18), landedPosition: 18);

        Assert.Equal(TileEffectOutcome.Moved, result.Outcome);
        Assert.Equal(14, result.Position);
    }

    [Fact]
    public void Resolve_HyperspaceJump_RequestsA50_50Pick()
    {
        var board = CreateBoard();
        var stub = new StubRandom(0);
        var engine = new TileEffectsEngine(board, stub);

        engine.Resolve(board.GetTile(18), landedPosition: 18);

        Assert.Equal((0, 2), stub.LastCall);
    }

    [Fact]
    public void Resolve_Mystery_DrawsACardAndMovesForwardByItsSpaces()
    {
        var board = CreateBoard();
        var mysteryTile = new Tile { Number = 10, Type = TileType.Mystery, Name = "Mystery", Description = "?" };
        var engine = new TileEffectsEngine(board, new StubRandom(1));

        var result = engine.Resolve(mysteryTile, landedPosition: 10);

        Assert.Equal(TileEffectOutcome.CardDrawn, result.Outcome);
        Assert.Equal(MovementCard.Dash4, result.DrawnCard);
        Assert.Equal(14, result.Position);
    }

    [Fact]
    public void Resolve_Mystery_ClampsToGoalWhenCardWouldOvershoot()
    {
        var board = CreateBoard();
        var mysteryTile = new Tile { Number = 24, Type = TileType.Mystery, Name = "Mystery", Description = "?" };
        var engine = new TileEffectsEngine(board, new StubRandom(1));

        var result = engine.Resolve(mysteryTile, landedPosition: 24);

        Assert.Equal(25, result.Position);
    }

    [Fact]
    public void Resolve_Boss_ReportsChallengeAndDoesNotMovePlayer()
    {
        var board = CreateBoard();
        var engine = new TileEffectsEngine(board, new StubRandom());

        var result = engine.Resolve(board.GetTile(24), landedPosition: 24);

        Assert.Equal(TileEffectOutcome.BossChallenge, result.Outcome);
        Assert.Equal(24, result.Position);
    }

    [Fact]
    public void ResolveBossChallenge_WhenPassed_AdvancesOneTile()
    {
        var engine = new TileEffectsEngine(CreateBoard(), new StubRandom());

        var result = engine.ResolveBossChallenge(bossTilePosition: 24, challengePassed: true);

        Assert.Equal(TileEffectOutcome.Moved, result.Outcome);
        Assert.Equal(25, result.Position);
    }

    [Fact]
    public void ResolveBossChallenge_WhenFailed_LeavesPlayerOnBossTile()
    {
        var engine = new TileEffectsEngine(CreateBoard(), new StubRandom());

        var result = engine.ResolveBossChallenge(bossTilePosition: 24, challengePassed: false);

        Assert.Equal(TileEffectOutcome.Moved, result.Outcome);
        Assert.Equal(24, result.Position);
    }

    [Fact]
    public void Resolve_ThrowsWhenTileWithMovementEffectHasNoTarget()
    {
        var engine = new TileEffectsEngine(CreateBoard(), new StubRandom());
        var brokenPortal = new Tile { Number = 3, Type = TileType.TimePortal, Name = "Broken", Description = "No target" };

        Assert.Throws<InvalidOperationException>(() => engine.Resolve(brokenPortal, landedPosition: 3));
    }

    [Fact]
    public void Resolve_ThrowsWhenTileIsNull()
    {
        var engine = new TileEffectsEngine(CreateBoard(), new StubRandom());

        Assert.Throws<ArgumentNullException>(() => engine.Resolve(null!, landedPosition: 1));
    }

    [Fact]
    public void Constructor_ThrowsWhenBoardIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new TileEffectsEngine(null!, new StubRandom()));
    }

    [Fact]
    public void Constructor_ThrowsWhenRandomIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new TileEffectsEngine(CreateBoard(), null!));
    }
}
