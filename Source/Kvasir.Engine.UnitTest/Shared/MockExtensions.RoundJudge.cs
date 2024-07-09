// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MockExtensions.RoundJudge.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Monday, July 8, 2024 1:16:29 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace Moq;

using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

using Arg = nGratis.AI.Kvasir.Framework.Arg;

internal static partial class MockExtensions
{
    public static Mock<IRoundJudge> WithDefault(this Mock<IRoundJudge> mockJudge)
    {
        mockJudge
            .Setup(mock => mock.ExecuteNextPhase(Arg.IsAny<ITabletop>()))
            .Callback<ITabletop>(tabletop => tabletop.TurnId += tabletop.Phase == Phase.Beginning ? 1 : 0)
            .Returns(ExecutionResult.SuccessfulWithoutWinner);

        mockJudge
            .Setup(mock => mock.ExecuteNextTurn(Arg.IsAny<ITabletop>()))
            .Callback<ITabletop>(tabletop => tabletop.TurnId++)
            .Returns(ExecutionResult.SuccessfulWithoutWinner);

        return mockJudge;
    }

    public static Mock<IRoundJudge> WithTerminalExecutionResult(this Mock<IRoundJudge> mockJudge, int turnId)
    {
        Guard
            .Require(turnId, nameof(turnId))
            .Is.Positive();

        mockJudge
            .Setup(mock => mock.ExecuteNextPhase(Arg.Tabletop.HasTurnWithId(turnId)))
            .Returns(ExecutionResult.Create(["[_MOCK_ERROR_MESSAGE_TO_SIMULATE_TERMINAL_CONDITION_]"]));

        mockJudge
            .Setup(mock => mock.ExecuteNextTurn(Arg.Tabletop.HasTurnWithId(turnId)))
            .Returns(ExecutionResult.Create(["[_MOCK_ERROR_MESSAGE_TO_SIMULATE_TERMINAL_CONDITION_]"]));

        return mockJudge;
    }
}