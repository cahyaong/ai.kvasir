// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KvasirAssertions.Library.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2020 Cahya Ong
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

namespace nGratis.AI.Kvasir.Engine.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using FluentAssertions.Execution;
    using FluentAssertions.Primitives;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Core.Contract;

    internal class ZoneAssertions : ReferenceTypeAssertions<Zone, ZoneAssertions>
    {
        public ZoneAssertions(Zone zone)
        {
            Guard
                .Require(zone, nameof(zone))
                .Is.Not.Null();

            // TODO: Extend <Guard> implementation to support property checking fluently!

            Guard
                .Require(zone.Kind, $"{nameof(zone)}.{nameof(zone.Kind)}")
                .Is.Not.Default();

            this.Subject = zone;
        }

        protected override string Identifier { get; } = "zone";

        public AndConstraint<ZoneAssertions> BeLibrary()
        {
            this
                .Subject.Kind
                .Should().Be(ZoneKind.Library, $"{this.Identifier} should be library");

            return new AndConstraint<ZoneAssertions>(this);
        }

        public AndConstraint<ZoneAssertions> BeHand()
        {
            this
                .Subject.Kind
                .Should().Be(ZoneKind.Hand, $"{this.Identifier} should be hand");

            return new AndConstraint<ZoneAssertions>(this);
        }

        public AndConstraint<ZoneAssertions> MatchDefinedDeck(DefinedBlob.Deck definedDeck)
        {
            Guard
                .Require(definedDeck, nameof(definedDeck))
                .Is.Not.Null();

            Guard
                .Require(definedDeck.CardNames, $"{nameof(definedDeck)}.{nameof(definedDeck.CardNames)}")
                .Is.Not.Null()
                .Is.Not.Empty();

            this
                .Subject
                .Must().HaveCardQuantity(definedDeck.CardQuantity);

            var actualCardNames = this
                .Subject.Cards?
                .Select(card => card.Name) ?? Enumerable.Empty<string>();

            actualCardNames
                .Distinct()
                .Should().BeEquivalentTo(definedDeck.CardNames, $"{this.Identifier} should have card names defined by deck");

            using (new AssertionScope())
            {
                definedDeck
                    .CardNames
                    .ForEach(cardName => this
                        .Subject
                        .Must().HaveCardQuantity(cardName, definedDeck[cardName]));
            }

            return new AndConstraint<ZoneAssertions>(this);
        }

        public AndConstraint<ZoneAssertions> BeSubsetOfDefinedDeck(DefinedBlob.Deck definedDeck)
        {
            Guard
                .Require(definedDeck, nameof(definedDeck))
                .Is.Not.Null();

            Guard
                .Require(definedDeck.CardNames, $"{nameof(definedDeck)}.{nameof(definedDeck.CardNames)}")
                .Is.Not.Null()
                .Is.Not.Empty();

            using (new AssertionScope())
            {
                this
                    .Subject.Cards?
                    .Select(card => card.Name)
                    .Distinct()
                    .Where(cardName => !definedDeck.CardNames.Contains(cardName))
                    .ForEach(cardName => Execute
                        .Assertion
                        .FailWith($"Expected {{context:zone}} to not have card [{cardName}], not defined by deck."));
            }

            return new AndConstraint<ZoneAssertions>(this);
        }

        public AndConstraint<ZoneAssertions> HaveUniqueCardInstance()
        {
            using (new AssertionScope())
            {
                this
                    .Subject.Cards?
                    .GroupBy(card => card.GetHashCode())
                    .Where(grouping => grouping.Count() > 1)
                    .ForEach(grouping => Execute
                        .Assertion
                        .FailWith(
                            $"Expected {{context:zone}} to have unique card instance, " +
                            $"but found {grouping.Count()} [{grouping.First().Name}] cards " +
                            $"with ID [{grouping.First().GetHashCode()}]."));
            }

            return new AndConstraint<ZoneAssertions>(this);
        }

        public AndConstraint<ZoneAssertions> HaveCardQuantity(ushort expectedQuantity)
        {
            var actualQuantity = this
                .Subject?.Cards?
                .Count() ?? 0;

            Execute
                .Assertion
                .ForCondition(actualQuantity == expectedQuantity)
                .FailWith(
                    $"Expected {{context:zone}} to have {expectedQuantity} cards, " +
                    $"but found {actualQuantity}.");

            return new AndConstraint<ZoneAssertions>(this);
        }

        public AndConstraint<ZoneAssertions> HaveCardQuantity(string cardName, ushort expectedQuantity)
        {
            Guard
                .Require(cardName, nameof(cardName))
                .Is.Not.Empty();

            var actualQuantity = this
                .Subject?.Cards?
                .Count(card => card.Name == cardName) ?? 0;

            Execute
                .Assertion
                .ForCondition(actualQuantity == expectedQuantity)
                .FailWith(
                    $"Expected {{context:zone}} to have {expectedQuantity} [{cardName}] cards, " +
                    $"but found {actualQuantity}.");

            return new AndConstraint<ZoneAssertions>(this);
        }
    }
}