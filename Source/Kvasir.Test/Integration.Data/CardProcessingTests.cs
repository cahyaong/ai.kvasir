// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CardProcessingTests.cs" company="nGratis">
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
// <creation_timestamp>Sunday, May 3, 2020 2:44:14 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Test
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using FluentAssertions.Execution;
    using JetBrains.Annotations;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Core;
    using nGratis.AI.Kvasir.Core.Parser;
    using nGratis.AI.Kvasir.Framework;
    using nGratis.Cop.Olympus.Contract;
    using nGratis.Cop.Olympus.Framework;
    using Xunit;

    public class CardProcessingTests
    {
        [Theory]
        [MemberData(nameof(TestData.ParsingBasicCreatureCardTheories), MemberType = typeof(TestData))]
        public async Task WhenGettingBasicCreatureCard_ShouldParseAndSerializeToKvasirFormatCorrectly(CardTheory theory)
        {
            // Arrange.

            var stubHandler = StubHttpMessageHandler
                .Create()
                .WithSuccessfulScryfallResponse("Raw_SCRYFALL");

            var fetcher = new ScryfallFetcher(stubHandler);
            var unparsedCard = await fetcher.FetchCardAsync(theory.CardSetName, theory.CardName);

            // Act.

            var processingResult = MagicCardProcessor.Instance.Process(unparsedCard);

            // Assert.

            var parsedCard = processingResult.GetValue<DefinedBlob.Card>();

            parsedCard
                .Should().NotBeNull("because we parse card");

            // TODO: Add another assertion for mana cost!

            using (new AssertionScope())
            {
                parsedCard
                    .Name
                    .Should().Be(theory.ExpectedCard.Name, "because card should have name");

                parsedCard
                    .Kind
                    .Should().Be(CardKind.Creature, "because card should have creature kind");

                parsedCard
                    .SuperKind
                    .Should().Be(CardSuperKind.None, "because card should not have any super-kind");

                parsedCard
                    .SubKinds
                    .Should().NotBeNull("because we initialize card sub-kinds")
                    .And.BeEquivalentTo(theory.ExpectedCard.SubKinds, "because card should have sub-kinds");

                parsedCard
                    .Abilities
                    .Should().NotBeNull("because we initialize card abilities")
                    .And.BeEmpty("because card should not have any ability");

                parsedCard
                    .Power
                    .Should().Be(theory.ExpectedCard.Power, "because card should have power");

                parsedCard
                    .Toughness
                    .Should().Be(theory.ExpectedCard.Toughness, "because card should have toughness");
            }
        }

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private static class TestData
        {
            public static IEnumerable<object[]> ParsingBasicCreatureCardTheories
            {
                get
                {
                    yield return CardTheory
                        .Create("Portal", "Foot Soldiers")
                        .ExpectParsedCard(new DefinedBlob.Card
                        {
                            Name = "Foot Soldiers",
                            SubKinds = new[] { CardSubKind.Human, CardSubKind.Soldier },
                            Power = 2,
                            Toughness = 4
                        })
                        .WithLabel(1, "Basic white creature")
                        .ToXunitTheory();

                    yield return CardTheory
                        .Create("Portal", "Giant Octopus")
                        .ExpectParsedCard(new DefinedBlob.Card
                        {
                            Name = "Giant Octopus",
                            SubKinds = new[] { CardSubKind.Octopus },
                            Power = 3,
                            Toughness = 3
                        })
                        .WithLabel(2, "Basic blue creature")
                        .ToXunitTheory();

                    yield return CardTheory
                        .Create("Portal", "Muck Rats")
                        .ExpectParsedCard(new DefinedBlob.Card
                        {
                            Name = "Muck Rats",
                            SubKinds = new[] { CardSubKind.Rat },
                            Power = 1,
                            Toughness = 1
                        })
                        .WithLabel(3, "Basic black creature")
                        .ToXunitTheory();

                    yield return CardTheory
                        .Create("Portal", "Goblin Bully")
                        .ExpectParsedCard(new DefinedBlob.Card
                        {
                            Name = "Goblin Bully",
                            SubKinds = new[] { CardSubKind.Goblin },
                            Power = 2,
                            Toughness = 1
                        })
                        .WithLabel(4, "Basic red creature")
                        .ToXunitTheory();

                    yield return CardTheory
                        .Create("Portal", "Elvish Ranger")
                        .ExpectParsedCard(new DefinedBlob.Card
                        {
                            Name = "Elvish Ranger",
                            SubKinds = new[] { CardSubKind.Elf },
                            Power = 4,
                            Toughness = 1
                        })
                        .WithLabel(5, "Basic green creature")
                        .ToXunitTheory();
                }
            }
        }

        public class CardTheory : CopTheory
        {
            private CardTheory()
            {
            }

            public string CardSetName { get; private set; }

            public string CardName { get; private set; }

            public DefinedBlob.Card ExpectedCard { get; private set; }

            public static CardTheory Create(string cardSetName, string cardName)
            {
                Guard
                    .Require(cardSetName, nameof(cardSetName))
                    .Is.Not.Empty();

                Guard
                    .Require(cardName, nameof(cardName))
                    .Is.Not.Empty();

                return new CardTheory
                {
                    CardSetName = cardSetName,
                    CardName = cardName
                };
            }

            public CardTheory ExpectParsedCard(DefinedBlob.Card definedCard)
            {
                Guard
                    .Require(definedCard, nameof(definedCard))
                    .Is.Not.Null();

                this.ExpectedCard = definedCard;

                return this;
            }
        }
    }
}