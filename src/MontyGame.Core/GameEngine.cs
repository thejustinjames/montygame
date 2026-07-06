namespace MontyGame.Core;

/// <summary>
/// One participant in a game: display name, current board position, and whether they've
/// already reached the goal. Mutated only by <see cref="GameEngine"/> as turns resolve —
/// a shell should treat this as read-only state to render.
/// </summary>
public sealed class GamePlayer
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public int Position { get; internal set; }

    public bool HasWon { get; internal set; }
}

/// <summary>Where a <see cref="GameEngine"/> is in its turn loop.</summary>
public enum GameStatus
{
    /// <summary>Waiting for <see cref="GameEngine.TakeTurn"/> from the current player.</summary>
    InProgress,

    /// <summary>
    /// The current player landed on a Boss tile; the shell must run the mini-challenge and
    /// report the result via <see cref="GameEngine.ResolveBossChallenge"/> before anyone
    /// else can take a turn.
    /// </summary>
    AwaitingBossChallenge,

    /// <summary>A player has reached the board's goal tile. See <see cref="GameEngine.Winner"/>.</summary>
    Complete
}

/// <summary>
/// Everything that happened during one call to <see cref="GameEngine.TakeTurn"/> or
/// <see cref="GameEngine.ResolveBossChallenge"/> — enough for a shell (CLI now, Unity
/// later) to narrate the turn and render the outcome.
/// </summary>
public sealed record TurnResult
{
    public required int PlayerId { get; init; }

    public required string PlayerName { get; init; }

    /// <summary>The die roll that produced this turn, or null when resolving a pending boss challenge.</summary>
    public int? DiceRoll { get; init; }

    /// <summary>Where the player stood before this turn/resolution was applied.</summary>
    public required int StartingPosition { get; init; }

    /// <summary>The tile the player landed on (before its effect, if any, was applied).</summary>
    public required Tile LandedTile { get; init; }

    public required TileEffectResult TileEffect { get; init; }

    /// <summary>Where the player ended up after the tile's effect was applied.</summary>
    public required int FinalPosition { get; init; }

    /// <summary>True if a boss challenge is still pending for this player (challenge not yet passed).</summary>
    public required bool AwaitingBossChallenge { get; init; }

    /// <summary>True if this turn brought the player to the board's goal tile.</summary>
    public required bool WonGame { get; init; }
}

/// <summary>
/// Drives the solo and pass-and-play turn loop (Docs/GAME_DESIGN_IDEATION.md "Core Loop"):
/// roll, move, apply the landed tile's effect, then pass to the next player — ending the
/// moment a player reaches the board's goal tile (tile 25 on World 1). Supports 1 player
/// (solo campaign) through 4 (local pass-and-play).
///
/// Landing on a Boss tile pauses the turn loop on the current player: <see cref="TakeTurn"/>
/// moves them onto the boss tile and then blocks until the shell reports the mini-challenge's
/// outcome through <see cref="ResolveBossChallenge"/>. A failed challenge is never a setback
/// (Docs/WORLD_1_LAYOUT.md: "fail = retry") — the player stays on the boss tile and can
/// attempt it again; only a pass advances them and lets the turn move on.
/// </summary>
public sealed class GameEngine
{
    public const int MinPlayers = 1;
    public const int MaxPlayers = 4;

    private readonly Board _board;
    private readonly Dice _dice;
    private readonly TileEffectsEngine _tileEffects;
    private readonly List<GamePlayer> _players;

    private int _currentPlayerIndex;
    private bool _awaitingBossChallenge;

    public GameEngine(Board board, IRandom random, IReadOnlyList<string> playerNames)
    {
        _board = board ?? throw new ArgumentNullException(nameof(board));
        if (random is null)
        {
            throw new ArgumentNullException(nameof(random));
        }

        if (playerNames is null)
        {
            throw new ArgumentNullException(nameof(playerNames));
        }

        if (playerNames.Count is < MinPlayers or > MaxPlayers)
        {
            throw new ArgumentException(
                $"MontyGame supports {MinPlayers}-{MaxPlayers} players (solo or local pass-and-play); got {playerNames.Count}.",
                nameof(playerNames));
        }

        _dice = new Dice(random);
        _tileEffects = new TileEffectsEngine(board, random);
        _players = playerNames
            .Select((name, i) => new GamePlayer { Id = i, Name = name, Position = board.StartTileNumber })
            .ToList();
    }

