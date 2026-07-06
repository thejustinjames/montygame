using MontyGame.Core;

namespace MontyGame.Core.Tests;

public class DiceTests
{
    [Fact]
    public void Roll_RequestsOneToSixFromRandomSource()
    {
        var stub = new StubRandom(4);
        var dice = new Dice(stub);

        dice.Roll();

        Assert.Equal((Dice.MinFace, Dice.MaxFace + 1), stub.LastCall);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    public void Roll_ReturnsWhateverTheRandomSourceProduces(int value)
    {
        var dice = new Dice(new StubRandom(value));

        Assert.Equal(value, dice.Roll());
    }

    [Fact]
    public void Roll_WithSeededSystemRandom_StaysWithinDieFaceRange()
    {
        var dice = new Dice(new SystemRandom(seed: 42));

        for (var i = 0; i < 100; i++)
        {
            var roll = dice.Roll();
            Assert.InRange(roll, Dice.MinFace, Dice.MaxFace);
        }
    }

    [Fact]
    public void Roll_WithSameSeed_ProducesSameSequence()
    {
        var diceA = new Dice(new SystemRandom(seed: 7));
        var diceB = new Dice(new SystemRandom(seed: 7));

        var rollsA = Enumerable.Range(0, 20).Select(_ => diceA.Roll()).ToArray();
        var rollsB = Enumerable.Range(0, 20).Select(_ => diceB.Roll()).ToArray();

        Assert.Equal(rollsA, rollsB);
    }

    [Fact]
    public void Constructor_ThrowsWhenRandomIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new Dice(null!));
    }
}
