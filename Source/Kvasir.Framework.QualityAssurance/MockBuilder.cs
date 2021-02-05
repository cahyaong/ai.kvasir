// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MockBuilder.cs" company="nGratis">
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
// <creation_timestamp>Monday, 5 November 2018 8:08:49 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace Moq.AI.Kvasir
{
    using System;
    using System.Linq;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Olympus.Contract;

    public class MockBuilder : Moq.MockBuilder
    {
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
                    CardSetCode = cardSetCode,
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
}