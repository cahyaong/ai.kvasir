// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CardProcessingTests.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Sunday, May 3, 2020 2:44:14 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.IntegrationTest;

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

using StubHttpMessageHandler = AI.Kvasir.Framework.StubHttpMessageHandler;

public class CardProcessingTests
{
    [Theory]
    [MemberData(nameof(TestData.ParsingBasicCreatureCardTheories), MemberType = typeof(TestData))]
    public async Task WhenGettingBasicCreatureCard_ShouldParseAndSerializeToKvasirFormatCorrectly(
        ParsingCardTheory theory)
    {
        // Arrange.

        var stubHandler = StubHttpMessageHandler
            .Create()
            .WithSuccessfulScryfallResponseInSession("Raw_SCRYFALL");

        var fetcher = new ScryfallFetcher(stubHandler);
        var cardProcessor = new MagicCardProcessor();

        var unparsedCard = await fetcher.FetchCardAsync(theory.CardSetName, theory.CardName);

        // Act.

        var processingResult = cardProcessor.Process(unparsedCard);

        // Assert.

        var parsedCard = processingResult.GetValue<DefinedBlob.Card>();

        parsedCard
            .Should().NotBeNull("because we can parse card");

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
                yield return ParsingCardTheory
                    .Create("Portal", "Foot Soldiers")
                    .Expect(new DefinedBlob.Card
                    {
                        Name = "Foot Soldiers",
                        SubKinds = new[] { CardSubKind.Human, CardSubKind.Soldier },
                        Power = 2,
                        Toughness = 4
                    })
                    .WithLabel(1, "Parsing basic white creature card")
                    .ToXunitTheory();

                yield return ParsingCardTheory
                    .Create("Portal", "Giant Octopus")
                    .Expect(new DefinedBlob.Card
                    {
                        Name = "Giant Octopus",
                        SubKinds = new[] { CardSubKind.Octopus },
                        Power = 3,
                        Toughness = 3
                    })
                    .WithLabel(2, "Parsing basic blue creature card")
                    .ToXunitTheory();

                yield return ParsingCardTheory
                    .Create("Portal", "Muck Rats")
                    .Expect(new DefinedBlob.Card
                    {
                        Name = "Muck Rats",
                        SubKinds = new[] { CardSubKind.Rat },
                        Power = 1,
                        Toughness = 1
                    })
                    .WithLabel(3, "Parsing basic black creature card")
                    .ToXunitTheory();

                yield return ParsingCardTheory
                    .Create("Portal", "Goblin Bully")
                    .Expect(new DefinedBlob.Card
                    {
                        Name = "Goblin Bully",
                        SubKinds = new[] { CardSubKind.Goblin },
                        Power = 2,
                        Toughness = 1
                    })
                    .WithLabel(4, "Parsing basic red creature card")
                    .ToXunitTheory();

                yield return ParsingCardTheory
                    .Create("Portal", "Elvish Ranger")
                    .Expect(new DefinedBlob.Card
                    {
                        Name = "Elvish Ranger",
                        SubKinds = new[] { CardSubKind.Elf },
                        Power = 4,
                        Toughness = 1
                    })
                    .WithLabel(5, "Parsing basic green creature card")
                    .ToXunitTheory();
            }
        }
    }

    public class ParsingCardTheory : OlympusTheory
    {
        private ParsingCardTheory()
        {
        }

        public string CardSetName { get; private init; }

        public string CardName { get; private init; }

        public DefinedBlob.Card ExpectedCard { get; private set; }

        public static ParsingCardTheory Create(string cardSetName, string cardName)
        {
            Guard
                .Require(cardSetName, nameof(cardSetName))
                .Is.Not.Empty();

            Guard
                .Require(cardName, nameof(cardName))
                .Is.Not.Empty();

            return new ParsingCardTheory
            {
                CardSetName = cardSetName,
                CardName = cardName
            };
        }

        public ParsingCardTheory Expect(DefinedBlob.Card card)
        {
            Guard
                .Require(card, nameof(card))
                .Is.Not.Null();

            this.ExpectedCard = card;

            return this;
        }
    }
}