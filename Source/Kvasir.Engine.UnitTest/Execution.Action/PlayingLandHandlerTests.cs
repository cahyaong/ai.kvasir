// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayingLandHandlerTests.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, September 17, 2022 6:51:55 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine.UnitTest;

using FluentAssertions;
using FluentAssertions.Execution;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Framework;
using Xunit;

public class PlayingLandHandlerTests
{
    public class ValidateMethod
    {
        [Fact]
        public void WhenPlayingFirstLandAsActivePlayer_ShouldReturnSuccessful()
        {
            // Arrange.

            var playingLandHandler = new PlayingLandHandler();
            var tabletop = StubBuilder.CreateDefaultTabletop();

            tabletop
                .ActivePlayer.Hand
                .AddLandCard("ACTIVE", 0, 1);

            var playingLandAction = Action.PlayCard(tabletop.ActivePlayer.Hand.FindFromTop());
            playingLandAction.Target.Player = tabletop.ActivePlayer;

            // Act.

            var validationResult = playingLandHandler.Validate(tabletop, playingLandAction);

            // Assert.

            validationResult
                .Should().NotBeNull()
                .And.BeEquivalentTo(ValidationResult.Successful);
        }

        [Fact]
        public void WhenPlayingSecondLandAsActivePlayer_ShouldReturnFailure()
        {
            // Arrange.

            var playingLandHandler = new PlayingLandHandler();
            var tabletop = StubBuilder.CreateDefaultTabletop();

            tabletop
                .ActivePlayer.Hand
                .AddLandCard("ACTIVE", 0, 3);

            var playingFirstLandAction = Action.PlayCard(tabletop.ActivePlayer.Hand.FindFromTop());
            playingFirstLandAction.Target.Player = tabletop.ActivePlayer;
            playingLandHandler.Resolve(tabletop, playingFirstLandAction);

            var playingSecondLandAction = Action.PlayCard(tabletop.ActivePlayer.Hand.FindFromTop());
            playingSecondLandAction.Target.Player = tabletop.ActivePlayer;

            // Act.

            var validationResult = playingLandHandler.Validate(tabletop, playingSecondLandAction);

            // Assert.

            validationResult
                .Should().NotBeNull();

            validationResult
                .HasError
                .Should().BeTrue();

            validationResult
                .Messages
                .Should().ContainSingle("Active player had played a land this turn!");
        }

        [Fact]
        public void WhenPlayingLandAsNonActivePlayer_ShouldReturnFailure()
        {
            // Arrange.

            var playingLandHandler = new PlayingLandHandler();
            var tabletop = StubBuilder.CreateDefaultTabletop();

            tabletop
                .ActivePlayer.Hand
                .AddLandCard("ACTIVE", 0, 1);

            tabletop
                .ActivePlayer.Hand
                .AddLandCard("NONACTIVE", 0, 1);

            var playingLandAction = Action.PlayCard(tabletop.ActivePlayer.Hand.FindFromTop());
            playingLandAction.Target.Player = tabletop.NonActivePlayer;

            // Act.

            var validationResult = playingLandHandler.Validate(tabletop, playingLandAction);

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
                    .Should().ContainSingle("Non-active player is playing a land!");
            }
        }

        [Fact]
        public void WhenPlayingLandAsActivePlayerWithNonEmptyStack_ShouldReturnFailure()
        {
            // Arrange.

            var playingLandHandler = new PlayingLandHandler();
            var tabletop = StubBuilder.CreateDefaultTabletop();

            tabletop
                .ActivePlayer.Hand
                .AddLandCard("ACTIVE", 0, 3);

            tabletop.Stack.AddToTop(StubBuilder.CreateStubAction(
                "[_MOCK_STUB_11_]",
                tabletop.ActivePlayer));

            tabletop.Stack.AddToTop(StubBuilder.CreateStubAction(
                "[_MOCK_STUB_21_]",
                tabletop.NonActivePlayer));

            var playingLandAction = Action.PlayCard(tabletop.ActivePlayer.Hand.FindFromTop());
            playingLandAction.Target.Player = tabletop.ActivePlayer;

            // Act.

            var validationResult = playingLandHandler.Validate(tabletop, playingLandAction);

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
                    .Should().ContainSingle("Stack is not empty when playing a land!");
            }
        }
    }
}