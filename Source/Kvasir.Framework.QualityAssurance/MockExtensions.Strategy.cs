﻿namespace nGratis.AI.Kvasir.Framework;

using System;
using System.Collections.Generic;
using Moq;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Engine;
using Action = nGratis.AI.Kvasir.Engine.Action;

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

    public static Mock<IStrategy> WithPerformingDefaultAction(this Mock<IStrategy> mockStrategy)
    {
        mockStrategy
            .Setup(mock => mock.PerformPrioritizedAction(Arg.IsAny<ITabletop>()))
            .Returns(Action.Pass);

        mockStrategy
            .Setup(mock => mock.PerformNonPrioritizedAction(Arg.IsAny<ITabletop>()))
            .Returns(Action.Pass);

        return mockStrategy;
    }

    public static Mock<IStrategy> WithPerformingPrioritizedAction(
        this Mock<IStrategy> mockStrategy,
        Func<ITabletop, IAction> selectAction)
    {
        mockStrategy
            .Setup(mock => mock.PerformPrioritizedAction(Arg.IsAny<ITabletop>()))
            .Returns(selectAction);

        return mockStrategy;
    }

    public static Mock<IStrategy> WithPerformingNonPrioritizedAction(
        this Mock<IStrategy> mockStrategy,
        Func<ITabletop, IAction> selectAction)
    {
        mockStrategy
            .Setup(mock => mock.PerformNonPrioritizedAction(Arg.IsAny<ITabletop>()))
            .Returns(selectAction);

        return mockStrategy;
    }
}