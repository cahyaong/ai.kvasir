// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KvasirAssertions.ExecutionResult.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Monday, July 8, 2024 7:03:10 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Framework;

using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Engine;

public class ExecutionResultAssertion : ReferenceTypeAssertions<ExecutionResult, ExecutionResultAssertion>
{
    public ExecutionResultAssertion(ExecutionResult subject)
        : base(subject)
    {
    }

    protected override string Identifier => "execution_result";

    public AndConstraint<ExecutionResultAssertion> CompleteWithoutReachingTerminalCondition()
    {
        using var _ = new AssertionScope();

        this.Subject.IsTerminal
            .Should().BeFalse("because execution should complete without reaching terminal condition");

        this.Subject.HasError
            .Should().BeFalse("because execution should complete without error");

        this.Subject.WinningPlayer
            .Should().Be(Player.None, "because execution should complete without winning player");

        return new AndConstraint<ExecutionResultAssertion>(this);
    }

    public AndConstraint<ExecutionResultAssertion> CompleteWithError()
    {
        using var _ = new AssertionScope();

        this.Subject.IsTerminal
            .Should().BeTrue("because execution should complete with terminal condition");

        this.Subject.HasError
            .Should().BeTrue("because execution should complete with error");

        this.Subject.WinningPlayer
            .Should().Be(Player.None, "because execution should complete without winning player");

        return new AndConstraint<ExecutionResultAssertion>(this);
    }

    public AndConstraint<ExecutionResultAssertion> CompleteWithWinningPlayer(IPlayer player)
    {
        using var _ = new AssertionScope();

        this.Subject.IsTerminal
            .Should().BeTrue("because execution should complete with terminal condition");

        this.Subject.HasError
            .Should().BeFalse("because execution should complete without error");

        this.Subject.WinningPlayer
            .Should().Be(player, "because execution should complete with winning player");

        return new AndConstraint<ExecutionResultAssertion>(this);
    }
}