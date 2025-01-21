// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GameSimulatorTests.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, June 2, 2022 5:13:56 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.IntegrationTest;

using Autofac;
using FluentAssertions;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Engine;
using nGratis.AI.Kvasir.Framework;
using Xunit;

public class GameSimulatorTests
{
    // TODO (SHOULD): Add tabletop snapshot based testing using larger deck and longer turn!

    [Fact]
    public void WhenGettingValidConfig_ShouldSimulateUntilCompletion()
    {
        // Arrange.

        var container = new ContainerBuilder()
            .RegisterTestingInfrastructure()
            .RegisterJudge()
            .Build();

        var stubProcessedRepository = new StubProcessedMagicRepository();

        var randomGenerator = new RandomGenerator("[_MOCK_ID_]", 42);
        var judicialAssistant = container.Resolve<IJudicialAssistant>();
        var entityFactory = new EntityFactory(stubProcessedRepository, randomGenerator, judicialAssistant);
        var roundJudge = container.Resolve<IGameJudge>();
        var gameSimulator = new GameSimulator(entityFactory, randomGenerator, roundJudge);

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

        var gameConfig = new GameConfig
        {
            MaxTurnCount = 50,
            DefinedPlayers = definedPlayers
        };

        // Act.

        var gameResult = gameSimulator.Simulate(gameConfig);

        // Assert.

        gameResult
            .HasError
            .Should().BeFalse("because execution should complete without error");

        gameResult
            .Tabletop.TurnId
            .Should().BeLessThan(50, "because turn should be executed up to max count");

        gameResult
            .WinningPlayer.Life
            .Should().BeGreaterThan(0, "because winning player should have positive life");
    }
}