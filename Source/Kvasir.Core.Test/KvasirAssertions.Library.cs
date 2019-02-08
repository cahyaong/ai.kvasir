// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KvasirAssertions.Library.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2018 Cahya Ong
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

namespace nGratis.AI.Kvasir.Core.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using FluentAssertions.Execution;
    using FluentAssertions.Primitives;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Core.Contract;

    internal class LibraryAssertions : ReferenceTypeAssertions<Library, LibraryAssertions>
    {
        public LibraryAssertions(Library library)
        {
            this.Subject = library;
        }

        protected override string Identifier { get; } = "library";

        public AndConstraint<LibraryAssertions> MatchDeckDefinition(DeckDefinition deckDefinition)
        {
            Guard
                .Require(deckDefinition, nameof(deckDefinition))
                .Is.Not.Null();

            this
                .Subject
                .Must().HaveCardQuantity(deckDefinition.CardQuantity);

            var actualCardNames = this
                .Subject?.Cards?
                .Select(card => card.Name) ?? Enumerable.Empty<string>();

            actualCardNames
                .Distinct()
                .Should().BeEquivalentTo(deckDefinition.CardNames, "library should have card names defined by deck");

            using (new AssertionScope())
            {
                deckDefinition
                    .CardNames?
                    .ForEach(cardName => this
                        .Subject
                        .Must().HaveCardQuantity(cardName, deckDefinition[cardName]));
            }

            return new AndConstraint<LibraryAssertions>(this);
        }

        public AndConstraint<LibraryAssertions> HaveCardQuantity(ushort expectedQuantity)
        {
            var actualQuantity = this
                .Subject?.Cards?
                .Count() ?? 0;

            Execute
                .Assertion
                .ForCondition(actualQuantity == expectedQuantity)
                .FailWith(
                    $"Expected {{context:library}} to have {expectedQuantity} cards, " +
                    $"but found {actualQuantity}.");

            return new AndConstraint<LibraryAssertions>(this);
        }

        public AndConstraint<LibraryAssertions> HaveCardQuantity(string cardName, ushort expectedQuantity)
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
                    $"Expected {{context:library}} to have {expectedQuantity} [{cardName}] cards, " +
                    $"but found {actualQuantity}.");

            return new AndConstraint<LibraryAssertions>(this);
        }
    }
}