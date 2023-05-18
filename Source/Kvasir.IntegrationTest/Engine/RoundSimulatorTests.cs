// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RoundSimulatorTests.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, June 2, 2022 5:13:56 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.IntegrationTest;

using FluentAssertions;
using Moq;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Engine;
using nGratis.AI.Kvasir.Framework;
using nGratis.Cop.Olympus.Contract;
using Xunit;

public class RoundSimulatorTests
{
    // TODO (SHOULD): Add tabletop snapshot based testing using larger deck and longer turn!

    [Fact]
    public void WhenGettingValidConfig_ShouldSimulateUntilCompletion()
    {
        // Arrange.

        var mockLogger = MockBuilder.CreateMock<ILogger>();
        var stubProcessedRepository = new StubProcessedMagicRepository();

        var randomGenerator = new RandomGenerator(42);
        var entityFactory = new MagicEntityFactory(stubProcessedRepository, randomGenerator);
        var roundSimulator = new RoundSimulator(entityFactory, randomGenerator, mockLogger.Object);

        var definedPlayers = new[]
        {
            new DefinedBlob.Player
            {
                Name = "[_MOCK_PLAYER__RED_]",
                DeckCode = "RED_01"
            },
            new DefinedBlob.Player
            {
                Name = "[_MOCK_PLAYER_WHITE_]",
                DeckCode = "WHITE_01"
            }
        };

        var simulationConfig = new SimulationConfig
        {
            MaxTurnCount = 15,
            DefinedPlayers = definedPlayers
        };

        // Act.

        var executionResult = roundSimulator.Simulate(simulationConfig);

        // Assert.

        executionResult
            .HasError
            .Should().BeFalse("because execution should complete without error");

        executionResult
            .Tabletop.TurnId
            .Should().Be(14, "because turn should be executed up to max count");
    }
}