namespace MontyGame.Core;

/// <summary>
/// Rolls the 1–6 die used on normal turns (Docs/GAME_DESIGN_IDEATION.md: "Roll the Die
/// or Draw a Card"). Lucky Spin / Mystery tiles draw a <see cref="MovementCard"/>
/// instead via <see cref="MovementCardDeck"/>.
/// </summary>
public sealed class Dice
{
    public const int MinFace = 1;
    public const int MaxFace = 6;

    private readonly IRandom _random;

    public Dice(IRandom random)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));
    }

    /// <summary>Rolls the die, returning a value from 1 to 6 inclusive.</summary>
    public int Roll() => _random.Next(MinFace, MaxFace + 1);
}
