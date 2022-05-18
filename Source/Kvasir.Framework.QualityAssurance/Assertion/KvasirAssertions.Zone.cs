// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KvasirAssertions.Zone.cs" company="nGratis">
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
// <creation_timestamp>Wednesday, 30 January 2019 11:57:02 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Framework;

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Engine;
using nGratis.Cop.Olympus.Contract;

public class ZoneAssertion : ReferenceTypeAssertions<IZone, ZoneAssertion>
{
    public ZoneAssertion(IZone zone)
        : base(zone)
    {
        zone.Kind
            .Should().NotBe(ZoneKind.Unknown);
    }

    protected override string Identifier => "zone";

    public AndConstraint<ZoneAssertion> BeLibrary()
    {
        this.Subject.Kind
            .Should().Be(ZoneKind.Library, $"because {this.Identifier} should be library");

        return new AndConstraint<ZoneAssertion>(this);
    }

    public AndConstraint<ZoneAssertion> BeHand()
    {
        this.Subject.Kind
            .Should().Be(ZoneKind.Hand, $"because {this.Identifier} should be hand");

        return new AndConstraint<ZoneAssertion>(this);
    }

    public AndConstraint<ZoneAssertion> BeGraveyard()
    {
        this.Subject.Kind
            .Should().Be(ZoneKind.Graveyard, $"because {this.Identifier} should be graveyard");

        return new AndConstraint<ZoneAssertion>(this);
    }

    public AndConstraint<ZoneAssertion> BeBattlefield()
    {
        this.Subject.Kind
            .Should().Be(ZoneKind.Battlefield, $"because {this.Identifier} should be battlefield");

        return new AndConstraint<ZoneAssertion>(this);
    }

    public AndConstraint<ZoneAssertion> BeStack()
    {
        this.Subject.Kind
            .Should().Be(ZoneKind.Stack, $"because {this.Identifier} should be stack");

        return new AndConstraint<ZoneAssertion>(this);
    }

    public AndConstraint<ZoneAssertion> BeExile()
    {
        this.Subject.Kind
            .Should().Be(ZoneKind.Exile, $"because {this.Identifier} should be exile");

        return new AndConstraint<ZoneAssertion>(this);
    }

    public AndConstraint<ZoneAssertion> BePublic()
    {
        this.Subject.Visibility
            .Should().Be(Visibility.Public, $"because {this.Identifier} should be public");

        return new AndConstraint<ZoneAssertion>(this);
    }

    public AndConstraint<ZoneAssertion> BeHidden()
    {
        this.Subject.Visibility
            .Should().Be(Visibility.Hidden, $"because {this.Identifier} should be hidden");

        return new AndConstraint<ZoneAssertion>(this);
    }

    public AndConstraint<ZoneAssertion> BeSubsetOfConstructedDeck(IDeck deck)
    {
        Guard
            .Require(deck.Cards, $"because {nameof(deck)}.{nameof(deck.Cards)}")
            .Is.Not.Empty();

        using (new AssertionScope())
        {
            foreach (var card in this.Subject.Cards)
            {
                deck.Cards
                    .Should().ContainEquivalentOf(
                        card,
                        $"because {this.Identifier} should contain card defined by deck");
            }
        }

        return new AndConstraint<ZoneAssertion>(this);
    }

    public AndConstraint<ZoneAssertion> HaveUniqueCardInstance()
    {
        using (new AssertionScope())
        {
            this
                .Subject.Cards
                .GroupBy(card => card.GetHashCode())
                .Where(grouping => grouping.Count() > 1)
                .ForEach(grouping => Execute
                    .Assertion
                    .FailWith(
                        $"Expected {this.Identifier} to have unique card instance, " +
                        $"but found {grouping.Count()} [{grouping.First().Name}] cards " +
                        $"with ID [{grouping.First().GetHashCode()}]."));
        }

        return new AndConstraint<ZoneAssertion>(this);
    }

    public AndConstraint<ZoneAssertion> HaveCardQuantity(ushort quantity)
    {
        var actualQuantity = this
            .Subject?.Cards
            .Count() ?? 0;

        Execute
            .Assertion
            .ForCondition(actualQuantity == quantity)
            .FailWith(
                $"Expected {this.Identifier} to have {quantity} cards, " +
                $"but found {actualQuantity}.");

        return new AndConstraint<ZoneAssertion>(this);
    }

    public AndConstraint<ZoneAssertion> HaveCardQuantity(string cardName, ushort quantity)
    {
        Guard
            .Require(cardName, nameof(cardName))
            .Is.Not.Empty();

        var actualQuantity = this
            .Subject?.Cards
            .Count(card => card.Name == cardName) ?? 0;

        Execute
            .Assertion
            .ForCondition(actualQuantity == quantity)
            .FailWith(
                $"Expected {this.Identifier} to have {quantity} [{cardName}] cards, " +
                $"but found {actualQuantity}.");

        return new AndConstraint<ZoneAssertion>(this);
    }
}