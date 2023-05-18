// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KvasirAssertions.Tabletop.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, February 20, 2020 7:18:36 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Framework;

using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using nGratis.AI.Kvasir.Engine;

public class TabletopAssertion : ReferenceTypeAssertions<ITabletop, TabletopAssertion>
{
    public TabletopAssertion(ITabletop tabletop)
        : base(tabletop)
    {
    }

    protected override string Identifier => "tabletop";

    public AndConstraint<TabletopAssertion> HavePlayers()
    {
        using (new AssertionScope())
        {
            this
                .Subject.ActivePlayer
                .Should().NotBeNull($"{this.Identifier} should have active player");

            this
                .Subject.NonActivePlayer
                .Should().NotBeNull($"{this.Identifier} should have nonactive player");
        }

        return new AndConstraint<TabletopAssertion>(this);
    }

    public AndConstraint<TabletopAssertion> HavePermanentInBattlefield(IPermanent permanent)
    {
        using (new AssertionScope())
        {
            this
                .Subject.Battlefield.FindAll()
                .Should().Contain(
                    permanent,
                    $"{this.Identifier} should have card [{permanent.Name}] in battlefield");

            this
                .Subject.ActivePlayer.Graveyard.FindAll()
                .Should().NotContain(
                    permanent.Card,
                    $"{this.Identifier} should not have card [{permanent.Name}] in active graveyard");

            this
                .Subject.NonActivePlayer.Graveyard.FindAll()
                .Should().NotContain(
                    permanent.Card,
                    $"{this.Identifier} should not have card [{permanent.Name}] in nonactive graveyard");
        }

        return new AndConstraint<TabletopAssertion>(this);
    }

    public AndConstraint<TabletopAssertion> HaveCardInActiveGraveyard(IPermanent permanent)
    {
        using (new AssertionScope())
        {
            this
                .Subject.Battlefield.FindAll()
                .Should().NotContain(
                    permanent,
                    $"{this.Identifier} should not have card [{permanent.Name}] in battlefield");

            this
                .Subject.ActivePlayer.Graveyard.FindAll()
                .Should().Contain(
                    permanent.Card,
                    $"{this.Identifier} should have card [{permanent.Name}] in active graveyard");

            this
                .Subject.NonActivePlayer.Graveyard.FindAll()
                .Should().NotContain(
                    permanent.Card,
                    $"{this.Identifier} should not have card [{permanent.Name}] in nonactive graveyard");
        }

        return new AndConstraint<TabletopAssertion>(this);
    }

    public AndConstraint<TabletopAssertion> HaveCardInNonActiveGraveyard(IPermanent permanent)
    {
        using (new AssertionScope())
        {
            this
                .Subject.Battlefield.FindAll()
                .Should().NotContain(
                    permanent,
                    $"{this.Identifier} should not have card [{permanent.Name}] in battlefield");

            this
                .Subject.ActivePlayer.Graveyard.FindAll()
                .Should().NotContain(
                    permanent.Card,
                    $"{this.Identifier} should not have card [{permanent.Name}] in active graveyard");

            this
                .Subject.NonActivePlayer.Graveyard.FindAll()
                .Should().Contain(
                    permanent.Card,
                    $"{this.Identifier} should have card [{permanent.Name}] in nonactive graveyard");
        }

        return new AndConstraint<TabletopAssertion>(this);
    }
}