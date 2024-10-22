// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RoundJudgeTests.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Sunday, May 15, 2022 7:00:13 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.IntegrationTest;

using System.Collections.Generic;
using System.Linq;
using Autofac;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Engine;
using nGratis.AI.Kvasir.Framework;
using Xunit;

public class RoundJudgeTests
{
    public class ExecuteNextTurn
    {
        [Fact]
        public void WhenGettingNonTerminalCondition_ShouldCompleteUntilEndingPhase()
        {
            // Arrange.

            var container = new ContainerBuilder()
                .RegisterTestingInfrastructure()
                .RegisterJudge()
                .Build();

            var tabletop = StubBuilder.CreateDefaultTabletop(Strategy.Noop, Strategy.Noop);
            tabletop.Phase = Phase.Beginning;
            tabletop.TurnId = 42;

            var roundJudge = container.Resolve<IRoundJudge>();

            // Act.

            var executionResult = roundJudge.ExecuteNextTurn(tabletop);

            // Assert.

            executionResult
                .Must().CompleteWithoutReachingTerminalCondition();

            tabletop.Phase
                .Should().Be(Phase.Ending, "because execution should complete without error or terminal condition");

            tabletop.TurnId
                .Should().Be(42, "because turn should reach ending phase");
        }

        [Fact]
        public void WhenGettingTerminalCondition_ShouldCompleteUntilTerminatingPhase()
        {
            // Arrange.

            var container = new ContainerBuilder()
                .RegisterTestingInfrastructure()
                .RegisterJudge()
                .Build();

            var judicialAssistant = container.Resolve<IJudicialAssistant>();
            var attackingStrategy = new AllAttackingStrategy(judicialAssistant);

            var tabletop = StubBuilder.CreateDefaultTabletop(attackingStrategy, Strategy.Noop);
            tabletop.Phase = Phase.Beginning;
            tabletop.TurnId = 42;
            tabletop.NonActivePlayer.Life = 1;

            var attackingCreature = tabletop
                .CreateActiveCreaturePermanent("[_MOCK_ATTACKER_]", 1, 1)
                .WithoutSummoningSickness();

            tabletop.Battlefield.AddToTop(attackingCreature);

            var roundJudge = container.Resolve<IRoundJudge>();

            // Act.

            var executionResult = roundJudge.ExecuteNextTurn(tabletop);

            // Assert.

            executionResult
                .Must().CompleteWithWinningPlayer(tabletop.ActivePlayer);

            tabletop.Phase
                .Should().Be(Phase.Combat, "because execution should complete with terminal condition");

            tabletop.TurnId
                .Should().Be(42, "because turn should reach terminating phase");
        }
    }

    public class ExecuteNextPhaseMethod_Beginning
    {
        [Fact]
        public void WhenEnteringBeginningPhase_ShouldSwitchActivePlayer()
        {
            // Arrange.

            var container = new ContainerBuilder()
                .RegisterTestingInfrastructure()
                .RegisterJudge()
                .Build();

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

            var roundJudge = container.Resolve<IRoundJudge>();

            // Act.

            var executionResult = roundJudge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .Must().CompleteWithoutReachingTerminalCondition();

            tabletop
                .ActivePlayer
                .Should().Be(secondPlayer, "because second player should be active");

            tabletop
                .NonActivePlayer
                .Should().Be(firstPlayer, "because first player should be nonactive");
        }

        [Fact]
        public void WhenEnteringBeginningPhase_ShouldNotLetActivePlayerDrawCard_IfFirstTurn()
        {
            // Arrange.

            var container = new ContainerBuilder()
                .RegisterTestingInfrastructure()
                .RegisterJudge()
                .Build();

            var firstPlayer = StubBuilder.CreateDefaultPlayer("[_MOCK_PLAYER__ALPHA_]");
            var secondPlayer = StubBuilder.CreateDefaultPlayer("[_MOCK_PLAYER__OMEGA_]");

            var tabletop = new Tabletop
            {
                TurnId = -1,
                Phase = Phase.Setup,
                ActivePlayer = firstPlayer,
                NonActivePlayer = secondPlayer
            };

            firstPlayer.Library.AddStubCard("ALPHA", 0, 5);
            secondPlayer.Library.AddStubCard("OMEGA", 0, 10);

            var roundJudge = container.Resolve<IRoundJudge>();

            // Act.

            var executionResult = roundJudge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .Must().CompleteWithoutReachingTerminalCondition();

            tabletop
                .ActivePlayer.Hand
                .Must().HaveQuantity(0);

            tabletop
                .ActivePlayer.Library
                .Must().HaveQuantity(5)
                .And.ContainMatching(@"\[_MOCK_STUB__ALPHA_0[0-4]_\]");

            tabletop
                .NonActivePlayer.Hand
                .Must().HaveQuantity(0);

            tabletop
                .NonActivePlayer.Library
                .Must().HaveQuantity(10)
                .And.ContainMatching(@"\[_MOCK_STUB__OMEGA_0[0-9]_\]");
        }

