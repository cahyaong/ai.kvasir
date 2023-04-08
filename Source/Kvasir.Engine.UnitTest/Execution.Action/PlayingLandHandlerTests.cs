// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayingLandHandlerTests.cs" company="nGratis">
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
// <creation_timestamp>Saturday, September 17, 2022 6:51:55 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine.UnitTest;

using FluentAssertions;
using FluentAssertions.Execution;
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

            var playingLandAction = Action.PlayLand(tabletop.ActivePlayer.Hand.FindFromTop());
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

            var playingFirstLandAction = Action.PlayLand(tabletop.ActivePlayer.Hand.FindFromTop());
            playingFirstLandAction.Target.Player = tabletop.ActivePlayer;
            playingLandHandler.Resolve(tabletop, playingFirstLandAction);

            var playingSecondLandAction = Action.PlayLand(tabletop.ActivePlayer.Hand.FindFromTop());
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

            var playingLandAction = Action.PlayLand(tabletop.ActivePlayer.Hand.FindFromTop());
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

            var playingLandAction = Action.PlayLand(tabletop.ActivePlayer.Hand.FindFromTop());
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