// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiscardingHandlerTests.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, February 25, 2023 2:16:17 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine.UnitTest;

using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using nGratis.AI.Kvasir.Framework;
using Xunit;

public class DiscardingHandlerTests
{
    public class ValidateMethod
    {
        [Fact]
        public void WhenDiscardingCardsEqualToRequiredQuantity_ShouldReturnSuccessful()
        {
            // Arrange.

            var discardingHandler = new DiscardingHandler();
            var tabletop = StubBuilder.CreateDefaultTabletop();

            var discardingAction = Action.Discard(Enumerable
                .Range(0, 3)
                .Select(index => StubBuilder.CreateStubCard($"[_MOCK_STUB_{index:D2}_]"))
                .ToArray());

            discardingAction.Parameter = Parameter.Builder
                .Create()
                .WithValue(ParameterKey.Amount, 3)
                .Build();

            // Act.

            var validationResult = discardingHandler.Validate(tabletop, discardingAction);

            // Assert.

            validationResult
                .Should().NotBeNull();

            validationResult
                .HasError
                .Should().BeFalse();
        }

        [Fact]
        public void WhenDiscardingCardsLessThanRequiredQuantity_ShouldReturnFailure()
        {
            // Arrange.

            var discardingHandler = new DiscardingHandler();
            var tabletop = StubBuilder.CreateDefaultTabletop();

            var discardingAction = Action.Discard(Enumerable
                .Range(0, 3)
                .Select(index => StubBuilder.CreateStubCard($"[_MOCK_STUB_{index:D2}_]"))
                .ToArray());

            discardingAction.Parameter = Parameter.Builder
                .Create()
                .WithValue(ParameterKey.Amount, 5)
                .Build();

            // Act.

            var validationResult = discardingHandler.Validate(tabletop, discardingAction);

            // Assert.

            validationResult
                .Should().NotBeNull();

            using (new AssertionScope())
            {
                validationResult
                    .HasError
                    .Should().BeTrue();

                validationResult
                    .Messages
                    .Should().ContainSingle(
                        @"Discarding action expect an exact quantity, but found less cards to discard! " +
                        @"Actual Quantity: [3]. Expected Quantity: [5].");
            }
        }

        [Fact]
        public void WhenDiscardingCardsMoreThanRequiredQuantity_ShouldReturnFailure()
        {
            // Arrange.

            var discardingHandler = new DiscardingHandler();
            var tabletop = StubBuilder.CreateDefaultTabletop();

            var discardingAction = Action.Discard(Enumerable
                .Range(0, 3)
                .Select(index => StubBuilder.CreateStubCard($"[_MOCK_STUB_{index:D2}_]"))
                .ToArray());

            discardingAction.Parameter = Parameter.Builder
                .Create()
                .WithValue(ParameterKey.Amount, 1)
                .Build();

            // Act.

            var validationResult = discardingHandler.Validate(tabletop, discardingAction);

            // Assert.

            validationResult
                .Should().NotBeNull();

            using (new AssertionScope())
            {
                validationResult
                    .HasError
                    .Should().BeTrue();

                validationResult
                    .Messages
                    .Should().ContainSingle(
                        @"Discarding action expect an exact quantity, but found more cards to discard! " +
                        @"Actual Quantity: [3]. Expected Quantity: [1].");
            }
        }
    }

    public class ResolveMethod
    {
        [Fact]
        public void WhenDiscardingCardsEqualToRequiredQuantity_ShouldMoveThemToGraveyard()
        {
            // Arrange.

            var discardingHandler = new DiscardingHandler();

            var tabletop = StubBuilder.CreateDefaultTabletop();
            tabletop.Phase = Phase.PostcombatMain;

            tabletop
                .ActivePlayer.Hand
                .AddStubCard("ACTIVE", 0, 10);

            var discardingAction = Action.Discard(tabletop
                .ActivePlayer.Hand
                .FindManyFromTop(3)
                .ToArray());

            discardingAction.Owner = Player.None;
            discardingAction.Target.Player = tabletop.ActivePlayer;

            discardingAction.Parameter = Parameter.Builder
                .Create()
                .WithValue(ParameterKey.Amount, 3)
                .Build();

            // Act.

            discardingHandler.Resolve(tabletop, discardingAction);

            // Assert.

            using (new AssertionScope())
            {
                tabletop
                    .ActivePlayer.Hand
                    .Must().HaveQuantity(7)
                    .And.ContainMatching("\\[_MOCK_STUB__ACTIVE_0[3-9]_\\]");

                tabletop
                    .ActivePlayer.Graveyard
                    .Must().HaveQuantity(3)
                    .And.ContainMatching("\\[_MOCK_STUB__ACTIVE_0[0-2]_\\]");
            }
        }

        [Fact]
        public void WhenDiscardingCardsLessThanRequiredQuantity_ShouldPickAdditionalCardFromHand()
        {
            // Arrange.

            var discardingHandler = new DiscardingHandler();

            var tabletop = StubBuilder.CreateDefaultTabletop();
            tabletop.Phase = Phase.PostcombatMain;

            tabletop
                .ActivePlayer.Hand
                .AddStubCard("ACTIVE", 0, 10);

            var discardingAction = Action.Discard(tabletop
                .ActivePlayer.Hand
                .FindManyFromTop(1)
                .ToArray());

            discardingAction.Owner = Player.None;
            discardingAction.Target.Player = tabletop.ActivePlayer;

            discardingAction.Parameter = Parameter.Builder
                .Create()
                .WithValue(ParameterKey.Amount, 3)
                .Build();

            // Act.

            discardingHandler.Resolve(tabletop, discardingAction);

            // Assert.

            using (new AssertionScope())
            {
                tabletop
                    .ActivePlayer.Hand
                    .Must().HaveQuantity(7)
                    .And.ContainMatching("\\[_MOCK_STUB__ACTIVE_0[3-9]_\\]");

                tabletop
                    .ActivePlayer.Graveyard
                    .Must().HaveQuantity(3)
                    .And.ContainMatching("\\[_MOCK_STUB__ACTIVE_0[0-2]_\\]");
            }
        }

        [Fact]
        public void WhenDiscardingCardsMoreThanRequiredQuantity_ShouldPutAdditionalCardToHand()
        {
            // Arrange.

            var discardingHandler = new DiscardingHandler();

            var tabletop = StubBuilder.CreateDefaultTabletop();
            tabletop.Phase = Phase.PostcombatMain;

            tabletop
                .ActivePlayer.Hand
                .AddStubCard("ACTIVE", 0, 10);

            var discardingAction = Action.Discard(tabletop
                .ActivePlayer.Hand
                .FindManyFromTop(5)
                .ToArray());

            discardingAction.Owner = Player.None;
            discardingAction.Target.Player = tabletop.ActivePlayer;

            discardingAction.Parameter = Parameter.Builder
                .Create()
                .WithValue(ParameterKey.Amount, 3)
                .Build();

            // Act.

            discardingHandler.Resolve(tabletop, discardingAction);

            // Assert.

            using (new AssertionScope())
            {
                tabletop
                    .ActivePlayer.Hand
                    .Must().HaveQuantity(7)
                    .And.ContainMatching("\\[_MOCK_STUB__ACTIVE_0[3-9]_\\]");

                tabletop
                    .ActivePlayer.Graveyard
                    .Must().HaveQuantity(3)
                    .And.ContainMatching("\\[_MOCK_STUB__ACTIVE_0[0-2]_\\]");
            }
        }
    }
}