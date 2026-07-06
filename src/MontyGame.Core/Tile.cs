namespace MontyGame.Core;

/// <summary>
/// The mechanical behavior a tile applies when a player lands on it.
/// </summary>
public enum TileType
{
    Normal,
    TimePortal,
    Whirlpool,
    Elevator,
    HyperspaceJump,
    Mystery,
    Boss
}

/// <summary>
/// A single space on a <see cref="Board"/>. Immutable — boards are built once by a
/// factory (e.g. <see cref="World1"/>) and read by the turn/tile-effects engine.
/// </summary>
public sealed record Tile
{
    /// <summary>1-based position on the board.</summary>
    public required int Number { get; init; }

    public required TileType Type { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    /// <summary>True for the single tile players spawn on (tile 1).</summary>
    public bool IsStart { get; init; }

    /// <summary>True for the single tile that wins the board (final tile).</summary>
    public bool IsGoal { get; init; }

    /// <summary>
    /// Destination tile for TimePortal/Whirlpool/Elevator effects, or the "forward"
    /// outcome for a HyperspaceJump. Null for tiles with no movement effect.
    /// </summary>
    public int? EffectTarget { get; init; }

    /// <summary>
    /// The "backward" outcome for a HyperspaceJump's 50/50 teleport. Null for every
    /// other tile type.
    /// </summary>
    public int? AlternateEffectTarget { get; init; }
}
