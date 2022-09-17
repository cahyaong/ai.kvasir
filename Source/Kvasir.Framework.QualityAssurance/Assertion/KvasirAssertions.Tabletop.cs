// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KvasirAssertions.Tabletop.cs" company="nGratis">
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