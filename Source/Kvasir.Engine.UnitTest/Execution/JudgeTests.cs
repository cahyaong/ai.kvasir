// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JudgeTests.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2021 Cahya Ong
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// </copyright>
// <author>Cahya Ong - cahya.ong@gmail.com</author>
// <creation_timestamp>Sunday, May 15, 2022 7:00:13 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine.UnitTest.Execution;

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using nGratis.AI.Kvasir.Framework;
using nGratis.Cop.Olympus.Contract;
using Xunit;

public class JudgeTests
{
    public class ExecuteNextPhaseMethod_Beginning
    {
        [Fact]
        public void WhenEnteringBeginningPhase_ShouldSwitchActivePlayer()
        {
            // Arrange.

            var mockLogger = MockBuilder.CreateMock<ILogger>();

            var firstPlayer = StubBuilder.CreateDefaultPlayer("[_MOCK_PLAYER_ALPHA_]");
            var secondPlayer = StubBuilder.CreateDefaultPlayer("[_MOCK_PLAYER_OMEGA_]");

            var tabletop = new Tabletop
            {
                TurnId = 0,
                Phase = Phase.Ending,
                ActivePlayer = firstPlayer,
                NonactivePlayer = secondPlayer
            };

            var judge = new Judge(mockLogger.Object);

            // Act.

            var executionResult = judge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .HasError
                .Should().BeFalse();

            tabletop
                .ActivePlayer
                .Should().Be(secondPlayer, "because second player should be active");

            tabletop
                .NonactivePlayer
                .Should().Be(firstPlayer, "because first player should be nonactive");
        }

        [Fact]
        public void WhenEnteringUntapStep_ShouldUntapCreatureControlledByActivePlayer()
        {
            // Arrange.

            var mockLogger = MockBuilder.CreateMock<ILogger>();

            var firstPlayer = StubBuilder.CreateDefaultPlayer("[_MOCK_PLAYER_ALPHA_]");
            var secondPlayer = StubBuilder.CreateDefaultPlayer("[_MOCK_PLAYER_OMEGA_]");

            var tabletop = new Tabletop
            {
                TurnId = 0,
                Phase = Phase.Ending,
                ActivePlayer = firstPlayer,
                NonactivePlayer = secondPlayer
            };

            tabletop
                .AddDefaultCreatureToBattlefield("[_MOCK_CREATURE_01_]", secondPlayer, secondPlayer, false)
                .AddDefaultCreatureToBattlefield("[_MOCK_CREATURE_02_]", secondPlayer, secondPlayer, true)
                .AddDefaultCreatureToBattlefield("[_MOCK_CREATURE_11_]", firstPlayer, secondPlayer, false)
                .AddDefaultCreatureToBattlefield("[_MOCK_CREATURE_12_]", firstPlayer, secondPlayer, true);

            var judge = new Judge(mockLogger.Object);

            // Act.

            var executionResult = judge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .HasError
                .Should().BeFalse();

            using var _ = new AssertionScope();

            tabletop
                .Battlefield
                .FindAll()
                .Select(card => card.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .IsTapped
                        .Should().BeFalse($"because creature [{creature.Card.Name}] should be untapped");
                });
        }

