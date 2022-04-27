namespace nGratis.AI.Kvasir.Framework;

using System.Collections.Generic;
using Moq;
using nGratis.AI.Kvasir.Engine;

public static partial class MockExtensions
{
    public static Mock<IStrategy> WithAttackingDecision(this Mock<IStrategy> mockStrategy, Creature attacker)
    {
        mockStrategy
            .Setup(mock => mock.DeclareAttacker(Arg.IsAny<Tabletop>()))
            .Returns(new AttackingDecision
            {
                Attackers = new[] { attacker }
            });

        return mockStrategy;
    }

    public static Mock<IStrategy> WithBlockingStrategy(
        this Mock<IStrategy> mockStrategy,
        Creature attacker,
        IReadOnlyCollection<Creature> blockers)
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