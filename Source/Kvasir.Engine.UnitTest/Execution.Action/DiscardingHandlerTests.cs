// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiscardingHandlerTests.cs" company="nGratis">
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