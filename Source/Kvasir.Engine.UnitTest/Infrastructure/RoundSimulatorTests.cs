// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RoundSimulatorTests.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Monday, 28 January 2019 4:07:09 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine.UnitTest;

using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Engine;
using nGratis.AI.Kvasir.Framework;
using Xunit;

public class RoundSimulatorTests
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

            var mockEntityFactory = MockBuilder
                .CreateMock<IMagicEntityFactory>()
                .WithDefaultPlayer();

            var mockRoundJudge = MockBuilder
                .CreateMock<IRoundJudge>()
                .WithDefault();

            var roundSimulator = new RoundSimulator(
                mockEntityFactory.Object,
                RandomGenerator.Default,
                mockRoundJudge.Object);

            // Act.

            var simulationResult = roundSimulator.Simulate(simulationConfig);

            // Assert.

            simulationResult
                .Should().NotBeNull();

            simulationResult
                .Tabletop
                .Must().HavePlayers();

            if (simulationResult.Tabletop.ActivePlayer.Name == "[_MOCK_PLAYER_01_]")
            {
                simulationResult
                    .Tabletop.NonActivePlayer.Name
                    .Should().Be(
                        "[_MOCK_PLAYER_02_]",
                        "because nonactive player should be different from active player");
            }
            else
            {
                simulationResult
                    .Tabletop.NonActivePlayer.Name
                    .Should().Be(
                        "[_MOCK_PLAYER_01_]",
                        "because nonactive player should be different from active player");
            }

            using var _ = new AssertionScope();

            simulationResult
                .Tabletop.ActivePlayer.Life
                .Should().Be(20, "because active player should begin with full life");

            simulationResult
                .Tabletop.NonActivePlayer.Life
                .Should().Be(20, "because nonactive player should begin with full life");
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

            var mockEntityFactory = MockBuilder
                .CreateMock<IMagicEntityFactory>()
                .WithDefaultPlayer();

            var mockRoundJudge = MockBuilder
                .CreateMock<IRoundJudge>()
                .WithDefault();

            var roundSimulator = new RoundSimulator(
                mockEntityFactory.Object,
                RandomGenerator.Default,
                mockRoundJudge.Object);

            // Act.

            var simulationResult = roundSimulator.Simulate(simulationConfig);

            // Assert.

            simulationResult
                .Should().NotBeNull();

            simulationResult
                .Tabletop
                .Must().HavePlayers();

            using var _ = new AssertionScope();

            var activeDeck = simulationResult
                    .Tabletop
                    .ActivePlayer.Deck;

            simulationResult
                .Tabletop.ActivePlayer.Library
                .Must().NotBeNull("because active player should have library")
                .And.BeLibrary()
                .And.BeHidden()
                .And.HaveQuantity((ushort)(activeDeck.Cards.Count - MagicConstant.Hand.MaxCardCount))
                .And.HaveUniqueCardInstance()
                .And.BeSubsetOfConstructedDeck(activeDeck);

            var nonActiveDeck = simulationResult
                .Tabletop
                .NonActivePlayer.Deck;

            simulationResult
                .Tabletop.NonActivePlayer.Library
                .Must().NotBeNull("because nonactive player should have library")
                .And.BeLibrary()
                .And.BeHidden()
                .And.HaveQuantity((ushort)(nonActiveDeck.Cards.Count - MagicConstant.Hand.MaxCardCount))
                .And.HaveUniqueCardInstance()
                .And.BeSubsetOfConstructedDeck(nonActiveDeck);
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

            var mockEntityFactory = MockBuilder
                .CreateMock<IMagicEntityFactory>()
                .WithDefaultPlayer();

            var mockRoundJudge = MockBuilder
                .CreateMock<IRoundJudge>()
                .WithDefault();

            var roundSimulator = new RoundSimulator(
                mockEntityFactory.Object,
                RandomGenerator.Default,
                mockRoundJudge.Object);

            // Act.

            var simulationResult = roundSimulator.Simulate(simulationConfig);

            // Assert.

            simulationResult
                .Should().NotBeNull();

            simulationResult
                .Tabletop
                .Must().HavePlayers();

            using var _ = new AssertionScope();

            simulationResult
                .Tabletop.ActivePlayer.Hand
                .Must().NotBeNull("because active player should have hand")
                .And.BeHand()
                .And.BeHidden()
                .And.HaveQuantity(MagicConstant.Hand.MaxCardCount)
                .And.HaveUniqueCardInstance()
                .And.BeSubsetOfConstructedDeck(simulationResult.Tabletop.ActivePlayer.Deck);

            simulationResult
                .Tabletop.NonActivePlayer.Hand
                .Must().NotBeNull("because nonactive player should have hand")
                .And.BeHand()
                .And.BeHidden()
                .And.HaveQuantity(MagicConstant.Hand.MaxCardCount)
                .And.HaveUniqueCardInstance()
                .And.BeSubsetOfConstructedDeck(simulationResult.Tabletop.NonActivePlayer.Deck);
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

            var mockEntityFactory = MockBuilder
                .CreateMock<IMagicEntityFactory>()
                .WithDefaultPlayer();

            var mockRoundJudge = MockBuilder
                .CreateMock<IRoundJudge>()
                .WithDefault();

            var roundSimulator = new RoundSimulator(
                mockEntityFactory.Object,
                RandomGenerator.Default,
                mockRoundJudge.Object);

            // Act.

            var simulationResult = roundSimulator.Simulate(simulationConfig);

            // Assert.

            simulationResult
                .Should().NotBeNull();

            simulationResult
                .Tabletop
                .Must().HavePlayers();

            using var _ = new AssertionScope();

            simulationResult
                .Tabletop.ActivePlayer.Graveyard
                .Must().NotBeNull("because active player should have hand")
                .And.BeGraveyard()
                .And.BePublic()
                .And.HaveQuantity(0);

            simulationResult
                .Tabletop.NonActivePlayer.Graveyard
                .Must().NotBeNull("because nonactive player should have hand")
                .And.BeGraveyard()
                .And.BePublic()
                .And.HaveQuantity(0);
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

            var mockEntityFactory = MockBuilder
                .CreateMock<IMagicEntityFactory>()
                .WithDefaultPlayer();

            var mockRoundJudge = MockBuilder
                .CreateMock<IRoundJudge>()
                .WithDefault();

            var roundSimulator = new RoundSimulator(
                mockEntityFactory.Object,
                RandomGenerator.Default,
                mockRoundJudge.Object);

            // Act.

            var simulationResult = roundSimulator.Simulate(simulationConfig);

            // Assert.

            simulationResult
                .Should().NotBeNull();

            simulationResult
                .Tabletop
                .Must().HavePlayers();

            using var _ = new AssertionScope();

            simulationResult
                .Tabletop.Battlefield
                .Must().BeBattlefield()
                .And.BePublic()
                .And.HaveQuantity(0);

            simulationResult
                .Tabletop.Stack
                .Must().BeStack()
                .And.BePublic()
                .And.HaveQuantity(0);

            simulationResult
                .Tabletop.Exile
                .Must().BeExile()
                .And.BePublic()
                .And.HaveQuantity(0);
        }

        [Fact]
        public void WhenGettingNonTerminalExecutionResult_shouldContinueUntilMaxTurn()
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
                MaxTurnCount = 10,
                DefinedPlayers = definedPlayers
            };

            var mockEntityFactory = MockBuilder
                .CreateMock<IMagicEntityFactory>()
                .WithDefaultPlayer();

            var mockRoundJudge = MockBuilder
                .CreateMock<IRoundJudge>()
                .WithDefault();

            var roundSimulator = new RoundSimulator(
                mockEntityFactory.Object,
                RandomGenerator.Default,
                mockRoundJudge.Object);

            // Act.

            var simulationResult = roundSimulator.Simulate(simulationConfig);

            // Assert.

            simulationResult
                .Should().NotBeNull();

            simulationResult.HasError
                .Should().BeFalse("because simulation result should not contain error message");

            simulationResult.Tabletop
                .Should().NotBeNull();

            using var _ = new AssertionScope();

            simulationResult.Tabletop.TurnId
                .Should().Be(9, "because simulation should not exceed maximum turn count");
        }

        [Fact]
        public void WhenGettingTerminalExecutionResult_shouldNotContinueUntilMaxTurn()
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
                MaxTurnCount = 10,
                DefinedPlayers = definedPlayers
            };

            var mockEntityFactory = MockBuilder
                .CreateMock<IMagicEntityFactory>()
                .WithDefaultPlayer();

            var mockRoundJudge = MockBuilder
                .CreateMock<IRoundJudge>()
                .WithDefault()
                .WithTerminalExecutionResult(3);

            var roundSimulator = new RoundSimulator(
                mockEntityFactory.Object,
                RandomGenerator.Default,
                mockRoundJudge.Object);

            // Act.

            var simulationResult = roundSimulator.Simulate(simulationConfig);

            // Assert.

            simulationResult
                .Should().NotBeNull();

            simulationResult.HasError
                .Should().BeTrue("because simulation result should contain error message");

            simulationResult.Tabletop
                .Should().NotBeNull();

            using var _ = new AssertionScope();

            simulationResult.Tabletop.TurnId
                .Should().Be(3, "because terminal execution result should be generated on this turn");
        }
    }
}