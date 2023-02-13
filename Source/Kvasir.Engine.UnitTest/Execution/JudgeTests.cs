﻿// --------------------------------------------------------------------------------------------------------------------
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

namespace nGratis.AI.Kvasir.Engine.UnitTest;

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

            var firstPlayer = StubBuilder.CreateDefaultPlayer("[_MOCK_PLAYER__ALPHA_]");
            var secondPlayer = StubBuilder.CreateDefaultPlayer("[_MOCK_PLAYER__OMEGA_]");

            var tabletop = new Tabletop
            {
                TurnId = 0,
                Phase = Phase.Ending,
                ActivePlayer = firstPlayer,
                NonActivePlayer = secondPlayer
            };

            tabletop.WithDefaultLibrary();

            var judge = new Judge(mockLogger.Object);

            // Act.

            var executionResult = judge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .HasError
                .Should().BeFalse("because execution should complete without error");

            tabletop
                .ActivePlayer
                .Should().Be(secondPlayer, "because second player should be active");

            tabletop
                .NonActivePlayer
                .Should().Be(firstPlayer, "because first player should be nonactive");
        }

        [Fact]
        public void WhenEnteringUntapStep_ShouldUntapCreatureControlledByActivePlayer()
        {
            // Arrange.

            var mockLogger = MockBuilder.CreateMock<ILogger>();

            var firstPlayer = StubBuilder.CreateDefaultPlayer("[_MOCK_PLAYER__ALPHA_]");
            var secondPlayer = StubBuilder.CreateDefaultPlayer("[_MOCK_PLAYER__OMEGA_]");

            var tabletop = new Tabletop
            {
                TurnId = 0,
                Phase = Phase.Ending,
                ActivePlayer = firstPlayer,
                NonActivePlayer = secondPlayer
            };

            tabletop.WithDefaultLibrary();

            tabletop
                .Battlefield
                .AddDefaultCreaturePermanent("[_MOCK_CREATURE_01_]", secondPlayer, secondPlayer, false)
                .AddDefaultCreaturePermanent("[_MOCK_CREATURE_02_]", secondPlayer, secondPlayer, true)
                .AddDefaultCreaturePermanent("[_MOCK_CREATURE_11_]", firstPlayer, secondPlayer, false)
                .AddDefaultCreaturePermanent("[_MOCK_CREATURE_12_]", firstPlayer, secondPlayer, true);

            var judge = new Judge(mockLogger.Object);

            // Act.

            var executionResult = judge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .HasError
                .Should().BeFalse("because execution should complete without error");

            using var _ = new AssertionScope();

            tabletop
                .Battlefield
                .FindAll()
                .Select(permanent => permanent.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .Permanent.IsTapped
                        .Should().BeFalse($"because creature [{creature.Permanent.Name}] should be untapped");
                });
        }

        [Fact]
        public void WhenEnteringUntapStep_ShouldKeepStatusForCreatureControlledByNonActivePlayer()
        {
            // Arrange.

            var mockLogger = MockBuilder.CreateMock<ILogger>();

            var firstPlayer = StubBuilder.CreateDefaultPlayer("[_MOCK_PLAYER__ALPHA_]");
            var secondPlayer = StubBuilder.CreateDefaultPlayer("[_MOCK_PLAYER__OMEGA_]");

            var tabletop = new Tabletop
            {
                TurnId = 0,
                Phase = Phase.Ending,
                ActivePlayer = firstPlayer,
                NonActivePlayer = secondPlayer
            };

            tabletop.WithDefaultLibrary();

            tabletop
                .Battlefield
                .AddDefaultCreaturePermanent("[_MOCK_CREATURE_01_]", firstPlayer, firstPlayer, false)
                .AddDefaultCreaturePermanent("[_MOCK_CREATURE_02_]", firstPlayer, firstPlayer, true)
                .AddDefaultCreaturePermanent("[_MOCK_CREATURE_11_]", secondPlayer, firstPlayer, false)
                .AddDefaultCreaturePermanent("[_MOCK_CREATURE_12_]", secondPlayer, firstPlayer, true);

            var judge = new Judge(mockLogger.Object);

            // Act.

            var executionResult = judge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .HasError
                .Should().BeFalse("because execution should complete without error");

            using var _ = new AssertionScope();

            tabletop
                .Battlefield
                .FindAll()
                .Where(permanent => permanent.Name.Contains("01") || permanent.Name.Contains("11"))
                .Select(permanent => permanent.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .Permanent.IsTapped
                        .Should().BeFalse($"because creature [{creature.Permanent.Name}] should be keep status");
                });

            tabletop
                .Battlefield
                .FindAll()
                .Where(permanent => permanent.Name.Contains("02") || permanent.Name.Contains("12"))
                .Select(permanent => permanent.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .Permanent.IsTapped
                        .Should().BeTrue($"because creature [{creature.Permanent.Name}] should be keep status");
                });
        }

        [Fact]
        public void WhenEnteringDrawStepOnSecondTurn_ShouldDrawOneCardForActivePlayer()
        {
            // Arrange.

            var mockLogger = MockBuilder.CreateMock<ILogger>();

            var firstPlayer = StubBuilder.CreateDefaultPlayer("[_MOCK_PLAYER__ALPHA_]");
            var secondPlayer = StubBuilder.CreateDefaultPlayer("[_MOCK_PLAYER__OMEGA_]");

            var tabletop = new Tabletop
            {
                TurnId = 0,
                Phase = Phase.Ending,
                ActivePlayer = firstPlayer,
                NonActivePlayer = secondPlayer
            };

            firstPlayer.Library.AddStubCard("ALPHA", 0, 5);
            firstPlayer.Hand.AddStubCard("ALPHA", 10, 1);

            secondPlayer.Library.AddStubCard("OMEGA", 0, 10);
            secondPlayer.Hand.AddStubCard("OMEGA", 10, 3);

            var judge = new Judge(mockLogger.Object);

            // Act.

            var executionResult = judge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .HasError
                .Should().BeFalse("because execution should complete without error");

            tabletop
                .ActivePlayer.Library
                .Must().HaveQuantity(9)
                .And.NotContain("[_MOCK_STUB__OMEGA_00_]");

            tabletop
                .ActivePlayer.Hand
                .Must().HaveQuantity(4)
                .And.Contain(
                    "[_MOCK_STUB__OMEGA_10_]", "[_MOCK_STUB__OMEGA_11_]", "[_MOCK_STUB__OMEGA_12_]",
                    "[_MOCK_STUB__OMEGA_00_]");

            tabletop
                .NonActivePlayer.Library
                .Must().HaveQuantity(5)
                .And.ContainMatching("\\[_MOCK_STUB__ALPHA_0[0-4]_\\]");

            tabletop
                .NonActivePlayer.Hand
                .Must().HaveQuantity(1)
                .And.Contain("[_MOCK_STUB__ALPHA_10_]");
        }

        [Fact]
        public void WhenEnteringDrawStepOnFirstTurn_ShouldNotDrawCardForActivePlayer()
        {
            // Arrange.

            var mockLogger = MockBuilder.CreateMock<ILogger>();

            var firstPlayer = StubBuilder.CreateDefaultPlayer("[_MOCK_PLAYER__ALPHA_]");
            var secondPlayer = StubBuilder.CreateDefaultPlayer("[_MOCK_PLAYER__OMEGA_]");

            var tabletop = new Tabletop
            {
                Phase = Phase.Setup,
                ActivePlayer = firstPlayer,
                NonActivePlayer = secondPlayer
            };

            firstPlayer.Library.AddStubCard("ALPHA", 0, 5);
            firstPlayer.Hand.AddStubCard("ALPHA", 10, 1);

            secondPlayer.Library.AddStubCard("OMEGA", 0, 10);
            secondPlayer.Hand.AddStubCard("OMEGA", 10, 3);

            var judge = new Judge(mockLogger.Object);

            // Act.

            var executionResult = judge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .HasError
                .Should().BeFalse("because execution should complete without error");

            tabletop
                .ActivePlayer.Library
                .Must().HaveQuantity(5)
                .And.ContainMatching("\\[_MOCK_STUB__ALPHA_0[0-4]_\\]");

            tabletop
                .ActivePlayer.Hand
                .Must().HaveQuantity(1)
                .And.Contain("[_MOCK_STUB__ALPHA_10_]");

            tabletop
                .NonActivePlayer.Library
                .Must().HaveQuantity(10)
                .And.ContainMatching("\\[_MOCK_STUB__OMEGA_0[0-9]_\\]");

            tabletop
                .NonActivePlayer.Hand
                .Must().HaveQuantity(3)
                .And.ContainMatching("\\[_MOCK_STUB__OMEGA_1[0-2]_\\]");
        }
    }

    public class ExecuteNextPhase_Main
    {
        [Fact]
        public void WhenPlayingSecondLand_ShouldFailValidation()
        {
            // Arrange.

            var mockLogger = MockBuilder.CreateMock<ILogger>();

            var mockFirstStrategy = MockBuilder
                .CreateMock<IStrategy>()
                .WithPerformingPrioritizedAction(tabletop => !tabletop.ActivePlayer.Hand.IsEmpty
                    ? Action.PlayLand(tabletop.ActivePlayer.Hand.FindFromTop())
                    : Action.Pass())
                .WithPerformingNonPrioritizedAction(_ => Action.Pass());

            var mockSecondStrategy = MockBuilder
                .CreateMock<IStrategy>()
                .WithPerformingDefaultAction();

            var firstPlayer = StubBuilder.CreateDefaultPlayer("[_MOCK_PLAYER__ALPHA_]", mockFirstStrategy.Object);
            var secondPlayer = StubBuilder.CreateDefaultPlayer("[_MOCK_PLAYER__OMEGA_]", mockSecondStrategy.Object);

            var tabletop = new Tabletop
            {
                Phase = Phase.Beginning,
                ActivePlayer = firstPlayer,
                NonActivePlayer = secondPlayer
            };

            firstPlayer.Hand.AddLandCard("ALPHA", 0, 3);

            var judge = new Judge(mockLogger.Object);

            // Act.

            var executionResult = judge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .HasError
                .Should().BeTrue("because execution should complete with error");

            executionResult
                .Messages
                .Should().HaveCount(1)
                .And.Contain(
                    "Active player had played a land this turn! " +
                    "Cause: [Action_PlayingLand]. " +
                    "Reference: [[_MOCK_LAND__ALPHA_01_]].");

            tabletop
                .Battlefield
                .Must().HaveQuantity(1)
                .And.Contain("[_MOCK_LAND__ALPHA_00_]");

            tabletop
                .ActivePlayer.Hand
                .Must().HaveQuantity(2)
                .And.ContainMatching("\\[_MOCK_LAND__ALPHA_0[1-2]_\\]");
        }
    }

    public class ExecuteNextPhaseMethod_Combat
    {
        [Fact]
        public void WhenEnteringDeclareAttackerStep_ShouldTapAttackingCreature()
        {
            // Arrange.

            var mockLogger = MockBuilder.CreateMock<ILogger>();
            var mockAttackingStrategy = MockBuilder.CreateMock<IStrategy>();
            var mockBlockingStrategy = MockBuilder.CreateMock<IStrategy>();

            var tabletop = StubBuilder.CreateDefaultTabletop(
                mockAttackingStrategy.Object,
                mockBlockingStrategy.Object);

            tabletop.Phase = Phase.PrecombatMain;

            var attackingPermanents = new[]
            {
                tabletop.CreateActiveCreaturePermanent("[_MOCK_ATTACKER_01_]", 1, 1),
                tabletop.CreateActiveCreaturePermanent("[_MOCK_ATTACKER_02_]", 2, 2),
                tabletop.CreateActiveCreaturePermanent("[_MOCK_ATTACKER_03_]", 3, 3)
            };

            var blockingPermanent = tabletop.CreateNonActiveCreaturePermanent("[_MOCK_BLOCKER_]", 0, 5);

            mockAttackingStrategy.WithAttackingDecision(attackingPermanents);
            mockBlockingStrategy.WithBlockingDecision(attackingPermanents.First(), new[] { blockingPermanent });

            Enumerable
                .Empty<IPermanent>()
                .Append(attackingPermanents)
                .Append(blockingPermanent)
                .ForEach(tabletop.Battlefield.AddToTop);

            var judge = new Judge(mockLogger.Object);

            // Act.

            var executionResult = judge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .HasError
                .Should().BeFalse("because execution should complete without error");

            using var _ = new AssertionScope();

            tabletop
                .Battlefield
                .FindAll()
                .Where(permanent => permanent.Name.Contains("MOCK_ATTACKER"))
                .Select(permanent => permanent.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .Permanent.IsTapped
                        .Should().BeTrue($"because attacking creature [{creature.Permanent.Name}] should be tapped");
                });
        }

        [Fact]
        public void WhenEnteringDeclareAttackerStep_ShouldKeepStatusForNotAttackingCreature()
        {
            // Arrange.

            var mockLogger = MockBuilder.CreateMock<ILogger>();

            var tabletop = StubBuilder.CreateDefaultTabletop();
            tabletop.Phase = Phase.PrecombatMain;

            for (var value = 1; value <= 3; value++)
            {
                var tappedPermanent = tabletop.CreateActiveCreaturePermanent(
                    $"[_MOCK_TAPPED_{value}_]",
                    value,
                    value);

                tappedPermanent.IsTapped = true;
                tabletop.Battlefield.AddToTop(tappedPermanent);

                var untappedPermanent = tabletop.CreateActiveCreaturePermanent(
                    $"[_MOCK_UNTAPPED_{value}_]",
                    value,
                    value);

                untappedPermanent.IsTapped = false;
                tabletop.Battlefield.AddToTop(untappedPermanent);
            }

            var judge = new Judge(mockLogger.Object);

            // Act.

            var executionResult = judge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .HasError
                .Should().BeFalse("because execution should complete without error");

            using var _ = new AssertionScope();

            tabletop
                .Battlefield
                .FindAll()
                .Where(permanent => permanent.Name.Contains("MOCK_TAPPED"))
                .Select(permanent => permanent.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .Permanent.IsTapped
                        .Should().BeTrue($"because tapped creature [{creature.Permanent.Name}] should keep status");
                });

            tabletop
                .Battlefield
                .FindAll()
                .Where(permanent => permanent.Name.Contains("MOCK_UNTAPPED"))
                .Select(permanent => permanent.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .Permanent.IsTapped
                        .Should().BeFalse($"because untapped creature [{creature.Permanent.Name}] should keep status");
                });
        }

        [Fact]
        public void WhenEnteringDeclareBlockerStep_ShouldNotTapBlockingCreature()
        {
            // Arrange.

            var mockLogger = MockBuilder.CreateMock<ILogger>();
            var mockAttackingStrategy = MockBuilder.CreateMock<IStrategy>();
            var mockBlockingStrategy = MockBuilder.CreateMock<IStrategy>();

            var tabletop = StubBuilder.CreateDefaultTabletop(
                mockAttackingStrategy.Object,
                mockBlockingStrategy.Object);

            tabletop.Phase = Phase.PrecombatMain;

            var attackingPermanent = tabletop.CreateActiveCreaturePermanent("[_MOCK_ATTACKER_]", 1, 1);

            var blockingPermanents = new[]
            {
                tabletop.CreateNonActiveCreaturePermanent("[_MOCK_BLOCKER_01_]", 0, 11),
                tabletop.CreateNonActiveCreaturePermanent("[_MOCK_BLOCKER_02_]", 0, 12),
                tabletop.CreateNonActiveCreaturePermanent("[_MOCK_BLOCKER_02_]", 0, 13)
            };

            mockAttackingStrategy.WithAttackingDecision(attackingPermanent);
            mockBlockingStrategy.WithBlockingDecision(attackingPermanent, blockingPermanents);

            Enumerable
                .Empty<IPermanent>()
                .Append(attackingPermanent)
                .Append(blockingPermanents)
                .ForEach(tabletop.Battlefield.AddToTop);

            var judge = new Judge(mockLogger.Object);

            // Act.

            var executionResult = judge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .HasError
                .Should().BeFalse("because execution should complete without error");

            using var _ = new AssertionScope();

            tabletop
                .Battlefield
                .FindAll()
                .Where(permanent => permanent.Name.Contains("MOCK_BLOCKER"))
                .Select(permanent => permanent.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .Permanent.IsTapped
                        .Should().BeFalse($"because blocking creature [{creature.Permanent.Name}] should not be tapped");
                });
        }

        [Fact]
        public void WhenEnteringDeclareBlockerStep_ShouldKeepStatusForNotBlockingCreature()
        {
            // Arrange.

            var mockLogger = MockBuilder.CreateMock<ILogger>();

            var tabletop = StubBuilder.CreateDefaultTabletop(Strategy.Noop, Strategy.Noop);
            tabletop.Phase = Phase.PrecombatMain;

            for (var value = 1; value <= 3; value++)
            {
                var tappedPermanent = tabletop.CreateNonActiveCreaturePermanent(
                    $"[_MOCK_TAPPED_{value}_]",
                    value,
                    value);

                tappedPermanent.IsTapped = true;
                tabletop.Battlefield.AddToTop(tappedPermanent);

                var untappedPermanent = tabletop.CreateNonActiveCreaturePermanent(
                    $"[_MOCK_UNTAPPED_{value}_]",
                    value,
                    value);

                untappedPermanent.IsTapped = false;
                tabletop.Battlefield.AddToTop(untappedPermanent);
            }

            var judge = new Judge(mockLogger.Object);

            // Act.

            var executionResult = judge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .HasError
                .Should().BeFalse("because execution should complete without error");

            using var _ = new AssertionScope();

            tabletop
                .Battlefield
                .FindAll()
                .Where(permanent => permanent.Name.Contains("MOCK_TAPPED"))
                .Select(permanent => permanent.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .Permanent.IsTapped
                        .Should().BeTrue($"because tapped creature [{creature.Permanent.Name}] should keep status");
                });

            tabletop
                .Battlefield
                .FindAll()
                .Where(permanent => permanent.Name.Contains("MOCK_UNTAPPED"))
                .Select(permanent => permanent.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .Permanent.IsTapped
                        .Should().BeFalse($"because untapped creature [{creature.Permanent.Name}] should keep status");
                });
        }
    }
}