        [Fact]
        public void WhenEnteringUntapStep_ShouldKeepStatusForCreatureControlledByNonactivePlayer()
        {
            // Arrange.

            var mockLogger = MockBuilder.CreateMock<ILogger>();

            var firstPlayer = StubBuilder.CreateDefaultPlayer("[_MOCK_PLAYER_ALPHA_]");
            var secondPlayer = StubBuilder.CreateDefaultPlayer("[_MOCK_PLAYER_OMEGA_]");

            var tabletop = new Tabletop
            {
                TurnId = 0,
                Phase = Phase.Ending,
                ActivePlayer = firstPlayer,
                NonactivePlayer = secondPlayer
            };

            tabletop
                .AddDefaultCreatureToBattlefield("[_MOCK_CREATURE_01_]", firstPlayer, firstPlayer, false)
                .AddDefaultCreatureToBattlefield("[_MOCK_CREATURE_02_]", firstPlayer, firstPlayer, true)
                .AddDefaultCreatureToBattlefield("[_MOCK_CREATURE_11_]", secondPlayer, firstPlayer, false)
                .AddDefaultCreatureToBattlefield("[_MOCK_CREATURE_12_]", secondPlayer, firstPlayer, true);

            var judge = new Judge(mockLogger.Object);

            // Act.

            var executionResult = judge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .HasError
                .Should().BeFalse();

            using var _ = new AssertionScope();

            tabletop
                .Battlefield
                .FindAll()
                .Where(card => card.Name.Contains("01") || card.Name.Contains("11"))
                .Select(card => card.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .IsTapped
                        .Should().BeFalse($"because creature [{creature.Card.Name}] should be keep status");
                });

