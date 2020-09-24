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
    using System.Diagnostics.CodeAnalysis;
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
            this.Identifier = $"card [{card.Name}]";
        }

        protected override string Identifier { get; }

        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        public AndConstraint<CardAssertion> HaveValidContent()
        {
            using (new AssertionScope())
            {
                this.Subject.MultiverseId
                    .Should().BePositive($"{this.Identifier} should have positive Multiverse ID");

                this.Subject.ScryfallId
                    .Should().NotBeNull(
                        $"{this.Identifier} should have non-empty Scryfall ID")
                    .And.MatchRegex(
                        @"[0-9a-f]{8}\-([0-9a-f]{4}\-){3}[0-9a-f]{12}",
                        $"{this.Identifier} should have Scryfall ID matching given pattern");

                this.Subject.ScryfallImageUrl
                    .Should().NotBeNullOrEmpty($"{this.Identifier} should have non-empty image URL");

                this.Subject.CardSetCode
                    .Should().NotBeNullOrEmpty(
                        $"{this.Identifier} should have non-empty card set code")
                    .And.MatchRegex(
                        @"\w{3,6}",
                        $"{this.Identifier} should have card set code with 3-6 alphanumerical values");

                this.Subject.Name
                    .Should().NotBeNullOrEmpty($"{this.Identifier} should have non-empty name");

                this.Subject.ManaCost
                    .Should().NotBeNull(
                        $"{this.Identifier} should have non-null mana cost")
                    .And.MatchRegex(
                        @"(\{[\dWUBRGX/]+\})*",
                        $"{this.Identifier} should have mana cost matching given pattern");

                this.Subject.Type
                    .Should().NotBeNullOrEmpty(
                        $"{this.Identifier} should have non-empty type")
                    .And.MatchRegex(
                        @"[a-zA-Z\-\s]+",
                        $"{this.Identifier} should have type with letter, hyphen or whitespace values");

                this.Subject.Rarity
                    .Should().NotBeNullOrEmpty(
                        $"{this.Identifier} should have non-empty rarity")
                    .And.MatchRegex(
                        @"[a-zA-Z]+",
                        $"{this.Identifier} should have rarity with letter values");

                this.Subject.Text
                    .Should().NotBeNull($"{this.Identifier} should have non-null text");

                this.Subject.FlavorText
                    .Should().NotBeNull($"{this.Identifier} should have non-null flavor text");

                this.Subject.Power
                    .Should().NotBeNull(
                        $"{this.Identifier} should have non-null power")
                    .And.MatchRegex(
                        @"[\d\*]*",
                        $"{this.Identifier} should have power with numerical or asterisk values");

                this.Subject.Toughness
                    .Should().NotBeNull(
                        $"{this.Identifier} should have non-null toughness")
                    .And.MatchRegex(
                        @"[\d\*]*",
                        $"{this.Identifier} should have toughness with numerical or asterisk values");

                this.Subject.Number
                    .Should().NotBeNullOrEmpty(
                        $"{this.Identifier} should have non-empty number")
                    .And.MatchRegex(
                        @"[\da-z]+",
                        $"{this.Identifier} should have number with numerical or letter values");

                this.Subject.Artist
                    .Should().NotBeNullOrEmpty(
                        $"{this.Identifier} should have non-empty artist")
                    .And.MatchRegex(
                        @"[a-zA-Z\s]+",
                        $"{this.Identifier} should have artist with letter or whitespace values");
            }

            return new AndConstraint<CardAssertion>(this);
        }
    }
}