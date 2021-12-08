// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TurnCoordinatorTests.cs" company="nGratis">
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
// <creation_timestamp>Friday, November 26, 2021 10:16:03 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine.UnitTest
{
    using FluentAssertions;
    using Moq;
    using nGratis.Cop.Olympus.Contract;
    using Xunit;

    public class TurnCoordinatorTests
    {
        public class ExecuteStepMethod_CombatPhase
        {
            [Fact]
            public void WhenGettingAttackerWithoutBlocker_ShouldDealDamageToNonactivePlayer()
            {
                // Arrange.

                var tabletop = StubBuilder.CreateDefaultTabletop();
                var attacker = tabletop.CreateAttacker("[_MOCK_ATTACKER_]", 3, 3);

                tabletop
                    .WithAttackingDecision(attacker)
                    .WithBlockingDecision();

                var mockLogger = MockBuilder.CreateMock<ILogger>();
                var turnCoordinator = new TurnCoordinator(tabletop, mockLogger.Object);

                // Act.

                turnCoordinator.ExecuteCombatPhase();

                // Assert.

                tabletop
                    .ActivePlayer.Life
                    .Should().Be(20);

                tabletop
                    .NonactivePlayer.Life
                    .Should().Be(17);

                tabletop
                    .Must().HaveCardInBattlefield(attacker);

                attacker
                    .Damage
                    .Should().Be(0);
            }

            [Fact]
            public void WhenGettingAttackerWithWeakerBlocker_ShouldDestroyBlockerAndDealNoDamageToNonactivePlayer()
            {
                // Arrange.

                var tabletop = StubBuilder.CreateDefaultTabletop();
                var attacker = tabletop.CreateAttacker("[_MOCK_ATTACKER_]", 3, 3);
                var blocker = tabletop.CreateAttacker("[_MOCK_BLOCKER_]", 1, 1);

                var combat = new Combat
                {
                    Attacker = attacker,
                    Blockers = new[] { blocker }
                };

                tabletop
                    .WithAttackingDecision(attacker)
                    .WithBlockingDecision(combat);

                var mockLogger = MockBuilder.CreateMock<ILogger>();
                var turnCoordinator = new TurnCoordinator(tabletop, mockLogger.Object);

                // Act.

                turnCoordinator.ExecuteCombatPhase();

                // Assert.

                tabletop
                    .ActivePlayer.Life
                    .Should().Be(20);

                tabletop
                    .NonactivePlayer.Life
                    .Should().Be(20);

                tabletop
                    .Must().HaveCardInBattlefield(attacker);

                tabletop
                    .Must().HaveCardInNonactiveGraveyard(blocker);

                attacker
                    .Damage
                    .Should().Be(1);

                blocker
                    .Damage
                    .Should().Be(0);
            }

            [Fact]
            public void WhenGettingAttackerWithStrongerBlocker_ShouldDestroyAttackerAndDealNoDamageToNonactivePlayer()
            {
                // Arrange.

                var tabletop = StubBuilder.CreateDefaultTabletop();
                var attacker = tabletop.CreateAttacker("[_MOCK_ATTACKER_]", 3, 3);
                var blocker = tabletop.CreateAttacker("[_MOCK_BLOCKER_]", 5, 5);

                var combat = new Combat
                {
                    Attacker = attacker,
                    Blockers = new[] { blocker }
                };

                tabletop
                    .WithAttackingDecision(attacker)
                    .WithBlockingDecision(combat);

                var mockLogger = MockBuilder.CreateMock<ILogger>();
                var turnCoordinator = new TurnCoordinator(tabletop, mockLogger.Object);

                // Act.

                turnCoordinator.ExecuteCombatPhase();

                // Assert.

                tabletop
                    .ActivePlayer.Life
                    .Should().Be(20);

                tabletop
                    .NonactivePlayer.Life
                    .Should().Be(20);

                tabletop
                    .Must().HaveCardInActiveGraveyard(attacker);

                tabletop
                    .Must().HaveCardInBattlefield(blocker);

                attacker
                    .Damage
                    .Should().Be(0);

                blocker
                    .Damage
                    .Should().Be(3);
            }
        }
    }
}