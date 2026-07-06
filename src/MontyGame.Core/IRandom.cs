namespace MontyGame.Core;

/// <summary>
/// Source of randomness for dice rolls and movement-card draws. Every random outcome
/// in the core flows through this interface — never <c>new Random()</c> inline — so
/// tests can supply deterministic sequences and playthroughs can be reproduced exactly.
/// </summary>
public interface IRandom
{
    /// <summary>Returns a random integer in the range [<paramref name="minInclusive"/>, <paramref name="maxExclusive"/>).</summary>
    int Next(int minInclusive, int maxExclusive);
}

/// <summary>
/// Default <see cref="IRandom"/>, backed by <see cref="System.Random"/>. Seedable so a
/// given playthrough (or test) can be replayed exactly.
/// </summary>
public sealed class SystemRandom : IRandom
{
    private readonly Random _random;

    /// <summary>Creates a generator seeded from system entropy (non-reproducible).</summary>
    public SystemRandom() => _random = new Random();

    /// <summary>Creates a generator seeded deterministically for reproducible sequences.</summary>
    public SystemRandom(int seed) => _random = new Random(seed);

    public int Next(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);
}
