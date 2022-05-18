// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JudgeDefinition.cs" company="nGratis">
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

namespace nGratis.AI.Kvasir.AcceptanceTest.Definition;

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Engine;
using nGratis.AI.Kvasir.Framework;
using nGratis.Cop.Olympus.Contract;
using TechTalk.SpecFlow;

[Binding]
public sealed class JudgeDefinition
{
    static JudgeDefinition()
    {
        // NOTE: This is a temporary workaround! We can't use <Xunit.TestFramework> attribute
        // because SpecFlow is also using it, and it will cause a conflict if we try adding our own one!

        TestingBootstrapper
            .Create()
            .WithFormatter();
    }

    private Judge _judge;
    private ITabletop _tabletop;

    private ICard _attacker;
    private List<ICard> _blockers;

    private Mock<IStrategy> _mockAttackingStrategy;
    private Mock<IStrategy> _mockBlockingStrategy;

    [BeforeScenario(Order = 0)]
    public void ExecutePreScenario()
    {
        var mockLogger = MockBuilder.CreateMock<ILogger>();

        this._mockAttackingStrategy = MockBuilder
            .CreateMock<IStrategy>()
            .WithDefault();

        this._mockBlockingStrategy = MockBuilder
            .CreateMock<IStrategy>()
            .WithDefault();

        this._tabletop = StubBuilder.CreateDefaultTabletop(
            this._mockAttackingStrategy.Object,
            this._mockBlockingStrategy.Object);

        this._judge = new Judge(mockLogger.Object);

        this._attacker = null;
        this._blockers = new List<ICard>();
    }

    [Given(@"the attacker has power (\d+) and toughness (\d+)")]
    public void GivenAttackerHasPowerAndToughness(int power, int toughness)
    {
        this._attacker = this._tabletop.CreateActiveCreature("[_MOCK_ATTACKER_]", power, toughness);
    }

    [Given(@"the blocker has power (\d+) and toughness (\d+)")]
    public void GivenBlockerHasPowerAndToughness(int power, int toughness)
    {
        var index = 0;
        var blocker = this._tabletop.CreateNonactiveCreature($"[_MOCK_BLOCKER_{index:D2}_]", power, toughness);

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

        var blocker = this._tabletop.CreateNonactiveCreature($"[_MOCK_BLOCKER_{index:D2}_]", power, toughness);

        this._blockers.Insert(index, blocker);
    }

    [When(@"the combat phase is executed")]
    public void WhenCombatPhaseIsExecuted()
    {
        this._mockAttackingStrategy.WithAttackingDecision(this._attacker);

        if (this._blockers.Any())
        {
            this._mockBlockingStrategy.WithBlockingDecision(this._attacker, this._blockers);
        }

        this._judge
            .ExecuteNextTurn(this._tabletop)
            .HasError
            .Should().BeFalse("because executing turn should not fail");
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
    public void ThenPlayerShouldHaveLife(IPlayer player, int life)
    {
        var status = this._tabletop.ActivePlayer == player
            ? "active"
            : "nonactive";

        player
            .Life
            .Should().Be(life, $"because {status} player should have correct life");
    }

    [Then(@"all creatures should be in (battlefield|graveyard) with (\d+) damage")]
    public void ThenAllCreaturesShouldBeInZoneWithDamage(ZoneKind zoneKind, int damage)
    {
        Guard
            .Require(zoneKind, nameof(zoneKind))
            .Is.Not.Default();

        using (new AssertionScope())
        {
            foreach (var creature in this.FindCreatures())
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
                    .FindPart<CreaturePart>().Damage
                    .Should().Be(damage, $"because creature [{creature.Name}] should have correct damage");
            }
        }
    }

    [Then(@"the attacker should be in (battlefield|graveyard) with (\d+) damage")]
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

            this._attacker.FindPart<CreaturePart>().Damage
                .Should().Be(damage, $"because attacker [{this._attacker.Name}] should have correct damage");
        }
    }

    [Then(@"the blocker should be in (battlefield|graveyard) with (\d+) damage")]
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

            blocker.FindPart<CreaturePart>().Damage
                .Should().Be(damage, $"because blocker [{blocker.Name}] should have correct damage");
        }
    }

    [StepArgumentTransformation(@"(active|nonactive) player")]
    public IPlayer TransformToPlayer(string text)
    {
        return text switch
        {
            "active" => this._tabletop.ActivePlayer,
            "nonactive" => this._tabletop.NonactivePlayer,

            _ => throw new KvasirTestingException(
                "Player should be 'active' or 'nonactive'!",
                ("Text", text))
        };
    }

    private IEnumerable<ICard> FindCreatures() => Enumerable
        .Empty<ICard>()
        .Append(this._attacker)
        .Append(this._blockers);
}