        [Fact]
        public void WhenEnteringBeginningPhase_ShouldLetActivePlayerDrawCard_IfNotFirstTurn()
        {
            // Arrange.

            var container = new ContainerBuilder()
                .RegisterTestingInfrastructure()
                .RegisterJudge()
                .Build();

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
            secondPlayer.Library.AddStubCard("OMEGA", 0, 10);

            var roundJudge = container.Resolve<IRoundJudge>();

            // Act.

            var executionResult = roundJudge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .Must().CompleteWithoutReachingTerminalCondition();

            tabletop
                .ActivePlayer.Hand
                .Must().HaveQuantity(1)
                .And.Contain("[_MOCK_STUB__OMEGA_00_]");

            tabletop
                .ActivePlayer.Library
                .Must().HaveQuantity(9)
                .And.NotContain("[_MOCK_STUB__OMEGA_00_]");

            tabletop
                .NonActivePlayer.Hand
                .Must().HaveQuantity(0);

            tabletop
                .NonActivePlayer.Library
                .Must().HaveQuantity(5);
        }

        [Fact]
        public void WhenEnteringBeginningPhase_ShouldReachTerminalCondition_IfActivePlayerDrawCardFromEmptyLibrary()
        {
            // Arrange.

            var container = new ContainerBuilder()
                .RegisterTestingInfrastructure()
                .RegisterJudge()
                .Build();

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
            secondPlayer.Library.RemoveAll();

            var roundJudge = container.Resolve<IRoundJudge>();

            // Act.

            var executionResult = roundJudge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .Must().CompleteWithWinningPlayer(firstPlayer);
        }

        [Fact]
        public void WhenEnteringUntapStep_ShouldRemoveSummoningSicknessFromCreatureControlledByActivePlayer()
        {
            // Arrange.

            var container = new ContainerBuilder()
                .RegisterTestingInfrastructure()
                .RegisterJudge()
                .Build();

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
                .AddCreaturePermanent(
                    "[_MOCK_CREATURE_01_]",
                    secondPlayer,
                    secondPlayer,
                    new() { HasSummoningSickness = false })
                .AddCreaturePermanent(
                    "[_MOCK_CREATURE_02_]",
                    secondPlayer,
                    secondPlayer,
                    new() { HasSummoningSickness = true })
                .AddCreaturePermanent(
                    "[_MOCK_CREATURE_11_]",
                    firstPlayer,
                    secondPlayer,
                    new() { HasSummoningSickness = false })
                .AddCreaturePermanent(
                    "[_MOCK_CREATURE_12_]",
                    firstPlayer,
                    secondPlayer,
                    new() { HasSummoningSickness = true });

            var roundJudge = container.Resolve<IRoundJudge>();

            // Act.

            var executionResult = roundJudge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .Must().CompleteWithoutReachingTerminalCondition();

            using var _ = new AssertionScope();

            tabletop
                .Battlefield
                .FindAll()
                .Select(permanent => permanent.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .HasSummoningSickness
                        .Should().BeFalse($"because creature [{creature.Name}] should not have summoning sickness");
                });
        }

