namespace MontyGame.Core;

/// <summary>
/// A movement card drawn instead of rolling the die (Docs/GAME_DESIGN_IDEATION.md:
/// "Jump 3", "Dash 4", "Slow Step 2"), drawn on Lucky Spin / Mystery tiles.
/// </summary>
public enum MovementCard
{
    Jump3,
    Dash4,
    SlowStep2
}

/// <summary>Movement distances for each <see cref="MovementCard"/>.</summary>
public static class MovementCardExtensions
{
    public static int Spaces(this MovementCard card) => card switch
    {
        MovementCard.Jump3 => 3,
        MovementCard.Dash4 => 4,
        MovementCard.SlowStep2 => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(card), card, "Unknown movement card.")
    };
}

/// <summary>
/// The deck of movement cards drawn on Lucky Spin / Mystery tiles. The deck never
/// depletes — every draw is an independent, uniformly random pick — so a young player
/// can always draw a card.
/// </summary>
public sealed class MovementCardDeck
{
    private static readonly MovementCard[] Cards =
    {
        MovementCard.Jump3,
        MovementCard.Dash4,
        MovementCard.SlowStep2
    };

    private readonly IRandom _random;

    public MovementCardDeck(IRandom random)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));
    }

    /// <summary>Draws a random movement card.</summary>
    public MovementCard Draw() => Cards[_random.Next(0, Cards.Length)];
}
