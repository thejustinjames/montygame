using MontyGame.Core;

namespace MontyGame.Core.Tests;

public class MovementCardsTests
{
    [Theory]
    [InlineData(MovementCard.Jump3, 3)]
    [InlineData(MovementCard.Dash4, 4)]
    [InlineData(MovementCard.SlowStep2, 2)]
    public void Spaces_MatchesDesignDocValues(MovementCard card, int expectedSpaces)
    {
        Assert.Equal(expectedSpaces, card.Spaces());
    }

    [Theory]
    [InlineData(0, MovementCard.Jump3)]
    [InlineData(1, MovementCard.Dash4)]
    [InlineData(2, MovementCard.SlowStep2)]
    public void Draw_ReturnsCardAtRandomIndex(int index, MovementCard expectedCard)
    {
        var deck = new MovementCardDeck(new StubRandom(index));

        Assert.Equal(expectedCard, deck.Draw());
    }

    [Fact]
    public void Draw_RequestsIndexWithinDeckBounds()
    {
        var stub = new StubRandom(0);
        var deck = new MovementCardDeck(stub);

        deck.Draw();

        Assert.Equal((0, 3), stub.LastCall);
    }

    [Fact]
    public void Draw_WithSeededSystemRandom_AlwaysReturnsADefinedCard()
    {
        var deck = new MovementCardDeck(new SystemRandom(seed: 99));
        var allCards = Enum.GetValues<MovementCard>();

        for (var i = 0; i < 50; i++)
        {
            Assert.Contains(deck.Draw(), allCards);
        }
    }

    [Fact]
    public void Constructor_ThrowsWhenRandomIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new MovementCardDeck(null!));
    }
}
