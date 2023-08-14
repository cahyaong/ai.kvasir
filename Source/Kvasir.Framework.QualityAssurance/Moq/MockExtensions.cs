// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MockExtensions.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, April 22, 2022 2:53:41 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace Moq;

using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Engine;
using nGratis.AI.Kvasir.Framework;

public static partial class MockExtensions
{
    public static Mock<IStrategy> WithDefault(this Mock<IStrategy> mockStrategy)
    {
        mockStrategy
            .Setup(mock => mock.DeclareAttacker(Arg.IsAny<ITabletop>()))
            .Returns(AttackingDecision.None);

        mockStrategy
            .Setup(mock => mock.DeclareBlocker(Arg.IsAny<ITabletop>()))
            .Returns(BlockingDecision.None);

        mockStrategy
            .Setup(mock => mock.PerformPrioritizedAction(Arg.IsAny<ITabletop>()))
            .Returns(Action.Pass);

        mockStrategy
            .Setup(mock => mock.PerformNonPrioritizedAction(Arg.IsAny<ITabletop>()))
            .Returns(Action.Pass);

        return mockStrategy;
    }
}