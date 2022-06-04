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

public class ZoneAssertion<TEntity> : ReferenceTypeAssertions<IZone<TEntity>, ZoneAssertion<TEntity>>
    where TEntity : IDiagnostic
{
    public ZoneAssertion(IZone<TEntity> zone)
        : base(zone)
    {
        zone.Kind
            .Should().NotBe(ZoneKind.Unknown);
    }

    protected override string Identifier => "zone";

    public AndConstraint<ZoneAssertion<TEntity>> BeLibrary()
    {
        this.Subject.Kind
            .Should().Be(ZoneKind.Library, $"because {this.Identifier} should be library");

        return new AndConstraint<ZoneAssertion<TEntity>>(this);
    }

    public AndConstraint<ZoneAssertion<TEntity>> BeHand()
    {
        this.Subject.Kind
            .Should().Be(ZoneKind.Hand, $"because {this.Identifier} should be hand");

        return new AndConstraint<ZoneAssertion<TEntity>>(this);
    }

    public AndConstraint<ZoneAssertion<TEntity>> BeGraveyard()
    {
        this.Subject.Kind
            .Should().Be(ZoneKind.Graveyard, $"because {this.Identifier} should be graveyard");

        return new AndConstraint<ZoneAssertion<TEntity>>(this);
    }

    public AndConstraint<ZoneAssertion<TEntity>> BeBattlefield()
    {
        this.Subject.Kind
            .Should().Be(ZoneKind.Battlefield, $"because {this.Identifier} should be battlefield");

        return new AndConstraint<ZoneAssertion<TEntity>>(this);
    }

    public AndConstraint<ZoneAssertion<TEntity>> BeStack()
    {
        this.Subject.Kind
            .Should().Be(ZoneKind.Stack, $"because {this.Identifier} should be stack");

        return new AndConstraint<ZoneAssertion<TEntity>>(this);
    }

    public AndConstraint<ZoneAssertion<TEntity>> BeExile()
    {
        this.Subject.Kind
            .Should().Be(ZoneKind.Exile, $"because {this.Identifier} should be exile");

        return new AndConstraint<ZoneAssertion<TEntity>>(this);
    }

    public AndConstraint<ZoneAssertion<TEntity>> BePublic()
    {
        this.Subject.Visibility
            .Should().Be(Visibility.Public, $"because {this.Identifier} should be public");

        return new AndConstraint<ZoneAssertion<TEntity>>(this);
    }

    public AndConstraint<ZoneAssertion<TEntity>> BeHidden()
    {
        this.Subject.Visibility
            .Should().Be(Visibility.Hidden, $"because {this.Identifier} should be hidden");

        return new AndConstraint<ZoneAssertion<TEntity>>(this);
    }

    public AndConstraint<ZoneAssertion<TEntity>> BeSubsetOfConstructedDeck(IDeck deck)
    {
        Guard
            .Require(typeof(TEntity), nameof(TEntity))
            .Is.EqualTo(typeof(ICard));

        Guard
            .Require(deck.Cards, $"because {nameof(deck)}.{nameof(deck.Cards)}")
            .Is.Not.Empty();

        using (new AssertionScope())
        {
            var cards = this
                .Subject
                .FindAll()
                .OfType<ICard>();

            foreach (var card in cards)
            {
                deck.Cards
                    .Should().ContainEquivalentOf(
                        card,
                        $"because {this.Identifier} should contain card defined by deck");
            }
        }

        return new AndConstraint<ZoneAssertion<TEntity>>(this);
    }

    public AndConstraint<ZoneAssertion<TEntity>> HaveUniqueCardInstance()
    {
        Guard
            .Require(typeof(TEntity), nameof(TEntity))
            .Is.EqualTo(typeof(ICard));

        using (new AssertionScope())
        {
            this.Subject
                .FindAll()
                .OfType<ICard>()
                .GroupBy(card => card.GetHashCode())
                .Where(grouping => grouping.Count() > 1)
                .ForEach(grouping => Execute
                    .Assertion
                    .FailWith(
                        $"Expected {this.Identifier} to have unique card instance, " +
                        $"but found {grouping.Count()} [{grouping.First().Name}] cards " +
                        $"with ID [{grouping.First().GetHashCode()}]."));
        }

        return new AndConstraint<ZoneAssertion<TEntity>>(this);
    }

    public AndConstraint<ZoneAssertion<TEntity>> HaveQuantity(ushort quantity)
    {
        var actualQuantity = this.Subject.Quantity;

        Execute
            .Assertion
            .ForCondition(actualQuantity == quantity)
            .FailWith(
                $"Expected {this.Identifier} to have {quantity} entities, " +
                $"but found {actualQuantity}.");

        return new AndConstraint<ZoneAssertion<TEntity>>(this);
    }

    public AndConstraint<ZoneAssertion<TEntity>> HaveQuantity(string name, ushort quantity)
    {
        Guard
            .Require(typeof(TEntity), nameof(TEntity))
            .Is.EqualTo(typeof(ICard));

        Guard
            .Require(name, nameof(name))
            .Is.Not.Empty();

        var actualQuantity = this
            .Subject
            .FindAll()
            .Count(card => card.Name == name);

        Execute
            .Assertion
            .ForCondition(actualQuantity == quantity)
            .FailWith(
                $"Expected {this.Identifier} to have {quantity} [{name}] entities, " +
                $"but found {actualQuantity}.");

        return new AndConstraint<ZoneAssertion<TEntity>>(this);
    }
}