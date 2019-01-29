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

                var agentDefinitions = new[]
                {
                    new AgentDefinition { Name = "[_AGENT_01_]" },
                    new AgentDefinition { Name = "[_AGENT_02_]" }
                };

                var mockFactory = MockBuilder
                    .CreateMock<IMagicObjectFactory>()
                    .WithAgent("[_AGENT_01_]")
                    .WithAgent("[_AGENT_02_]");

                // Act.

                var gameContext = new GameContext(agentDefinitions, mockFactory.Object, RandomGenerator.Default);

                // Assert.

                gameContext
                    .CurrentPhase
                    .Should().Be(GameContext.Phase.Beginning);
            }

            [Fact]
            public void WhenGettingValidParameter_ShouldSetupAgent()
            {
                // Arrange.

                var agentDefinitions = new[]
                {
                    new AgentDefinition { Name = "[_AGENT_01_]" },
                    new AgentDefinition { Name = "[_AGENT_02_]" }
                };

                var mockFactory = MockBuilder
                    .CreateMock<IMagicObjectFactory>()
                    .WithAgent("[_AGENT_01_]")
                    .WithAgent("[_AGENT_02_]");

                // Act.

                var gameContext = new GameContext(agentDefinitions, mockFactory.Object, RandomGenerator.Default);

                // Assert.

                gameContext
                    .ActiveAgent
                    .Should().NotBeNull();

                gameContext
                    .PassiveAgent
                    .Should().NotBeNull();

                if (gameContext.ActiveAgent.Name == "[_AGENT_01_]")
                {
                    gameContext
                        .PassiveAgent.Name
                        .Should().Be("[_AGENT_02_]");
                }
                else
                {
                    gameContext
                        .PassiveAgent.Name
                        .Should().Be("[_AGENT_01_]");
                }

                using (new AssertionScope())
                {
                    gameContext
                        .ActiveAgent.Life
                        .Should().Be(20);

                    gameContext
                        .PassiveAgent.Life
                        .Should().Be(20);
                }
            }
        }
    }
}