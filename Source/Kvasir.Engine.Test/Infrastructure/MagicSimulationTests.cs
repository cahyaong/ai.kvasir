// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicSimulationTests.cs" company="nGratis">
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
// <creation_timestamp>Monday, 28 January 2019 4:07:09 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine.Test
{
    using FluentAssertions;
    using FluentAssertions.Execution;
    using Moq.AI.Kvasir;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Engine;
    using nGratis.Cop.Olympus.Contract;
    using Xunit;

    public class MagicSimulationTests
    {
        public class SimulateMethod
        {
            [Fact]
            public void WhenGettingValidParameter_ShouldSetupPlayer()
            {
                // Arrange.

                var definedPlayers = new[]
                {
                    new DefinedBlob.Player
                    {
                        Name = "[_MOCK_PLAYER_01_]",
                        DeckCode = "[_MOCK_CODE_ELF_]"
                    },
                    new DefinedBlob.Player
                    {
                        Name = "[_MOCK_PLAYER_02_]",
                        DeckCode = "[_MOCK_CODE_GOBLIN_]"
                    }
                };

                var simulationConfig = new SimulationConfig
                {
                    MaxTurnCount = 0,
                    DefinedPlayers = definedPlayers
                };

                var mockFactory = MockBuilder
                    .CreateMock<IMagicEntityFactory>()
                    .WithDefaultPlayer();

                var mockLogger = MockBuilder
                    .CreateMock<ILogger>();

                var simulation = new MagicSimulation(mockFactory.Object, RandomGenerator.Default, mockLogger.Object);

                // Act.

                var simulationResult = simulation.Simulate(simulationConfig);

                // Assert.

                simulationResult
                    .Should().NotBeNull()
                    .And.BeOfType<SimulationResult>();

                simulationResult
                    .Tabletop
                    .Must().HavePlayers();

                if (simulationResult.Tabletop.ActivePlayer.Name == "[_MOCK_PLAYER_01_]")
                {
                    simulationResult
                        .Tabletop
                        .NonactivePlayer.Name
                        .Should().Be("[_MOCK_PLAYER_02_]", "nonactive player should be different from active player");
                }
                else
                {
                    simulationResult
                        .Tabletop
                        .NonactivePlayer.Name
                        .Should().Be("[_MOCK_PLAYER_01_]", "nonactive player should be different from active player");
                }

                using (new AssertionScope())
                {
                    simulationResult
                        .Tabletop
                        .ActivePlayer.Life
                        .Should().Be(20, "active player should begin with full life");

                    simulationResult
                        .Tabletop
                        .NonactivePlayer.Life
                        .Should().Be(20, "nonactive player should begin with full life");
                }

                using (new AssertionScope())
                {
                    simulationResult
                        .Tabletop
                        .ActivePlayer.Opponent
                        .Should().Be(
                            simulationResult.Tabletop.NonactivePlayer,
                            "active player's opponent should be nonactive player");

                    simulationResult
                        .Tabletop
                        .NonactivePlayer.Opponent
                        .Should().Be(
                            simulationResult.Tabletop.ActivePlayer,
                            "nonactive player's opponent should be active player");
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
                        DeckCode = "[_MOCK_CODE_ELF_]"
                    },
                    new DefinedBlob.Player
                    {
                        Name = "[_MOCK_PLAYER_02_]",
                        DeckCode = "[_MOCK_CODE_GOBLIN_]"
                    }
                };

                var simulationConfig = new SimulationConfig
                {
                    MaxTurnCount = 0,
                    DefinedPlayers = definedPlayers
                };

                var mockFactory = MockBuilder
                    .CreateMock<IMagicEntityFactory>()
                    .WithDefaultPlayer();

                var mockLogger = MockBuilder
                    .CreateMock<ILogger>();

                var simulation = new MagicSimulation(mockFactory.Object, RandomGenerator.Default, mockLogger.Object);

                // Act.

                var simulationResult = simulation.Simulate(simulationConfig);

                // Assert.

                simulationResult
                    .Should().NotBeNull()
                    .And.BeOfType<SimulationResult>();

                simulationResult
                    .Tabletop
                    .Must().HavePlayers();

                using (new AssertionScope())
                {
                    var activeDeck = simulationResult
                        .Tabletop
                        .ActivePlayer.Deck;

                    simulationResult
                        .Tabletop
                        .ActivePlayer.Library
                        .Must().NotBeNull("active player should have library")
                        .And.BeLibrary()
                        .And.BeHidden()
                        .And.HaveCardQuantity((ushort)(activeDeck.Cards.Count - GameConstant.Hand.MaximumCardCount))
                        .And.HaveUniqueCardInstance()
                        .And.BeSubsetOfConstructedDeck(activeDeck);

                    var nonactiveDeck = simulationResult
                        .Tabletop
                        .NonactivePlayer.Deck;

                    simulationResult
                        .Tabletop
                        .NonactivePlayer.Library
                        .Must().NotBeNull("nonactive player should have library")
                        .And.BeLibrary()
                        .And.BeHidden()
                        .And.HaveCardQuantity((ushort)(nonactiveDeck.Cards.Count - GameConstant.Hand.MaximumCardCount))
                        .And.HaveUniqueCardInstance()
                        .And.BeSubsetOfConstructedDeck(nonactiveDeck);
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
                        DeckCode = "[_MOCK_CODE_ELF_]"
                    },
                    new DefinedBlob.Player
                    {
                        Name = "[_MOCK_PLAYER_02_]",
                        DeckCode = "[_MOCK_CODE_GOBLIN_]"
                    }
                };

                var simulationConfig = new SimulationConfig
                {
                    MaxTurnCount = 0,
                    DefinedPlayers = definedPlayers
                };

                var mockFactory = MockBuilder
                    .CreateMock<IMagicEntityFactory>()
                    .WithDefaultPlayer();

                var mockLogger = MockBuilder
                    .CreateMock<ILogger>();

                var simulation = new MagicSimulation(mockFactory.Object, RandomGenerator.Default, mockLogger.Object);

                // Act.

                var simulationResult = simulation.Simulate(simulationConfig);

                // Assert.

                simulationResult
                    .Should().NotBeNull()
                    .And.BeOfType<SimulationResult>();

                simulationResult
                    .Tabletop
                    .Must().HavePlayers();

                using (new AssertionScope())
                {
                    simulationResult
                        .Tabletop
                        .ActivePlayer.Hand
                        .Must().NotBeNull("active player should have hand")
                        .And.BeHand()
                        .And.BeHidden()
                        .And.HaveCardQuantity(GameConstant.Hand.MaximumCardCount)
                        .And.HaveUniqueCardInstance()
                        .And.BeSubsetOfConstructedDeck(simulationResult.Tabletop.ActivePlayer.Deck);

                    simulationResult
                        .Tabletop
                        .NonactivePlayer.Hand
                        .Must().NotBeNull("nonactive player should have hand")
                        .And.BeHand()
                        .And.BeHidden()
                        .And.HaveCardQuantity(GameConstant.Hand.MaximumCardCount)
                        .And.HaveUniqueCardInstance()
                        .And.BeSubsetOfConstructedDeck(simulationResult.Tabletop.NonactivePlayer.Deck);
                }
            }

            [Fact]
            public void WhenGettingValidParameter_ShouldSetupPlayerGraveyard()
            {
                // Arrange.

                var definedPlayers = new[]
                {
                    new DefinedBlob.Player
                    {
                        Name = "[_MOCK_PLAYER_01_]",
                        DeckCode = "[_MOCK_CODE_ELF_]"
                    },
                    new DefinedBlob.Player
                    {
                        Name = "[_MOCK_PLAYER_02_]",
                        DeckCode = "[_MOCK_CODE_GOBLIN_]"
                    }
                };

                var simulationConfig = new SimulationConfig
                {
                    MaxTurnCount = 0,
                    DefinedPlayers = definedPlayers
                };

                var mockFactory = MockBuilder
                    .CreateMock<IMagicEntityFactory>()
                    .WithDefaultPlayer();

                var mockLogger = MockBuilder
                    .CreateMock<ILogger>();

                var simulation = new MagicSimulation(mockFactory.Object, RandomGenerator.Default, mockLogger.Object);

                // Act.

                var simulationResult = simulation.Simulate(simulationConfig);

                // Assert.

                simulationResult
                    .Should().NotBeNull()
                    .And.BeOfType<SimulationResult>();

                simulationResult
                    .Tabletop
                    .Must().HavePlayers();

                using (new AssertionScope())
                {
                    simulationResult
                        .Tabletop
                        .ActivePlayer.Graveyard
                        .Must().NotBeNull("active player should have hand")
                        .And.BeGraveyard()
                        .And.BePublic()
                        .And.HaveCardQuantity(0);

                    simulationResult
                        .Tabletop
                        .NonactivePlayer.Graveyard
                        .Must().NotBeNull("nonactive player should have hand")
                        .And.BeGraveyard()
                        .And.BePublic()
                        .And.HaveCardQuantity(0);
                }
            }

            [Fact]
            public void WhenGettingValidParameter_ShouldSetupSharedZones()
            {
                // Arrange.

                var definedPlayers = new[]
                {
                    new DefinedBlob.Player
                    {
                        Name = "[_MOCK_PLAYER_01_]",
                        DeckCode = "[_MOCK_CODE_ELF_]"
                    },
                    new DefinedBlob.Player
                    {
                        Name = "[_MOCK_PLAYER_02_]",
                        DeckCode = "[_MOCK_CODE_GOBLIN_]"
                    }
                };

                var simulationConfig = new SimulationConfig
                {
                    MaxTurnCount = 0,
                    DefinedPlayers = definedPlayers
                };

                var mockFactory = MockBuilder
                    .CreateMock<IMagicEntityFactory>()
                    .WithDefaultPlayer();

                var mockLogger = MockBuilder
                    .CreateMock<ILogger>();

                var simulation = new MagicSimulation(mockFactory.Object, RandomGenerator.Default, mockLogger.Object);

                // Act.

                var simulationResult = simulation.Simulate(simulationConfig);

                // Assert.

                simulationResult
                    .Should().NotBeNull()
                    .And.BeOfType<SimulationResult>();

                simulationResult
                    .Tabletop
                    .Must().HavePlayers();

                using (new AssertionScope())
                {
                    simulationResult
                        .Tabletop
                        .Battlefield
                        .Must().BeBattlefield()
                        .And.BePublic()
                        .And.HaveCardQuantity(0);

                    simulationResult
                        .Tabletop
                        .Stack
                        .Must().BeStack()
                        .And.BePublic()
                        .And.HaveCardQuantity(0);

                    simulationResult
                        .Tabletop
                        .Exile
                        .Must().BeExile()
                        .And.BePublic()
                        .And.HaveCardQuantity(0);

                    simulationResult
                        .Tabletop
                        .Command
                        .Must().BeCommand()
                        .And.BePublic()
                        .And.HaveCardQuantity(0);

                    simulationResult
                        .Tabletop
                        .Ante
                        .Must().BeAnte()
                        .And.BePublic()
                        .And.HaveCardQuantity(0);
                }
            }
        }
    }
}