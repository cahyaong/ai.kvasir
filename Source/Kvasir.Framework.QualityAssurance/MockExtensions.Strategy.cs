namespace nGratis.AI.Kvasir.Framework;

using System.Collections.Generic;
using Moq;
using nGratis.AI.Kvasir.Engine;

public static partial class MockExtensions
{
    public static Mock<IStrategy> WithAttackingDecision(
        this Mock<IStrategy> mockStrategy,
        params ICard[] attackers)
    {
        mockStrategy
            .Setup(mock => mock.DeclareAttacker(Arg.IsAny<Tabletop>()))
            .Returns(new AttackingDecision
            {
                Attackers = attackers
            });

        return mockStrategy;
    }

    public static Mock<IStrategy> WithBlockingDecision(
        this Mock<IStrategy> mockStrategy,
        ICard attacker,
        IReadOnlyCollection<ICard> blockers)
    {
        var combat = new Combat
        {
            Attacker = attacker,
            Blockers = blockers
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