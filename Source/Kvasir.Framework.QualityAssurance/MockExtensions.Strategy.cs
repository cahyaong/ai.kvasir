namespace nGratis.AI.Kvasir.Framework;

using System.Collections.Generic;
using Moq;
using nGratis.AI.Kvasir.Engine;

public static partial class MockExtensions
{
    public static Mock<IStrategy> WithAttackingDecision(
        this Mock<IStrategy> mockStrategy,
        params IPermanent[] attackingPermanents)
    {
        mockStrategy
            .Setup(mock => mock.DeclareAttacker(Arg.IsAny<Tabletop>()))
            .Returns(new AttackingDecision
            {
                AttackingPermanents = attackingPermanents
            });

        return mockStrategy;
    }

    public static Mock<IStrategy> WithBlockingDecision(
        this Mock<IStrategy> mockStrategy,
        IPermanent attackingPermanent,
        IReadOnlyCollection<IPermanent> blockingPermanents)
    {
        var combat = new Combat
        {
            AttackingPermanent = attackingPermanent,
            BlockingPermanents = blockingPermanents
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