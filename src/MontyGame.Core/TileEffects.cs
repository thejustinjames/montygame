namespace MontyGame.Core;

/// <summary>
/// What kind of outcome a <see cref="TileEffectsEngine"/> produced when resolving a tile.
/// </summary>
public enum TileEffectOutcome
{
    /// <summary>Normal tile — no movement.</summary>
    None,

    /// <summary>TimePortal, Whirlpool, Elevator, or HyperspaceJump moved the player.</summary>
    Moved,

    /// <summary>Mystery tile drew a movement card and moved the player by its value.</summary>
    CardDrawn,

    /// <summary>Boss tile — progress is gated until the shell resolves its mini-challenge via <see cref="TileEffectsEngine.ResolveBossChallenge"/>.</summary>
    BossChallenge
}

/// <summary>
/// The result of resolving a tile's effect: where the player ends up, and what kind of
/// effect produced that position.
/// </summary>
public sealed record TileEffectResult
{
    public required TileEffectOutcome Outcome { get; init; }

    /// <summary>The player's position after the effect, always clamped to the board's tile range.</summary>
    public required int Position { get; init; }

    /// <summary>The card drawn, only set when <see cref="Outcome"/> is <see cref="TileEffectOutcome.CardDrawn"/>.</summary>
    public MovementCard? DrawnCard { get; init; }
}

/// <summary>
/// Applies the movement effect for whatever tile a player lands on
/// (Docs/GAME_DESIGN_IDEATION.md, Docs/WORLD_1_LAYOUT.md): TimePortal and Elevator warp
/// forward, Whirlpool pulls backward, HyperspaceJump is a 50/50 forward/backward warp,
/// Mystery draws a movement card, and Boss gates progress until its mini-challenge is
/// passed. Every resulting position is clamped to the board's playable range, so a
/// player can never be pushed below the start tile or past the goal — the forgiving,
/// no-eliminations rule that applies to every tile type.
/// </summary>
public sealed class TileEffectsEngine
{
    private readonly Board _board;
    private readonly IRandom _random;
    private readonly MovementCardDeck _cardDeck;

    public TileEffectsEngine(Board board, IRandom random)
    {
        _board = board ?? throw new ArgumentNullException(nameof(board));
        _random = random ?? throw new ArgumentNullException(nameof(random));
        _cardDeck = new MovementCardDeck(_random);
    }

    /// <summary>
    /// Resolves the effect of landing on <paramref name="tile"/>, having just moved to
    /// <paramref name="landedPosition"/>. Boss tiles do not move the player here — the
    /// shell runs the boss's mini-game and reports the outcome via
    /// <see cref="ResolveBossChallenge"/>.
    /// </summary>
    public TileEffectResult Resolve(Tile tile, int landedPosition)
    {
        if (tile is null)
        {
            throw new ArgumentNullException(nameof(tile));
        }

        return tile.Type switch
        {
            TileType.Normal => NoEffect(landedPosition),
            TileType.TimePortal => Moved(RequireTarget(tile)),
            TileType.Whirlpool => Moved(RequireTarget(tile)),
            TileType.Elevator => Moved(RequireTarget(tile)),
            TileType.HyperspaceJump => Moved(ResolveHyperspaceTarget(tile)),
            TileType.Mystery => ResolveMystery(landedPosition),
            TileType.Boss => new TileEffectResult { Outcome = TileEffectOutcome.BossChallenge, Position = NoEffect(landedPosition).Position },
            _ => throw new ArgumentOutOfRangeException(nameof(tile), tile.Type, "Unknown tile type.")
        };
    }

    /// <summary>
    /// Resolves a Boss tile's mini-challenge (Docs/WORLD_1_LAYOUT.md: "win = advance, fail
    /// = retry"). Passing advances one tile past the boss; failing leaves the player on the
    /// boss tile so they can try again — never a setback, just a hold.
    /// </summary>
    public TileEffectResult ResolveBossChallenge(int bossTilePosition, bool challengePassed) =>
        Moved(challengePassed ? bossTilePosition + 1 : bossTilePosition);

    private TileEffectResult ResolveMystery(int landedPosition)
    {
        var card = _cardDeck.Draw();
        return new TileEffectResult
        {
            Outcome = TileEffectOutcome.CardDrawn,
            Position = _board.ClampToBoard(landedPosition + card.Spaces()),
            DrawnCard = card
        };
    }

    private int ResolveHyperspaceTarget(Tile tile)
    {
        var forward = tile.EffectTarget ?? throw new InvalidOperationException(
            $"Tile {tile.Number} is a HyperspaceJump but has no forward EffectTarget.");
        var backward = tile.AlternateEffectTarget ?? throw new InvalidOperationException(
            $"Tile {tile.Number} is a HyperspaceJump but has no backward AlternateEffectTarget.");

        return _random.Next(0, 2) == 0 ? forward : backward;
    }

    private static int RequireTarget(Tile tile) =>
        tile.EffectTarget ?? throw new InvalidOperationException(
            $"Tile {tile.Number} ({tile.Type}) requires an EffectTarget but none was set.");

    private TileEffectResult NoEffect(int landedPosition) =>
        new() { Outcome = TileEffectOutcome.None, Position = _board.ClampToBoard(landedPosition) };

    private TileEffectResult Moved(int rawPosition) =>
        new() { Outcome = TileEffectOutcome.Moved, Position = _board.ClampToBoard(rawPosition) };
}
