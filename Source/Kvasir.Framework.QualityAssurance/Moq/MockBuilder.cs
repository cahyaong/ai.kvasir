// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MockBuilder.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Monday, 5 November 2018 8:08:49 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Framework;

using System;
using System.Linq;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public class MockBuilder : Cop.Olympus.Framework.MockBuilder
{
    private MockBuilder()
    {
    }

    public static UnparsedBlob.CardSet[] CreateUnparsedCardSets(ushort count)
    {
        return Enumerable
            .Range(1, count)
            .Select(index => new UnparsedBlob.CardSet
            {
                Code = $"[_MOCK_CODE_{index:D2}_]",
                Name = $"[_MOCK_NAME_{index:D2}_]",
                ReleasedTimestamp = new DateTime(2000, 1, 1)
            })
            .ToArray();
    }

    public static UnparsedBlob.Card[] CreateUnparsedCards(string cardSetCode, ushort count)
    {
        Guard
            .Require(cardSetCode, nameof(cardSetCode))
            .Is.Not.Empty();

        return Enumerable
            .Range(1, count)
            .Select(index => new UnparsedBlob.Card
            {
                MultiverseId = index,
                ScryfallId = "[_MOCK_SCRYFALL_ID_]",
                ScryfallImageUrl = "[_MOCK_SCRYFALL_IMAGE_URL_]",
                SetCode = cardSetCode,
                Name = $"[_MOCK_CODE_{index:D2}_]",
                ManaCost = "[_MOCK_MANA_COST_]",
                Type = "[_MOCK_TYPE_]",
                Rarity = "[_MOCK_RARITY_]",
                Text = "[_MOCK_TEXT_]",
                FlavorText = "[_MOCK_FLAVOR_TEXT_]",
                Power = "[_MOCK_POWER_]",
                Toughness = "[_MOCK_TOUGHNESS_]",
                Number = index.ToString(),
                Artist = "[_MOCK_ARTIST_]"
            })
            .ToArray();
    }

    public static DefinedBlob.Deck CreateDefinedElfDeck()
    {
        return DefinedBlob.Deck.Builder
            .Create()
            .WithCode("[_MOCK_CODE_ELF_]")
            .WithName("[_MOCK_NAME_ELF_]")
            .WithCardAndQuantity("Elvish Eulogist", "LRW", 3, 4)
            .WithCardAndQuantity("Elvish Warrior", "9ED", 240, 4)
            .WithCardAndQuantity("Llanowar Elves", "LEA", 210, 4)
            .WithCardAndQuantity("Forest", "LEA", 294, 6)
            .Build();
    }

    public static DefinedBlob.Deck CreateDefinedGoblinDeck()
    {
        return DefinedBlob.Deck.Builder
            .Create()
            .WithCode("[_MOCK_CODE_GOBLIN_]")
            .WithName("[_MOCK_NAME_GOBLIN_]")
            .WithCardAndQuantity("Goblin Cohort", "BOK", 106, 4)
            .WithCardAndQuantity("Goblin Sledder", "BOK", 209, 4)
            .WithCardAndQuantity("Goblin Warchief", "SCG", 97, 4)
            .WithCardAndQuantity("Mountain", "LEA", 292, 6)
            .Build();
    }
}