namespace nGratis.AI.Kvasir.Framework;

using System.Collections.Generic;
using Moq;
using nGratis.AI.Kvasir.Engine;

public static partial class MockExtensions
{
    public static Mock<IStrategy> WithAttackingDecision(
        this Mock<IStrategy> mockStrategy,
        params ICard[] attackingCards)
    {
        mockStrategy
            .Setup(mock => mock.DeclareAttacker(Arg.IsAny<Tabletop>()))
            .Returns(new AttackingDecision
            {
                AttackingCards = attackingCards
            });

        return mockStrategy;
    }

    public static Mock<IStrategy> WithBlockingDecision(
        this Mock<IStrategy> mockStrategy,
        ICard attackingCard,
        IReadOnlyCollection<ICard> blockingCards)
    {
        var combat = new Combat
        {
            AttackingCard = attackingCard,
            BlockingCards = blockingCards
        };

        mockStrategy
            .Setup(mock => mock.DeclareBlocker(Arg.IsAny<Tabletop>()))
            .Returns(new BlockingDecision
            {
                Combats = new[] { combat }
            });

        return mockStrategy;
    }
}