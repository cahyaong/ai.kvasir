// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KvasirAssertions.Card.cs" company="nGratis">
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
// <creation_timestamp>Thursday, September 24, 2020 1:49:09 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Test
{
    using FluentAssertions;
    using FluentAssertions.Execution;
    using FluentAssertions.Primitives;
    using nGratis.AI.Kvasir.Contract;

    public class CardAssertion : ReferenceTypeAssertions<UnparsedBlob.Card, CardAssertion>
    {
        public CardAssertion(UnparsedBlob.Card card)
        {
            card
                .Should().NotBeNull();

            this.Subject = card;
        }

        protected override string Identifier { get; } = "card";

        public AndConstraint<CardAssertion> HaveValidContent()
        {
            using (new AssertionScope())
            {
                this.Subject.MultiverseId
                    .Should().BePositive();

                this.Subject.ScryfallId
                    .Should().NotBeNull()
                    .And.MatchRegex(@"[0-9a-f]{8}\-([0-9a-f]{4}\-){3}[0-9a-f]{12}");

                this.Subject.ScryfallImageUrl
                    .Should().NotBeNullOrEmpty();

                this.Subject.CardSetCode
                    .Should().NotBeNullOrEmpty()
                    .And.MatchRegex(@"\w{3,6}");

                this.Subject.Name
                    .Should().NotBeNullOrWhiteSpace();

                this.Subject.ManaCost
                    .Should().NotBeNull()
                    .And.MatchRegex(@"(\{[\dWUBRGX/]+\})*");

                this.Subject.Type
                    .Should().NotBeNullOrEmpty()
                    .And.MatchRegex(@"[a-zA-Z\-\s]+");

                this.Subject.Rarity
                    .Should().NotBeNullOrEmpty()
                    .And.MatchRegex(@"[a-zA-Z]+");

                this.Subject.Text
                    .Should().NotBeNull();

                this.Subject.FlavorText
                    .Should().NotBeNull();

                this.Subject.Power
                    .Should().NotBeNull()
                    .And.MatchRegex(@"[\d\*]*");

                this.Subject.Toughness
                    .Should().NotBeNull()
                    .And.MatchRegex(@"[\d\*]*");

                this.Subject.Number
                    .Should().NotBeNull()
                    .And.MatchRegex(@"[\da-z]+");

                this.Subject.Artist
                    .Should().NotBeNullOrEmpty()
                    .And.MatchRegex(@"[a-zA-Z\s]+");
            }

            return new AndConstraint<CardAssertion>(this);
        }
    }
}