    public Board Board => _board;

    public IReadOnlyList<GamePlayer> Players => _players;

    /// <summary>The player whose turn it is (or who has a boss challenge pending).</summary>
    public GamePlayer CurrentPlayer => _players[_currentPlayerIndex];

    /// <summary>The player who reached the goal tile first, or null while the game is still in progress.</summary>
    public GamePlayer? Winner => _players.FirstOrDefault(p => p.HasWon);

    public GameStatus Status => Winner is not null
        ? GameStatus.Complete
        : _awaitingBossChallenge
            ? GameStatus.AwaitingBossChallenge
            : GameStatus.InProgress;

    /// <summary>
    /// Rolls the die for the current player, moves them, and applies the landed tile's
    /// effect. Advances to the next player unless the tile was a Boss — in which case the
    /// turn stays open on this player until <see cref="ResolveBossChallenge"/> is called.
    /// </summary>
    public TurnResult TakeTurn()
    {
        EnsureTurnCanBeTaken();

        var player = CurrentPlayer;
        var startingPosition = player.Position;
        var roll = _dice.Roll();
        var movedPosition = _board.ClampToBoard(player.Position + roll);
        var tile = _board.GetTile(movedPosition);
        var effect = _tileEffects.Resolve(tile, movedPosition);

        return Finish(player, startingPosition, roll, tile, effect,
            stillAwaitingBossChallenge: effect.Outcome == TileEffectOutcome.BossChallenge);
    }

    /// <summary>
    /// Reports the outcome of the current player's pending Boss mini-challenge. Passing
    /// advances them one tile past the boss and lets the turn loop continue to the next
    /// player; failing leaves them on the boss tile so the shell can let them try again.
    /// </summary>
    public TurnResult ResolveBossChallenge(bool challengePassed)
    {
        if (!_awaitingBossChallenge)
        {
            throw new InvalidOperationException("No boss challenge is currently pending.");
        }

        var player = CurrentPlayer;
        var startingPosition = player.Position;
        var tile = _board.GetTile(player.Position);
        var effect = _tileEffects.ResolveBossChallenge(player.Position, challengePassed);

        return Finish(player, startingPosition, diceRoll: null, tile, effect,
            stillAwaitingBossChallenge: !challengePassed);
    }

    private TurnResult Finish(
        GamePlayer player,
        int startingPosition,
        int? diceRoll,
        Tile tile,
        TileEffectResult effect,
        bool stillAwaitingBossChallenge)
    {
        player.Position = effect.Position;
        _awaitingBossChallenge = stillAwaitingBossChallenge;

        var won = !stillAwaitingBossChallenge && player.Position == _board.GoalTileNumber;
        if (won)
        {
            player.HasWon = true;
        }

        var result = new TurnResult
        {
            PlayerId = player.Id,
            PlayerName = player.Name,
            DiceRoll = diceRoll,
            StartingPosition = startingPosition,
            LandedTile = tile,
            TileEffect = effect,
            FinalPosition = player.Position,
            AwaitingBossChallenge = stillAwaitingBossChallenge,
            WonGame = won
        };

        if (!stillAwaitingBossChallenge)
        {
            AdvanceToNextPlayer();
        }

        return result;
    }

    private void EnsureTurnCanBeTaken()
    {
        if (Winner is not null)
        {
            throw new InvalidOperationException("The game is already complete; a player has reached the goal.");
        }

        if (_awaitingBossChallenge)
        {
            throw new InvalidOperationException(
                "A boss challenge is pending for the current player; call ResolveBossChallenge before taking another turn.");
        }
    }

    private void AdvanceToNextPlayer() =>
        _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
}