        [Fact]
        public void WhenEnteringUntapStep_ShouldKeepSummoningSicknessFromCreatureControlledByNonActivePlayer()
        {
            // Arrange.

            var container = new ContainerBuilder()
                .RegisterTestingInfrastructure()
                .RegisterJudge()
                .Build();

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
                .AddCreaturePermanent(
                    "[_MOCK_CREATURE_01_]",
                    firstPlayer,
                    firstPlayer,
                    new() { HasSummoningSickness = false })
                .AddCreaturePermanent(
                    "[_MOCK_CREATURE_02_]",
                    firstPlayer,
                    firstPlayer,
                    new() { HasSummoningSickness = true })
                .AddCreaturePermanent(
                    "[_MOCK_CREATURE_11_]",
                    secondPlayer,
                    firstPlayer,
                    new() { HasSummoningSickness = false })
                .AddCreaturePermanent(
                    "[_MOCK_CREATURE_12_]",
                    secondPlayer,
                    firstPlayer,
                    new() { HasSummoningSickness = true });

            var roundJudge = container.Resolve<IRoundJudge>();

            // Act.

            var executionResult = roundJudge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .Must().CompleteWithoutReachingTerminalCondition();

            using var _ = new AssertionScope();

            tabletop
                .Battlefield
                .FindAll()
                .Where(permanent => permanent.Name.Contains("01") || permanent.Name.Contains("11"))
                .Select(permanent => permanent.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .HasSummoningSickness
                        .Should().BeFalse($"because creature [{creature.Name}] should keep summoning sickness");
                });

