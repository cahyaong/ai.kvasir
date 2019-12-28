// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GameContextTests.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2018 Cahya Ong
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

namespace nGratis.AI.Kvasir.Core.Test
{
    using System.Linq;
    using FluentAssertions;
    using FluentAssertions.Execution;
    using nGratis.AI.Kvasir.Contract;
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
                    new DefinedBlob.Agent
                    {
                        Name = "[_MOCK_AGENT_01_]",
                        Deck = MockBuilder.CreateDefinedElfDeck()
                    },
                    new DefinedBlob.Agent
                    {
                        Name = "[_MOCK_AGENT_02_]",
                        Deck = MockBuilder.CreateDefinedGoblinDeck()
                    }
                };

                var mockFactory = MockBuilder
                    .CreateMock<IMagicObjectFactory>()
                    .WithDefaultAgent();

                // Act.

                var gameContext = new GameContext(definedAgents, mockFactory.Object, RandomGenerator.Default);

                // Assert.

                gameContext
                    .CurrentPhase
                    .Should().Be(GameContext.Phase.Beginning, "game context should start with <Beginning> phase");
            }

            [Fact]
            public void WhenGettingValidParameter_ShouldSetupAgent()
            {
                // Arrange.

                var definedAgents = new[]
                {
                    new DefinedBlob.Agent
                    {
                        Name = "[_MOCK_AGENT_01_]",
                        Deck = MockBuilder.CreateDefinedElfDeck()
                    },
                    new DefinedBlob.Agent
                    {
                        Name = "[_MOCK_AGENT_02_]",
                        Deck = MockBuilder.CreateDefinedGoblinDeck()
                    }
                };

                var mockFactory = MockBuilder
                    .CreateMock<IMagicObjectFactory>()
                    .WithDefaultAgent();

                // Act.

                var gameContext = new GameContext(definedAgents, mockFactory.Object, RandomGenerator.Default);

                // Assert.

                using (new AssertionScope())
                {
                    gameContext
                        .ActiveAgent
                        .Should().NotBeNull("game context should have active agent");

                    gameContext
                        .PassiveAgent
                        .Should().NotBeNull("game context should have passive agent");
                }

                if (gameContext.ActiveAgent.Name == "[_MOCK_AGENT_01_]")
                {
                    gameContext
                        .PassiveAgent.Name
                        .Should().Be("[_MOCK_AGENT_02_]", "passive agent should be different from active agent");
                }
                else
                {
                    gameContext
                        .PassiveAgent.Name
                        .Should().Be("[_MOCK_AGENT_01_]", "passive agent should be different from active agent");
                }

                using (new AssertionScope())
                {
                    gameContext
                        .ActiveAgent.Life
                        .Should().Be(20, "active agent should begin with full life");

                    gameContext
                        .PassiveAgent.Life
                        .Should().Be(20, "passive agent should begin with full life");
                }
            }

            [Fact]
            public void WhenGettingValidParameter_ShouldSetupLibrary()
            {
                // Arrange.

                var definedAgents = new[]
                {
                    new DefinedBlob.Agent
                    {
                        Name = "[_MOCK_AGENT_01_]",
                        Deck = MockBuilder.CreateDefinedElfDeck()
                    },
                    new DefinedBlob.Agent
                    {
                        Name = "[_MOCK_AGENT_02_]",
                        Deck = MockBuilder.CreateDefinedGoblinDeck()
                    }
                };

                var mockFactory = MockBuilder
                    .CreateMock<IMagicObjectFactory>()
                    .WithDefaultAgent();

                // Act.

                var gameContext = new GameContext(definedAgents, mockFactory.Object, RandomGenerator.Default);

                // Assert.

                using (new AssertionScope())
                {
                    gameContext
                        .ActiveAgent
                        .Should().NotBeNull("game context should have active agent");

                    gameContext
                        .PassiveAgent
                        .Should().NotBeNull("game context should have passive agent");
                }

                var definedAgent = definedAgents.ToDictionary(
                    agent => agent.Name,
                    agent => agent.Deck);

                using (new AssertionScope())
                {
                    gameContext
                        .ActiveAgent.Library
                        .Must().NotBeNull("active agent should have library")
                        .And.MatchDefinedDeck(definedAgents
                            .Single(agent => agent.Name == gameContext.ActiveAgent.Name)
                            .Deck);

                    gameContext
                        .PassiveAgent.Library
                        .Must().NotBeNull("passive agent should have library")
                        .And.MatchDefinedDeck(definedAgents
                            .Single(agent => agent.Name == gameContext.PassiveAgent.Name)
                            .Deck);
                }
            }
        }
    }
}