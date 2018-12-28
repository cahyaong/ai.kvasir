// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicParserTests.cs" company="nGratis">
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
// <creation_timestamp>Wednesday, 26 December 2018 8:53:54 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using JetBrains.Annotations;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Core.Contract;
    using nGratis.Cop.Core.Testing;
    using Xunit;

    public class MagicParserTests
    {
        public class ParseRawCardMethod
        {
            [Theory]
            [MemberData(nameof(TestData.ValidCardKindTheories), MemberType = typeof(TestData))]
            public void WhenGettingValidCardType_ShouldParseItAsCardKind(CardKindTheory theory)
            {
                // Arrange.

                var rawCard = new RawCard
                {
                    Type = theory.Type
                };

                var magicParser = new MagicParser();

                // Act.

                var cardInfo = magicParser.ParseRawCard(rawCard);

                // Assert.

                cardInfo
                    .Should().NotBeNull();

                cardInfo
                    .Value
                    .Should().NotBeNull();

                cardInfo
                    .Value.Kind
                    .Should().Be(theory.Kind);

                cardInfo
                    .Value.SuperKind
                    .Should().Be(theory.SuperKind);

                cardInfo
                    .Value.SubKinds
                    .Should().BeEquivalentTo(theory.SubKinds);
            }

            [Theory]
            [MemberData(nameof(TestData.InvalidCardKindTheories), MemberType = typeof(TestData))]
            public void WhenGettingInvalidCardType_ShouldGenerateErrorMessage(CardKindTheory theory)
            {
                // Arrange.

                var rawCard = new RawCard
                {
                    Type = theory.Type
                };

                var magicParser = new MagicParser();

                // Act.

                var cardInfo = magicParser.ParseRawCard(rawCard);

                // Assert.

                cardInfo
                    .Should().NotBeNull();

                cardInfo
                    .Messages
                    .Should().NotBeNull()
                    .And.Contain(theory.Message);
            }

            [UsedImplicitly(ImplicitUseTargetFlags.Members)]
            private static class TestData
            {
                public static IEnumerable<object[]> ValidCardKindTheories
                {
                    get
                    {
                        yield return CardKindTheory
                            .Create("Artifact")
                            .ExpectValid(
                                CardKind.Artifact,
                                CardSuperKind.None)
                            .WithLabel("CASE 01 -> Card type without sub-type.")
                            .ToXunitTheory();

                        yield return CardKindTheory
                            .Create("Creature - Elf Warrior")
                            .ExpectValid(
                                CardKind.Creature,
                                CardSuperKind.None,
                                CardSubKind.Elf,
                                CardSubKind.Warrior)
                            .WithLabel("CASE 02 -> Card type with multiple sub-types.")
                            .ToXunitTheory();

                        yield return CardKindTheory
                            .Create("Legendary Creature - Goblin Shaman")
                            .ExpectValid(
                                CardKind.Creature,
                                CardSuperKind.Legendary,
                                CardSubKind.Goblin,
                                CardSubKind.Shaman)
                            .WithLabel("CASE 03 -> Card type with super-type and multiple sub-types.")
                            .ToXunitTheory();
                    }
                }

                public static IEnumerable<object[]> InvalidCardKindTheories
                {
                    get
                    {
                        yield return CardKindTheory
                            .Create(string.Empty)
                            .ExpectInvalid("<Kind> No matching pattern for value [].")
                            .WithLabel("CASE 01 -> Empty type.")
                            .ToXunitTheory();

                        yield return CardKindTheory
                            .Create("Food")
                            .ExpectInvalid("<Kind> No mapping for value [Food].")
                            .WithLabel("CASE 02 -> Invalid card type.")
                            .ToXunitTheory();

                        yield return CardKindTheory
                            .Create("Extraordinary Creature")
                            .ExpectInvalid("<SuperKind> No mapping for value [Extraordinary].")
                            .WithLabel("CASE 03 -> Invalid card super-type.")
                            .ToXunitTheory();

                        yield return CardKindTheory
                            .Create("Creature - Quokka Ranger")
                            .ExpectInvalid("<SubKind> No mapping for value [Quokka], [Ranger].")
                            .WithLabel("CASE 04 -> Invalid card sub-types.")
                            .ToXunitTheory();
                    }
                }
            }
        }

        public class CardKindTheory : CopTheory
        {
            public string Type { get; private set; }

            public CardKind Kind { get; private set; }

            public CardSuperKind SuperKind { get; private set; }

            public IEnumerable<CardSubKind> SubKinds { get; private set; }

            public string Message { get; private set; }

            public static CardKindTheory Create(string type)
            {
                return new CardKindTheory
                {
                    Type = type,
                    Kind = CardKind.Unknown,
                    SuperKind = CardSuperKind.Unknown,
                    SubKinds = Enumerable.Empty<CardSubKind>(),
                    Message = string.Empty
                };
            }

            public CardKindTheory ExpectValid(CardKind kind, CardSuperKind superKind, params CardSubKind[] subKinds)
            {
                Guard
                    .Require(kind, nameof(kind))
                    .Is.Not.EqualTo(CardKind.Unknown);

                Guard
                    .Require(superKind, nameof(superKind))
                    .Is.Not.EqualTo(CardSuperKind.Unknown);

                // TODO: Extend <Guard> class to support collection check against specific value(s).

                var hasInvalidSubKind = subKinds
                    .Any(subKind => subKind == CardSubKind.Unknown);

                Guard
                    .Require(hasInvalidSubKind, nameof(hasInvalidSubKind))
                    .Is.False();

                this.Kind = kind;
                this.SuperKind = superKind;
                this.SubKinds = subKinds;

                return this;
            }

            public CardKindTheory ExpectInvalid(string message)
            {
                Guard
                    .Require(message, nameof(message))
                    .Is.Not.Empty();

                this.Message = message;

                return this;
            }
        }
    }
}