            tabletop
                .Battlefield
                .FindAll()
                .Where(permanent => permanent.Name.Contains("02") || permanent.Name.Contains("12"))
                .Select(permanent => permanent.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .HasSummoningSickness
                        .Should().BeTrue($"because creature [{creature.Name}] should keep summoning sickness");
                });
        }

        [Fact]
        public void WhenEnteringUntapStep_ShouldUntapPermanentControlledByActivePlayer()
        {
            // Arrange.

            var container = new ContainerBuilder()
                .RegisterTestingInfrastructure()
                .RegisterJudge()
                .Build();

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
                .AddStubPermanent(
                    "[_MOCK_STUB_01_]",
                    secondPlayer,
                    secondPlayer,
                    new() { IsTapped = false })
                .AddStubPermanent(
                    "[_MOCK_STUB_02_]",
                    secondPlayer,
                    secondPlayer,
                    new() { IsTapped = true })
                .AddStubPermanent(
                    "[_MOCK_STUB_11_]",
                    firstPlayer,
                    secondPlayer,
                    new() { IsTapped = false })
                .AddStubPermanent(
                    "[_MOCK_STUB_12_]",
                    firstPlayer,
                    secondPlayer,
                    new() { IsTapped = true });

            var roundJudge = container.Resolve<IRoundJudge>();

            // Act.

            var executionResult = roundJudge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .Must().CompleteWithoutReachingTerminalCondition();

            using var _ = new AssertionScope();

            tabletop
                .Battlefield
                .FindAll()
                .ForEach(permanent =>
                {
                    permanent
                        .IsTapped
                        .Should().BeFalse($"because permanent [{permanent.Name}] should be untapped");
                });
        }

        [Fact]
        public void WhenEnteringUntapStep_ShouldKeepStatusForPermanentControlledByNonActivePlayer()
        {
            // Arrange.

            var container = new ContainerBuilder()
                .RegisterTestingInfrastructure()
                .RegisterJudge()
                .Build();

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
                .AddStubPermanent(
                    "[_MOCK_STUB_01_]",
                    firstPlayer,
                    firstPlayer,
                    new() { IsTapped = false })
                .AddStubPermanent(
                    "[_MOCK_STUB_02_]",
                    firstPlayer,
                    firstPlayer,
                    new() { IsTapped = true })
                .AddStubPermanent(
                    "[_MOCK_STUB_11_]",
                    secondPlayer,
                    firstPlayer,
                    new() { IsTapped = false })
                .AddStubPermanent(
                    "[_MOCK_STUB_12_]",
                    secondPlayer,
                    firstPlayer,
                    new() { IsTapped = true });

            var roundJudge = container.Resolve<IRoundJudge>();

            // Act.

            var executionResult = roundJudge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .Must().CompleteWithoutReachingTerminalCondition();

            using var _ = new AssertionScope();

            tabletop
                .Battlefield
                .FindAll()
                .Where(permanent => permanent.Name.Contains("01") || permanent.Name.Contains("11"))
                .ForEach(permanent =>
                {
                    permanent
                        .IsTapped
                        .Should().BeFalse($"because permanent [{permanent.Name}] should keep status");
                });

            tabletop
                .Battlefield
                .FindAll()
                .Where(permanent => permanent.Name.Contains("02") || permanent.Name.Contains("12"))
                .ForEach(permanent =>
                {
                    permanent
                        .IsTapped
                        .Should().BeTrue($"because permanent [{permanent.Name}] should keep status");
                });
        }

        [Fact]
        public void WhenEnteringDrawStepOnSecondTurn_ShouldDrawOneCardForActivePlayer()
        {
            // Arrange.

            var container = new ContainerBuilder()
                .RegisterTestingInfrastructure()
                .RegisterJudge()
                .Build();

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

            var roundJudge = container.Resolve<IRoundJudge>();

            // Act.

            var executionResult = roundJudge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .Must().CompleteWithoutReachingTerminalCondition();

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
                .And.ContainMatching(@"\[_MOCK_STUB__ALPHA_0[0-4]_\]");

            tabletop
                .NonActivePlayer.Hand
                .Must().HaveQuantity(1)
                .And.Contain("[_MOCK_STUB__ALPHA_10_]");
        }

        [Fact]
        public void WhenEnteringDrawStepOnFirstTurn_ShouldNotDrawCardForActivePlayer()
        {
            // Arrange.

            var container = new ContainerBuilder()
                .RegisterTestingInfrastructure()
                .RegisterJudge()
                .Build();

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

            var roundJudge = container.Resolve<IRoundJudge>();

            // Act.

            var executionResult = roundJudge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .Must().CompleteWithoutReachingTerminalCondition();

            tabletop
                .ActivePlayer.Library
                .Must().HaveQuantity(5)
                .And.ContainMatching(@"\[_MOCK_STUB__ALPHA_0[0-4]_\]");

            tabletop
                .ActivePlayer.Hand
                .Must().HaveQuantity(1)
                .And.Contain("[_MOCK_STUB__ALPHA_10_]");

            tabletop
                .NonActivePlayer.Library
                .Must().HaveQuantity(10)
                .And.ContainMatching(@"\[_MOCK_STUB__OMEGA_0[0-9]_\]");

            tabletop
                .NonActivePlayer.Hand
                .Must().HaveQuantity(3)
                .And.ContainMatching(@"\[_MOCK_STUB__OMEGA_1[0-2]_\]");
        }
    }

    public class ExecuteNextPhase_Main
    {
        [Fact]
        public void WhenPlayingSecondLand_ShouldFailValidation()
        {
            // Arrange.

            var container = new ContainerBuilder()
                .RegisterTestingInfrastructure()
                .RegisterJudge()
                .Build();

            var mockFirstStrategy = MockBuilder
                .CreateMock<IStrategy>()
                .WithPerformingPrioritizedAction(tabletop =>
                {
                    var action = !tabletop.ActivePlayer.Hand.IsEmpty
                        ? Action.PlayCard(tabletop.ActivePlayer.Hand.FindFromTop())
                        : Action.Pass();

                    action.Target.Player = tabletop.ActivePlayer;

                    return action;
                })
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

            var roundJudge = container.Resolve<IRoundJudge>();

            // Act.

            var executionResult = roundJudge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .Must().CompleteWithError();

            executionResult
                .Messages
                .Should().HaveCount(1)
                .And.Contain(
                    "Active player had played a land this turn! " +
                    "Cause: [Action_PlayingLand]. " +
                    "References: ([[_MOCK_LAND__ALPHA_01_]]).");

            tabletop
                .Battlefield
                .Must().HaveQuantity(1)
                .And.Contain("[_MOCK_LAND__ALPHA_00_]");

            tabletop
                .ActivePlayer.Hand
                .Must().HaveQuantity(2)
                .And.ContainMatching(@"\[_MOCK_LAND__ALPHA_0[1-2]_\]");
        }
    }

    public class ExecuteNextPhaseMethod_Combat
    {
        [Fact]
        public void WhenEnteringDeclareAttackerStep_ShouldTapAttackingCreature()
        {
            // Arrange.

            var container = new ContainerBuilder()
                .RegisterTestingInfrastructure()
                .RegisterJudge()
                .Build();

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
            mockBlockingStrategy.WithBlockingDecision(attackingPermanents.First(), [blockingPermanent]);

            Enumerable
                .Empty<IPermanent>()
                .Append(attackingPermanents)
                .Append(blockingPermanent)
                .ForEach(tabletop.Battlefield.AddToTop);

            var roundJudge = container.Resolve<IRoundJudge>();

            // Act.

            var executionResult = roundJudge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .Must().CompleteWithoutReachingTerminalCondition();

            using var _ = new AssertionScope();

            tabletop
                .Battlefield
                .FindAll()
                .Where(permanent => permanent.Name.Contains("MOCK_ATTACKER"))
                .Select(permanent => permanent.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .IsTapped
                        .Should().BeTrue($"because attacking creature [{creature.Name}] should be tapped");
                });
        }

        [Fact]
        public void WhenEnteringDeclareAttackerStep_ShouldKeepStatusForNotAttackingCreature()
        {
            // Arrange.

            var container = new ContainerBuilder()
                .RegisterTestingInfrastructure()
                .RegisterJudge()
                .Build();

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

            var roundJudge = container.Resolve<IRoundJudge>();

            // Act.

            var executionResult = roundJudge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .Must().CompleteWithoutReachingTerminalCondition();

            using var _ = new AssertionScope();

            tabletop
                .Battlefield
                .FindAll()
                .Where(permanent => permanent.Name.Contains("MOCK_TAPPED"))
                .Select(permanent => permanent.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .IsTapped
                        .Should().BeTrue($"because tapped creature [{creature.Name}] should keep status");
                });

            tabletop
                .Battlefield
                .FindAll()
                .Where(permanent => permanent.Name.Contains("MOCK_UNTAPPED"))
                .Select(permanent => permanent.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .IsTapped
                        .Should().BeFalse($"because untapped creature [{creature.Name}] should keep status");
                });
        }

        [Fact]
        public void WhenEnteringDeclareBlockerStep_ShouldNotTapBlockingCreature()
        {
            // Arrange.

            var container = new ContainerBuilder()
                .RegisterTestingInfrastructure()
                .RegisterJudge()
                .Build();

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

            var roundJudge = container.Resolve<IRoundJudge>();

            // Act.

            var executionResult = roundJudge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .Must().CompleteWithoutReachingTerminalCondition();

            using var _ = new AssertionScope();

            tabletop
                .Battlefield
                .FindAll()
                .Where(permanent => permanent.Name.Contains("MOCK_BLOCKER"))
                .Select(permanent => permanent.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .IsTapped
                        .Should().BeFalse($"because blocking creature [{creature.Name}] should not be tapped");
                });
        }

        [Fact]
        public void WhenEnteringDeclareBlockerStep_ShouldKeepStatusForNotBlockingCreature()
        {
            // Arrange.

            var container = new ContainerBuilder()
                .RegisterTestingInfrastructure()
                .RegisterJudge()
                .Build();

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

            var roundJudge = container.Resolve<IRoundJudge>();

            // Act.

            var executionResult = roundJudge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .Must().CompleteWithoutReachingTerminalCondition();

            using var _ = new AssertionScope();

            tabletop
                .Battlefield
                .FindAll()
                .Where(permanent => permanent.Name.Contains("MOCK_TAPPED"))
                .Select(permanent => permanent.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .IsTapped
                        .Should().BeTrue($"because tapped creature [{creature.Name}] should keep status");
                });

            tabletop
                .Battlefield
                .FindAll()
                .Where(permanent => permanent.Name.Contains("MOCK_UNTAPPED"))
                .Select(permanent => permanent.ToProxyCreature())
                .ForEach(creature =>
                {
                    creature
                        .IsTapped
                        .Should().BeFalse($"because untapped creature [{creature.Name}] should keep status");
                });
        }

        [Fact]
        public void WhenGivingLethalDamageToNonActivePlayer_ShouldTerminateWithWinningPlayer()
        {
            // Arrange.

            var container = new ContainerBuilder()
                .RegisterTestingInfrastructure()
                .RegisterJudge()
                .Build();

            var judicialAssistant = container.Resolve<IJudicialAssistant>();
            var attackingStrategy = new AllAttackingStrategy(judicialAssistant);

            var tabletop = StubBuilder.CreateDefaultTabletop(attackingStrategy, Strategy.Noop);
            tabletop.Phase = Phase.PrecombatMain;
            tabletop.NonActivePlayer.Life = 1;

            var attackingCreature = tabletop
                .CreateActiveCreaturePermanent("[_MOCK_ATTACKER_]", 1, 1)
                .WithoutSummoningSickness();

            tabletop.Battlefield.AddToTop(attackingCreature);

            var roundJudge = container.Resolve<IRoundJudge>();

            // Act.

            var executionResult = roundJudge.ExecuteNextPhase(tabletop);

            // Assert.

            executionResult
                .Must().CompleteWithWinningPlayer(tabletop.ActivePlayer);
        }
    }
}