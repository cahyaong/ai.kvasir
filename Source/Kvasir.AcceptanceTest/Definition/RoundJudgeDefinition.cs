// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RoundJudgeDefinition.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Wednesday, December 8, 2021 6:05:38 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.AcceptanceTest;

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Engine;
using nGratis.AI.Kvasir.Framework;
using nGratis.Cop.Olympus.Contract;
using TechTalk.SpecFlow;

[Binding]
public sealed class RoundJudgeDefinition
{
    static RoundJudgeDefinition()
    {
        // NOTE: This is a temporary workaround! We can't use <Xunit.TestFramework> attribute
        // because SpecFlow is also using it, and it will cause a conflict if we try adding our own one!

        TestingBootstrapper
            .Create()
            .WithFormatter();
    }

    private IRoundJudge _roundJudge;
    private ITabletop _tabletop;

    private IPermanent _attackingPermanent;
    private List<IPermanent> _blockingPermanents;

    private Mock<IStrategy> _mockAttackingStrategy;
    private Mock<IStrategy> _mockBlockingStrategy;

    [BeforeScenario(Order = 0)]
    public void ExecutePreScenario()
    {
        var container = new ContainerBuilder()
            .RegisterTestingInfrastructure()
            .RegisterJudge()
            .Build();

        this._mockAttackingStrategy = MockBuilder
            .CreateMock<IStrategy>()
            .WithDefault();

        this._mockBlockingStrategy = MockBuilder
            .CreateMock<IStrategy>()
            .WithDefault();

        this._tabletop = StubBuilder.CreateDefaultTabletop(
            this._mockAttackingStrategy.Object,
            this._mockBlockingStrategy.Object);

        this._roundJudge = container.Resolve<IRoundJudge>();

        this._attackingPermanent = Permanent.Unknown;
        this._blockingPermanents = new List<IPermanent>();
    }

    [Given(@"the attacker has power (\d+) and toughness (\d+)")]
    public void GivenAttackerHasPowerAndToughness(int power, int toughness)
    {
        this._attackingPermanent = this
            ._tabletop
            .CreateActiveCreaturePermanent("[_MOCK_ATTACKER_]", power, toughness);

        this._tabletop.Battlefield.AddToTop(this._attackingPermanent);
    }

    [Given(@"the blocker has power (\d+) and toughness (\d+)")]
    public void GivenBlockerHasPowerAndToughness(int power, int toughness)
    {
        var index = 0;

        var blockingPermanent = this
            ._tabletop
            .CreateNonActiveCreaturePermanent($"[_MOCK_BLOCKER_{index:D2}_]", power, toughness);

        this._blockingPermanents.Insert(index, blockingPermanent);
        this._tabletop.Battlefield.AddToTop(blockingPermanent);
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

        var blockingPermanent = this
            ._tabletop
            .CreateNonActiveCreaturePermanent($"[_MOCK_BLOCKER_{index:D2}_]", power, toughness);

        this._blockingPermanents.Insert(index, blockingPermanent);
    }

    [When(@"the combat phase is executed")]
    public void WhenCombatPhaseIsExecuted()
    {
        this._mockAttackingStrategy.WithAttackingDecision(this._attackingPermanent);

        if (this._blockingPermanents.Any())
        {
            this._mockBlockingStrategy.WithBlockingDecision(this._attackingPermanent, this._blockingPermanents);
        }

        this._roundJudge
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
            .NonActivePlayer.Life
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
            foreach (var permanent in this.FindCreaturePermanents())
            {
                if (zoneKind == ZoneKind.Battlefield)
                {
                    this._tabletop
                        .Must().HavePermanentInBattlefield(permanent);
                }
                else
                {
                    throw new KvasirTestingException(
                        "Assertion does not handle the given zone!",
                        ("Zone Kind", zoneKind));
                }

                permanent
                    .FindPart<CreaturePart>().Damage
                    .Should().Be(damage, $"because creature [{permanent.Name}] should have correct damage");
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
                    .Must().HavePermanentInBattlefield(this._attackingPermanent);
            }
            else if (zoneKind == ZoneKind.Graveyard)
            {
                this._tabletop
                    .Must().HaveCardInActiveGraveyard(this._attackingPermanent);
            }

            this._attackingPermanent.FindPart<CreaturePart>().Damage
                .Should().Be(damage, $"because attacker [{this._attackingPermanent.Name}] should have correct damage");
        }
    }

    [Then(@"the blocker should be in (battlefield|graveyard) with (\d+) damage")]
    public void ThenBlockerShouldBeInZoneWithDamage(ZoneKind zoneKind, int damage)
    {
        Guard
            .Require(zoneKind, nameof(zoneKind))
            .Is.Not.Default();

        var blocker = this._blockingPermanents[0];

        using (new AssertionScope())
        {
            if (zoneKind == ZoneKind.Battlefield)
            {
                this._tabletop
                    .Must().HavePermanentInBattlefield(blocker);
            }
            else if (zoneKind == ZoneKind.Graveyard)
            {
                this._tabletop
                    .Must().HaveCardInNonActiveGraveyard(blocker);
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
            "nonactive" => this._tabletop.NonActivePlayer,

            _ => throw new KvasirTestingException(
                "Player should be 'active' or 'nonactive'!",
                ("Text", text))
        };
    }

    private IEnumerable<IPermanent> FindCreaturePermanents() => Enumerable
        .Empty<IPermanent>()
        .Append(this._attackingPermanent)
        .Append(this._blockingPermanents);
}