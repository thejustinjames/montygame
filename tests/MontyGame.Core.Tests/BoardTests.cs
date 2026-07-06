using MontyGame.Core;

namespace MontyGame.Core.Tests;

public class BoardTests
{
    [Fact]
    public void World1Board_Has25Tiles()
    {
        var board = World1.CreateDinoJungleBoard();

        Assert.Equal(25, board.TileCount);
    }

    [Fact]
    public void World1Board_TileNumbersAreContiguousFromOne()
    {
        var board = World1.CreateDinoJungleBoard();

        for (var i = 0; i < board.TileCount; i++)
        {
            Assert.Equal(i + 1, board.Tiles[i].Number);
        }
    }

    [Fact]
    public void World1Board_StartIsTileOneAndGoalIsTile25()
    {
        var board = World1.CreateDinoJungleBoard();

        Assert.Equal(1, board.StartTileNumber);
        Assert.Equal(25, board.GoalTileNumber);
        Assert.True(board.GetTile(1).IsStart);
        Assert.True(board.GetTile(25).IsGoal);
    }

    [Theory]
    [InlineData(5, TileType.TimePortal, 9)]
    [InlineData(8, TileType.Whirlpool, 5)]
    [InlineData(12, TileType.Elevator, 15)]
    [InlineData(15, TileType.TimePortal, 18)]
    [InlineData(21, TileType.Whirlpool, 17)]
    public void World1Board_MovementTilesHaveExpectedEffectTargets(int tileNumber, TileType expectedType, int expectedTarget)
    {
        var board = World1.CreateDinoJungleBoard();

        var tile = board.GetTile(tileNumber);

        Assert.Equal(expectedType, tile.Type);
        Assert.Equal(expectedTarget, tile.EffectTarget);
    }

    [Fact]
    public void World1Board_HyperspaceJumpTileHasForwardAndBackTargets()
    {
        var board = World1.CreateDinoJungleBoard();

        var tile = board.GetTile(18);

        Assert.Equal(TileType.HyperspaceJump, tile.Type);
        Assert.Equal(21, tile.EffectTarget);
        Assert.Equal(14, tile.AlternateEffectTarget);
    }

    [Fact]
    public void World1Board_BossTileIsTile24()
    {
        var board = World1.CreateDinoJungleBoard();

        var tile = board.GetTile(24);

        Assert.Equal(TileType.Boss, tile.Type);
    }

    [Fact]
    public void World1Board_NormalTilesHaveNoEffectTarget()
    {
        var board = World1.CreateDinoJungleBoard();

        foreach (var tile in board.Tiles.Where(t => t.Type == TileType.Normal))
        {
            Assert.Null(tile.EffectTarget);
            Assert.Null(tile.AlternateEffectTarget);
        }
    }

    [Fact]
    public void GetTile_ThrowsForOutOfRangeNumbers()
    {
        var board = World1.CreateDinoJungleBoard();

        Assert.Throws<ArgumentOutOfRangeException>(() => board.GetTile(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => board.GetTile(26));
    }

    [Fact]
    public void IsValidTileNumber_ReflectsBoardBounds()
    {
        var board = World1.CreateDinoJungleBoard();

        Assert.False(board.IsValidTileNumber(0));
        Assert.True(board.IsValidTileNumber(1));
        Assert.True(board.IsValidTileNumber(25));
        Assert.False(board.IsValidTileNumber(26));
    }

    [Theory]
    [InlineData(-5, 1)]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(12, 12)]
    [InlineData(25, 25)]
    [InlineData(30, 25)]
    public void ClampToBoard_NeverDropsBelowStartOrPastGoal(int candidate, int expected)
    {
        var board = World1.CreateDinoJungleBoard();

        Assert.Equal(expected, board.ClampToBoard(candidate));
    }

    [Fact]
    public void Constructor_ThrowsWhenTilesAreEmpty()
    {
        Assert.Throws<ArgumentException>(() => new Board(Array.Empty<Tile>()));
    }

    [Fact]
    public void Constructor_ThrowsWhenTileNumberingIsNotContiguous()
    {
        var tiles = new[]
        {
            new Tile { Number = 1, Type = TileType.Normal, Name = "A", Description = "A", IsStart = true },
            new Tile { Number = 3, Type = TileType.Normal, Name = "B", Description = "B", IsGoal = true },
        };

        Assert.Throws<ArgumentException>(() => new Board(tiles));
    }

    [Fact]
    public void Constructor_ThrowsWhenNoStartTile()
    {
        var tiles = new[]
        {
            new Tile { Number = 1, Type = TileType.Normal, Name = "A", Description = "A" },
            new Tile { Number = 2, Type = TileType.Normal, Name = "B", Description = "B", IsGoal = true },
        };

        Assert.Throws<ArgumentException>(() => new Board(tiles));
    }

    [Fact]
    public void Constructor_ThrowsWhenNoGoalTile()
    {
        var tiles = new[]
        {
            new Tile { Number = 1, Type = TileType.Normal, Name = "A", Description = "A", IsStart = true },
            new Tile { Number = 2, Type = TileType.Normal, Name = "B", Description = "B" },
        };

        Assert.Throws<ArgumentException>(() => new Board(tiles));
    }

    [Fact]
    public void Constructor_ThrowsWhenMultipleStartTiles()
    {
        var tiles = new[]
        {
            new Tile { Number = 1, Type = TileType.Normal, Name = "A", Description = "A", IsStart = true },
            new Tile { Number = 2, Type = TileType.Normal, Name = "B", Description = "B", IsStart = true, IsGoal = true },
        };

        Assert.Throws<ArgumentException>(() => new Board(tiles));
    }
}
