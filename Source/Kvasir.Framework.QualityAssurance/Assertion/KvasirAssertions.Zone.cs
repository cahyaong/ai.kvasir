// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KvasirAssertions.Zone.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Wednesday, 30 January 2019 11:57:02 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Framework;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using JetBrains.Annotations;
using nGratis.AI.Kvasir.Contract;
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
                        $"because {this.Identifier} [{Subject.Kind}] should contain card defined by deck");
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
                .Select(grouping => new
                {
                    Id = grouping.First().GetHashCode(),
                    grouping.First().Name,
                    Count = grouping.Count(),
                    Unit = "card".ToCorrectPluralization(grouping.Count()),
                })
                .ForEach(anon => Execute
                    .Assertion
                    .FailWith(
                        $"Expected {this.Identifier} [{Subject.Kind}] to have unique card instance, " +
                        $"but found {anon.Count} [{anon.Name}] {anon.Unit} " +
                        $"with ID [{anon.Id}]."));
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
                $"Expected {this.Identifier} [{Subject.Kind}] " +
                $"to have {quantity} {"entity".ToCorrectPluralization(quantity)}, " +
                $"but found {actualQuantity}.");

        return new AndConstraint<ZoneAssertion<TEntity>>(this);
    }

    public AndConstraint<ZoneAssertion<TEntity>> HaveQuantity(string name, ushort quantity)
    {
        Guard
            .Require(typeof(TEntity), nameof(TEntity))
            .Is.AssignableTo(typeof(IDiagnostic));

        Guard
            .Require(name, nameof(name))
            .Is.Not.Empty();

        var actualQuantity = this
            .Subject
            .FindAll()
            .OfType<IDiagnostic>()
            .Count(diagnostic => diagnostic.Name == name);

        Execute
            .Assertion
            .ForCondition(actualQuantity == quantity)
            .FailWith(
                $"Expected {this.Identifier} [{Subject.Kind}] " +
                $"to have {quantity} [{name}] {"entity".ToCorrectPluralization(quantity)}, " +
                $"but found {actualQuantity}.");

        return new AndConstraint<ZoneAssertion<TEntity>>(this);
    }

    public AndConstraint<ZoneAssertion<TEntity>> Contain(params string[] names)
    {
        Guard
            .Require(typeof(TEntity), nameof(TEntity))
            .Is.AssignableTo(typeof(IDiagnostic));

        Guard
            .Require(names, nameof(names))
            .Is.Not.Empty();

        var actualNames = this
            .Subject
            .FindAll()
            .OfType<IDiagnostic>()
            .Select(diagnostic => diagnostic.Name)
            .ToImmutableList();

        Execute
            .Assertion
            .ForCondition(names.All(name => actualNames.Contains(name)))
            .FailWith(
                $"Expected {this.Identifier} [{Subject.Kind}] to contain ({names.ToPrettifiedText()}), " +
                $"but found ({actualNames.ToPrettifiedText()})");

        return new AndConstraint<ZoneAssertion<TEntity>>(this);
    }

    public AndConstraint<ZoneAssertion<TEntity>> NotContain(params string[] names)
    {
        Guard
            .Require(typeof(TEntity), nameof(TEntity))
            .Is.AssignableTo(typeof(IDiagnostic));

        Guard
            .Require(names, nameof(names))
            .Is.Not.Empty();

        var actualNames = this
            .Subject
            .FindAll()
            .OfType<IDiagnostic>()
            .Select(diagnostic => diagnostic.Name)
            .ToImmutableList();

        Execute
            .Assertion
            .ForCondition(names.All(name => !actualNames.Contains(name)))
            .FailWith(
                $"Expected {this.Identifier} [{Subject.Kind}] to not contain ({names.ToPrettifiedText()}), " +
                $"but found ({actualNames.ToPrettifiedText()})");

        return new AndConstraint<ZoneAssertion<TEntity>>(this);
    }

    public AndConstraint<ZoneAssertion<TEntity>> ContainMatching([RegexPattern] string pattern)
    {
        Guard
            .Require(typeof(TEntity), nameof(TEntity))
            .Is.AssignableTo(typeof(IDiagnostic));

        Guard
            .Require(pattern, nameof(pattern))
            .Is.Not.Empty();

        var actualNames = this
            .Subject
            .FindAll()
            .OfType<IDiagnostic>()
            .Select(diagnostic => diagnostic.Name)
            .ToImmutableList();

        var regex = new Regex(pattern);

        Execute
            .Assertion
            .ForCondition(actualNames.All(name => regex.IsMatch(name)))
            .FailWith(
                $"Expected {this.Identifier} [{Subject.Kind}] to contain pattern [{pattern}], " +
                $"but found ({actualNames.ToPrettifiedText()})");

        return new AndConstraint<ZoneAssertion<TEntity>>(this);
    }
}