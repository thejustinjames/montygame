namespace MontyGame.Core;

/// <summary>
/// An ordered sequence of tiles from tile 1 (start) to the final tile (goal). Boards are
/// built once (see <see cref="World1"/>) and then read-only for the rest of the game.
/// </summary>
public sealed class Board
{
    public IReadOnlyList<Tile> Tiles { get; }

    public int TileCount => Tiles.Count;

    public int StartTileNumber { get; }

    public int GoalTileNumber { get; }

    public Board(IReadOnlyList<Tile> tiles)
    {
        if (tiles is null || tiles.Count == 0)
        {
            throw new ArgumentException("A board must contain at least one tile.", nameof(tiles));
        }

        for (var i = 0; i < tiles.Count; i++)
        {
            var expectedNumber = i + 1;
            if (tiles[i].Number != expectedNumber)
            {
                throw new ArgumentException(
                    $"Tiles must be numbered contiguously starting at 1; expected tile {expectedNumber} at index {i} but found tile {tiles[i].Number}.",
                    nameof(tiles));
            }
        }

        var startTiles = tiles.Where(t => t.IsStart).ToList();
        if (startTiles.Count != 1)
        {
            throw new ArgumentException("A board must have exactly one start tile.", nameof(tiles));
        }

        var goalTiles = tiles.Where(t => t.IsGoal).ToList();
        if (goalTiles.Count != 1)
        {
            throw new ArgumentException("A board must have exactly one goal tile.", nameof(tiles));
        }

        Tiles = tiles;
        StartTileNumber = startTiles[0].Number;
        GoalTileNumber = goalTiles[0].Number;
    }

    /// <summary>True if <paramref name="tileNumber"/> falls within this board's range.</summary>
    public bool IsValidTileNumber(int tileNumber) => tileNumber >= 1 && tileNumber <= TileCount;

    /// <summary>Gets the tile at the given 1-based position.</summary>
    public Tile GetTile(int tileNumber)
    {
        if (!IsValidTileNumber(tileNumber))
        {
            throw new ArgumentOutOfRangeException(nameof(tileNumber), tileNumber,
                $"Tile number must be between 1 and {TileCount}.");
        }

        return Tiles[tileNumber - 1];
    }

    /// <summary>
    /// Clamps a candidate position to the board's playable range, honoring the
    /// forgiving-design rule that a player never drops below the start tile.
    /// </summary>
    public int ClampToBoard(int tileNumber) => Math.Clamp(tileNumber, StartTileNumber, GoalTileNumber);
}

/// <summary>
/// Factory for the World 1 "Dino Jungle" board: 25 tiles per
/// Docs/WORLD_1_LAYOUT.md, including tile names, descriptions, and the movement
/// effects for every portal, whirlpool, elevator, hyperspace jump, and boss tile.
/// </summary>
public static class World1
{
    public static Board CreateDinoJungleBoard()
    {
        var tiles = new List<Tile>
        {
            new() { Number = 1, Type = TileType.Normal, Name = "Jungle Entrance", Description = "Lush jungle entrance", IsStart = true },
            new() { Number = 2, Type = TileType.Normal, Name = "Forest Path", Description = "Forest path with ferns" },
            new() { Number = 3, Type = TileType.Normal, Name = "Moss-Covered Rocks", Description = "Moss-covered rocks" },
            new() { Number = 4, Type = TileType.Normal, Name = "Jungle Vines", Description = "Jungle vines" },
            new() { Number = 5, Type = TileType.TimePortal, Name = "Hidden Waterfall", Description = "Hidden waterfall (shimmers)", EffectTarget = 9 },
            new() { Number = 6, Type = TileType.Normal, Name = "Muddy Ground", Description = "Muddy ground; slightly slowed movement (visual only)" },
            new() { Number = 7, Type = TileType.Normal, Name = "Ancient Stone Ruin", Description = "Ancient stone ruin; hints at mystery" },
            new() { Number = 8, Type = TileType.Whirlpool, Name = "Swampy Vortex", Description = "Swampy vortex", EffectTarget = 5 },
            new() { Number = 9, Type = TileType.Normal, Name = "Vine Bridge", Description = "Vine bridge" },
            new() { Number = 10, Type = TileType.Normal, Name = "Bamboo Grove", Description = "Bamboo grove" },
            new() { Number = 11, Type = TileType.Normal, Name = "River Crossing", Description = "River crossing" },
            new() { Number = 12, Type = TileType.Elevator, Name = "Hanging Vine Lift", Description = "Hanging vine lift", EffectTarget = 15 },
            new() { Number = 13, Type = TileType.Normal, Name = "Fern Patch", Description = "Fern patch" },
            new() { Number = 14, Type = TileType.Normal, Name = "Stone Carved Path", Description = "Stone carved path" },
            new() { Number = 15, Type = TileType.TimePortal, Name = "Underground Cave Entrance", Description = "Underground cave entrance (glows)", EffectTarget = 18 },
            new() { Number = 16, Type = TileType.Normal, Name = "Jungle Canopy", Description = "Jungle canopy" },
            new() { Number = 17, Type = TileType.Normal, Name = "Hidden Waterhole", Description = "Hidden waterhole" },
            new() { Number = 18, Type = TileType.HyperspaceJump, Name = "Mysterious Glowing Orb", Description = "Mysterious glowing orb; random teleport forward or back", EffectTarget = 21, AlternateEffectTarget = 14 },
            new() { Number = 19, Type = TileType.Normal, Name = "Ancient Temple Steps", Description = "Ancient temple steps" },
            new() { Number = 20, Type = TileType.Normal, Name = "Overgrown Statue", Description = "Overgrown statue" },
            new() { Number = 21, Type = TileType.Whirlpool, Name = "Quicksand Pit", Description = "Quicksand pit", EffectTarget = 17 },
            new() { Number = 22, Type = TileType.Normal, Name = "Jungle Clearing", Description = "Jungle clearing" },
            new() { Number = 23, Type = TileType.Normal, Name = "Exotic Flowers", Description = "Exotic flowers" },
            new() { Number = 24, Type = TileType.Boss, Name = "Giant T-Rex Encounter", Description = "Mini-boss: pattern-dodge challenge (win = advance, fail = retry)" },
            new() { Number = 25, Type = TileType.Normal, Name = "Portal Key Chamber", Description = "Portal Key chamber (shimmers magically)", IsGoal = true },
        };

        return new Board(tiles);
    }
}
