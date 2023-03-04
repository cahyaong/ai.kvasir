// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RoundSimulatorTests.cs" company="nGratis">
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