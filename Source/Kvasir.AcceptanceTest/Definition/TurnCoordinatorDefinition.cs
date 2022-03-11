// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TurnCoordinatorDefinition.cs" company="nGratis">
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
// <creation_timestamp>Wednesday, December 8, 2021 6:05:38 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.AcceptanceTest.Definition
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using FluentAssertions.Execution;
    using Humanizer;
    using Moq.AI.Kvasir;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Engine;
    using nGratis.AI.Kvasir.Framework;
    using nGratis.Cop.Olympus.Contract;
    using TechTalk.SpecFlow;

    [Binding]
    public sealed class TurnCoordinatorDefinition
    {
        private Tabletop _tabletop;
        private TurnCoordinator _turnCoordinator;

        private Creature _attacker;
        private List<Creature> _blockers;

        public IEnumerable<Creature> Creatures => Enumerable
            .Empty<Creature>()
            .Append(this._attacker)
            .Append(this._blockers);

        [BeforeScenario(Order = 0)]
        public void ExecutePreScenario()
        {
            var mockLogger = MockBuilder.CreateMock<ILogger>();

            this._tabletop = StubBuilder.CreateDefaultTabletop();
            this._turnCoordinator = new TurnCoordinator(this._tabletop, mockLogger.Object);

            this._attacker = null;
            this._blockers = new List<Creature>();
        }

        [Given(@"the attacker has power (\d+) and toughness (\d+)")]
        public void GivenAttackerHasPowerAndToughness(int power, int toughness)
        {
            this._attacker = this._tabletop.CreateAttacker("[_MOCK_ATTACKER_]", power, toughness);
        }

        [Given(@"the blocker has power (\d+) and toughness (\d+)")]
        public void GivenBlockerHasPowerAndToughness(int power, int toughness)
        {
            var index = 0;
            var blocker = this._tabletop.CreateBlocker($"[_MOCK_BLOCKER_{index:D2}_]", power, toughness);

            this._blockers.Insert(index, blocker);
        }

        [Given(@"the (\w+) blocker has power (\d+) and toughness (\d+)")]
        public void GivenBlockerHasPowerAndToughness(string ordinal, int power, int toughness)
        {
            var index = !string.IsNullOrEmpty(ordinal)
                ? ordinal.ToNumericalValue() - 1
                : 0;

            Guard
                .Ensure(index, nameof(index))
                .Is.GreaterThanOrEqualTo(0);

            var blocker = this._tabletop.CreateBlocker($"[_MOCK_BLOCKER_{index:D2}_]", power, toughness);

            this._blockers.Insert(index, blocker);
        }

        [When(@"the combat phase is executed")]
        public void WhenCombatPhaseIsExecuted()
        {
            this._tabletop
                .WithAttackingDecision(this._attacker)
                .WithBlockingDecision();

            if (this._blockers.Any())
            {
                this._tabletop.WithBlockingDecision(new Combat
                {
                    Attacker = this._attacker,
                    Blockers = this._blockers
                });
            }
            else
            {
                this._tabletop.WithBlockingDecision();
            }

            this._turnCoordinator
                .ExecuteStep(0, Ticker.PhaseState.Combat, Ticker.StepState.DeclareAttackers)
                .HasError
                .Should().BeFalse("because declaring attackers should not fail");

            this._turnCoordinator
                .ExecuteStep(0, Ticker.PhaseState.Combat, Ticker.StepState.AssignBlockers)
                .HasError
                .Should().BeFalse("because assigning blockers should not fail");

            this._turnCoordinator
                .ExecuteStep(0, Ticker.PhaseState.Combat, Ticker.StepState.CombatDamage)
                .HasError
                .Should().BeFalse("because resolving combat damage should not fail");
        }

        [Then(@"all players should have (\d+) life")]
        public void ThenAllPlayersShouldHaveLife(int life)
        {
            this._tabletop
                .ActivePlayer.Life
                .Should().Be(life, "because active player should have correct life");

            this._tabletop
                .NonactivePlayer.Life
                .Should().Be(life, "because nonactive player should have correct life");
        }

        [Then(@"the (.+) should have (\d+) life")]
        public void ThenPlayerShouldHaveLife(Player player, int life)
        {
            var status = this._tabletop.ActivePlayer == player
                ? "active"
                : "nonactive";

            player
                .Life
                .Should().Be(life, $"because {status} player should have correct life");
        }

        [Then(@"all creatures should be in (.+) with (\d+) damage")]
        public void ThenAllCreaturesShouldBeInZoneWithDamage(ZoneKind zoneKind, int damage)
        {
            Guard
                .Require(zoneKind, nameof(zoneKind))
                .Is.Not.Default();

            using (new AssertionScope())
            {
                foreach (var creature in this.Creatures)
                {
                    if (zoneKind == ZoneKind.Battlefield)
                    {
                        this._tabletop
                            .Must().HaveCardInBattlefield(creature);
                    }
                    else
                    {
                        throw new KvasirTestingException(
                            "Assertion does not handle the given zone!",
                            ("Zone Kind", zoneKind));
                    }

                    creature
                        .Damage
                        .Should().Be(damage, $"because creature [{creature.Name}] should have correct damage");
                }
            }
        }

        [Then(@"the attacker should be in (.+) with (\d+) damage")]
        public void ThenAttackerShouldBeInZoneWithDamage(ZoneKind zoneKind, int damage)
        {
            Guard
                .Require(zoneKind, nameof(zoneKind))
                .Is.Not.Default();

            using (new AssertionScope())
            {
                if (zoneKind == ZoneKind.Battlefield)
                {
                    this._tabletop
                        .Must().HaveCardInBattlefield(this._attacker);
                }
                else if (zoneKind == ZoneKind.Graveyard)
                {
                    this._tabletop
                        .Must().HaveCardInActiveGraveyard(this._attacker);
                }
                else
                {
                    throw new KvasirTestingException(
                        "Assertion does not handle the given zone!",
                        ("Zone Kind", zoneKind));
                }

                this._attacker.Damage
                    .Should().Be(damage, $"because attacker [{this._attacker.Name}] should have correct damage");
            }
        }

        [Then(@"the blocker should be in (.+) with (\d+) damage")]
        public void ThenBlockerShouldBeInZoneWithDamage(ZoneKind zoneKind, int damage)
        {
            Guard
                .Require(zoneKind, nameof(zoneKind))
                .Is.Not.Default();

            var blocker = this._blockers[0];

            using (new AssertionScope())
            {
                if (zoneKind == ZoneKind.Battlefield)
                {
                    this._tabletop
                        .Must().HaveCardInBattlefield(blocker);
                }
                else if (zoneKind == ZoneKind.Graveyard)
                {
                    this._tabletop
                        .Must().HaveCardInNonactiveGraveyard(blocker);
                }
                else
                {
                    throw new KvasirTestingException(
                        "Assertion does not handle the given zone!",
                        ("Zone Kind", zoneKind));
                }

                blocker.Damage
                    .Should().Be(damage, $"because blocker [{blocker.Name}] should have correct damage");
            }
        }

        [StepArgumentTransformation(@"(battlefield)")]
        public ZoneKind TransformToZoneKind(string text)
        {
            Guard
                .Require(text, nameof(text))
                .Is.Not.Null();

            return Enum.Parse<ZoneKind>(text.Titleize());
        }

        [StepArgumentTransformation(@"(active|nonactive) player")]
        public Player TransformToPlayer(string text)
        {
            Guard
                .Require(text, nameof(text))
                .Is.Not.Null();

            return text switch
            {
                "active" => this._tabletop.ActivePlayer,
                "nonactive" => this._tabletop.NonactivePlayer,

                _ => throw new KvasirTestingException(
                    "Player should be 'active' or 'nonactive'!",
                    ("Text", text))
            };
        }
    }
}