            tabletop
                .Battlefield
                .FindAll()
                .Where(card => card.Name.Contains("02") || card.Name.Contains("12"))
                .Select(card => card.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .IsTapped
                        .Should().BeTrue($"because creature [{creature.Card.Name}] should be keep status");
                });
        }
    }

    public class ExecuteNextPhaseMethod_Combat
    {
        [Fact]
        public void WhenDeclaringAttacker_ShouldTapAttackingCreature()
        {
            // Arrange.

            var mockLogger = MockBuilder.CreateMock<ILogger>();
            var mockAttackingStrategy = MockBuilder.CreateMock<IStrategy>();
            var mockBlockingStrategy = MockBuilder.CreateMock<IStrategy>();

            var tabletop = StubBuilder.CreateDefaultTabletop(
                mockAttackingStrategy.Object,
                mockBlockingStrategy.Object);

            tabletop.Phase = Phase.PrecombatMain;

            var attackers = new[]
            {
                tabletop.CreateActiveCreature("[_MOCK_ATTACKER_01_]", 1, 1),
                tabletop.CreateActiveCreature("[_MOCK_ATTACKER_02_]", 2, 2),
                tabletop.CreateActiveCreature("[_MOCK_ATTACKER_02_]", 3, 3)
            };

            var blocker = tabletop.CreateNonactiveCreature("[_MOCK_BLOCKER_]", 0, 5);

            mockAttackingStrategy.WithAttackingDecision(attackers);
            mockBlockingStrategy.WithBlockingDecision(attackers.First(), new[] { blocker });

            var judge = new Judge(mockLogger.Object);

            // Act.

            var executionResult = judge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .HasError
                .Should().BeFalse();

            using var _ = new AssertionScope();

            tabletop
                .Battlefield
                .FindAll()
                .Where(card => card.Name.Contains("MOCK_ATTACKER"))
                .Select(card => card.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .IsTapped
                        .Should().BeTrue($"because attacking creature [{creature.Card.Name}] should be tapped");
                });
        }

        [Fact]
        public void WhenDeclaringAttacker_ShouldKeepStatusForNotAttackingCreature()
        {
            // Arrange.

            var mockLogger = MockBuilder.CreateMock<ILogger>();

            var tabletop = StubBuilder.CreateDefaultTabletop();
            tabletop.Phase = Phase.PrecombatMain;

            Enumerable
                .Range(1, 3)
                .ForEach(value =>
                {
                    tabletop
                        .CreateActiveCreature($"[_MOCK_TAPPED_{value}_]", value, value)
                        .ToProxyCreature()
                        .IsTapped = true;

                    tabletop
                        .CreateActiveCreature($"[_MOCK_UNTAPPED_{value}_]", value, value)
                        .ToProxyCreature()
                        .IsTapped = false;
                });

            var judge = new Judge(mockLogger.Object);

            // Act.

            var executionResult = judge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .HasError
                .Should().BeFalse();

            using var _ = new AssertionScope();

            tabletop
                .Battlefield
                .FindAll()
                .Where(card => card.Name.Contains("MOCK_TAPPED"))
                .Select(card => card.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .IsTapped
                        .Should().BeTrue($"because tapped creature [{creature.Card.Name}] should keep status");
                });

            tabletop
                .Battlefield
                .FindAll()
                .Where(card => card.Name.Contains("MOCK_UNTAPPED"))
                .Select(card => card.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .IsTapped
                        .Should().BeFalse($"because untapped creature [{creature.Card.Name}] should keep status");
                });
        }

        [Fact]
        public void WhenDeclaringBlocker_ShouldNotTapBlockingCreature()
        {
            // Arrange.

            var mockLogger = MockBuilder.CreateMock<ILogger>();
            var mockAttackingStrategy = MockBuilder.CreateMock<IStrategy>();
            var mockBlockingStrategy = MockBuilder.CreateMock<IStrategy>();

            var tabletop = StubBuilder.CreateDefaultTabletop(
                mockAttackingStrategy.Object,
                mockBlockingStrategy.Object);

            tabletop.Phase = Phase.PrecombatMain;

            var attacker = tabletop.CreateActiveCreature("[_MOCK_ATTACKER_]", 1, 1);

            var blockers = new[]
            {
                tabletop.CreateNonactiveCreature("[_MOCK_BLOCKER_01_]", 0, 11),
                tabletop.CreateNonactiveCreature("[_MOCK_BLOCKER_02_]", 0, 12),
                tabletop.CreateNonactiveCreature("[_MOCK_BLOCKER_02_]", 0, 13)
            };

            mockAttackingStrategy.WithAttackingDecision(attacker);
            mockBlockingStrategy.WithBlockingDecision(attacker, blockers);

            var judge = new Judge(mockLogger.Object);

            // Act.

            var executionResult = judge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .HasError
                .Should().BeFalse();

            using var _ = new AssertionScope();

            tabletop
                .Battlefield
                .FindAll()
                .Where(card => card.Name.Contains("MOCK_BLOCKER"))
                .Select(card => card.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .IsTapped
                        .Should().BeFalse($"because blocking creature [{creature.Card.Name}] should not be tapped");
                });
        }

        [Fact]
        public void WhenDeclaringBlocker_ShouldKeepStatusForNotBlockingCreature()
        {
            // Arrange.

            var mockLogger = MockBuilder.CreateMock<ILogger>();

            var tabletop = StubBuilder.CreateDefaultTabletop(Strategy.Noop, Strategy.Noop);
            tabletop.Phase = Phase.PrecombatMain;

            Enumerable
                .Range(1, 3)
                .ForEach(value =>
                {
                    tabletop
                        .CreateNonactiveCreature($"[_MOCK_TAPPED_{value}_]", value, value)
                        .ToProxyCreature()
                        .IsTapped = true;

                    tabletop
                        .CreateNonactiveCreature($"[_MOCK_UNTAPPED_{value}_]", value, value)
                        .ToProxyCreature()
                        .IsTapped = false;
                });

            var judge = new Judge(mockLogger.Object);

            // Act.

            var executionResult = judge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .HasError
                .Should().BeFalse();

            using var _ = new AssertionScope();

            tabletop
                .Battlefield
                .FindAll()
                .Where(card => card.Name.Contains("MOCK_TAPPED"))
                .Select(card => card.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .IsTapped
                        .Should().BeTrue($"because tapped creature [{creature.Card.Name}] should keep status");
                });

            tabletop
                .Battlefield
                .FindAll()
                .Where(card => card.Name.Contains("MOCK_UNTAPPED"))
                .Select(card => card.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .IsTapped
                        .Should().BeFalse($"because untapped creature [{creature.Card.Name}] should keep status");
                });
        }
    }
}