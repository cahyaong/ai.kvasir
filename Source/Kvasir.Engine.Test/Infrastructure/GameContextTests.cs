﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GameContextTests.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2020 Cahya Ong
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
// <creation_timestamp>Monday, 28 January 2019 4:07:09 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine.Test
{
    using System.Linq;
    using FluentAssertions;
    using FluentAssertions.Execution;
    using Moq.AI.Kvasir;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Engine;
    using Xunit;

    public class GameContextTests
    {
        public class Constructor
        {
            [Fact]
            public void WhenGettingValidParameter_ShouldSetCurrentPhaseToBeginning()
            {
                // Arrange.

                var definedAgents = new[]
                {
                    new DefinedBlob.Player
                    {
                        Name = "[_MOCK_PLAYER_01_]",
                        Deck = MockBuilder.CreateDefinedElfDeck()
                    },
                    new DefinedBlob.Player
                    {
                        Name = "[_MOCK_PLAYER_02_]",
                        Deck = MockBuilder.CreateDefinedGoblinDeck()
                    }
                };

                var mockFactory = MockBuilder
                    .CreateMock<IMagicEntityFactory>()
                    .WithDefaultPlayer();

                // Act.

                var gameContext = new GameContext(definedAgents, mockFactory.Object, RandomGenerator.Default);

                // Assert.

                gameContext
                    .CurrentStatus
                    .Should().Be(GameContext.Status.Starting, "game context should start with <Starting> phase");
            }

            [Fact]
            public void WhenGettingValidParameter_ShouldSetupPlayer()
            {
                // Arrange.

                var definedPlayers = new[]
                {
                    new DefinedBlob.Player
                    {
                        Name = "[_MOCK_PLAYER_01_]",
                        Deck = MockBuilder.CreateDefinedElfDeck()
                    },
                    new DefinedBlob.Player
                    {
                        Name = "[_MOCK_PLAYER_02_]",
                        Deck = MockBuilder.CreateDefinedGoblinDeck()
                    }
                };

                var mockFactory = MockBuilder
                    .CreateMock<IMagicEntityFactory>()
                    .WithDefaultPlayer();

                // Act.

                var gameContext = new GameContext(definedPlayers, mockFactory.Object, RandomGenerator.Default);

                // Assert.

                gameContext
                    .Must().HavePlayers();

                if (gameContext.ActivePlayer.Name == "[_MOCK_PLAYER_01_]")
                {
                    gameContext
                        .NonactivePlayer.Name
                        .Should().Be("[_MOCK_PLAYER_02_]", "nonactive player should be different from active player");
                }
                else
                {
                    gameContext
                        .NonactivePlayer.Name
                        .Should().Be("[_MOCK_PLAYER_01_]", "nonactive player should be different from active player");
                }

                using (new AssertionScope())
                {
                    gameContext
                        .ActivePlayer.Life
                        .Should().Be(20, "active player should begin with full life");

                    gameContext
                        .NonactivePlayer.Life
                        .Should().Be(20, "nonactive player should begin with full life");
                }

                using (new AssertionScope())
                {
                    gameContext
                        .ActivePlayer.Opponent
                        .Should().Be(gameContext.NonactivePlayer, "active player's opponent should be nonactive player");

                    gameContext
                        .NonactivePlayer.Opponent
                        .Should().Be(gameContext.ActivePlayer, "nonactive player's opponent should be active player");
                }
            }

            [Fact]
            public void WhenGettingValidParameter_ShouldSetupPlayerLibrary()
            {
                // Arrange.

                var definedPlayers = new[]
                {
                    new DefinedBlob.Player
                    {
                        Name = "[_MOCK_PLAYER_01_]",
                        Deck = MockBuilder.CreateDefinedElfDeck()
                    },
                    new DefinedBlob.Player
                    {
                        Name = "[_MOCK_PLAYER_02_]",
                        Deck = MockBuilder.CreateDefinedGoblinDeck()
                    }
                };

                var mockFactory = MockBuilder
                    .CreateMock<IMagicEntityFactory>()
                    .WithDefaultPlayer();

                // Act.

                var gameContext = new GameContext(definedPlayers, mockFactory.Object, RandomGenerator.Default);

                // Assert.

                gameContext
                    .Must().HavePlayers();

                using (new AssertionScope())
                {
                    var activeDeck = definedPlayers
                        .Single(player => player.Name == gameContext.ActivePlayer.Name)
                        .Deck;

                    gameContext
                        .ActivePlayer.Library
                        .Must().NotBeNull("active player should have library")
                        .And.BeLibrary()
                        .And.HaveCardQuantity((ushort)(activeDeck.CardQuantity - GameConstant.Hand.MaximumCardCount))
                        .And.HaveUniqueCardInstance()
                        .And.BeSubsetOfDefinedDeck(activeDeck);

                    var nonactiveDeck = definedPlayers
                        .Single(player => player.Name == gameContext.NonactivePlayer.Name)
                        .Deck;

                    gameContext
                        .NonactivePlayer.Library
                        .Must().NotBeNull("nonactive player should have library")
                        .And.BeLibrary()
                        .And.HaveCardQuantity((ushort)(nonactiveDeck.CardQuantity - GameConstant.Hand.MaximumCardCount))
                        .And.HaveUniqueCardInstance()
                        .And.BeSubsetOfDefinedDeck(nonactiveDeck);
                }
            }

            [Fact]
            public void WhenGettingValidParameter_ShouldSetupPlayerHand()
            {
                // Arrange.

                var definedPlayers = new[]
                {
                    new DefinedBlob.Player
                    {
                        Name = "[_MOCK_PLAYER_01_]",
                        Deck = MockBuilder.CreateDefinedElfDeck()
                    },
                    new DefinedBlob.Player
                    {
                        Name = "[_MOCK_PLAYER_02_]",
                        Deck = MockBuilder.CreateDefinedGoblinDeck()
                    }
                };

                var mockFactory = MockBuilder
                    .CreateMock<IMagicEntityFactory>()
                    .WithDefaultPlayer();

                // Act.

                var gameContext = new GameContext(definedPlayers, mockFactory.Object, RandomGenerator.Default);

                // Assert.

                gameContext
                    .Must().HavePlayers();

                using (new AssertionScope())
                {
                    gameContext
                        .ActivePlayer.Hand
                        .Must().NotBeNull("active player should have hand")
                        .And.BeHand()
                        .And.HaveCardQuantity(GameConstant.Hand.MaximumCardCount)
                        .And.HaveUniqueCardInstance()
                        .And.BeSubsetOfDefinedDeck(definedPlayers
                            .Single(player => player.Name == gameContext.ActivePlayer.Name)
                            .Deck);

                    gameContext
                        .NonactivePlayer.Hand
                        .Must().NotBeNull("nonactive player should have hand")
                        .And.BeHand()
                        .And.HaveCardQuantity(GameConstant.Hand.MaximumCardCount)
                        .And.HaveUniqueCardInstance()
                        .And.BeSubsetOfDefinedDeck(definedPlayers
                            .Single(player => player.Name == gameContext.NonactivePlayer.Name)
                            .Deck);
                }
            }
        }
    }
}