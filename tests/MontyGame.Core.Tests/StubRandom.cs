using MontyGame.Core;

namespace MontyGame.Core.Tests;

/// <summary>Test double that plays back a fixed sequence of <see cref="IRandom.Next"/> results.</summary>
public sealed class StubRandom : IRandom
{
    private readonly Queue<int> _values;

    public StubRandom(params int[] values) => _values = new Queue<int>(values);

    public (int MinInclusive, int MaxExclusive)? LastCall { get; private set; }

    public int Next(int minInclusive, int maxExclusive)
    {
        LastCall = (minInclusive, maxExclusive);
        return _values.Dequeue();
    }
}
