// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MockBuilder.cs" company="nGratis">
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
// <creation_timestamp>Monday, 5 November 2018 8:08:49 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Test
{
    using System;
    using System.Linq;
    using nGratis.AI.Kvasir.Contract.Magic;
    using nGratis.Cop.Core.Contract;

    internal class MockBuilder : Moq.MockBuilder
    {
        public static CardSet[] CreateCardSets(ushort count)
        {
            return Enumerable
                .Range(1, count)
                .Select(index => new CardSet
                {
                    Code = $"[_MOCK_CODE_{index:D2}_]",
                    Name = $"[_MOCK_NAME_{index:D2}_]",
                    ReleasedTimestamp = new DateTime(2000, 1, 1)
                })
                .ToArray();
        }

        public static Card[] CreteCards(string cardSetCode, ushort count)
        {
            Guard
                .Require(cardSetCode, nameof(cardSetCode))
                .Is.Not.Empty();

            return Enumerable
                .Range(1, count)
                .Select(index => new Card
                {
                    MultiverseId = index,
                    CardSetCode = cardSetCode,
                    Name = $"[_MOCK_CODE_{index:D2}_]",
                    ManaCost = "[_MOCK_MANA_COST_]",
                    Type = "[_MOCK_TYPE_]",
                    Rarity = "[_MOCK_RARITY_]",
                    Text = "[_MOCK_TEXT_]",
                    FlavorText = "[_MOCK_FLAVOR_TEXT_]",
                    Power = "[_MOCK_POWER_]",
                    Toughness = "[_MOCK_TOUGHNESS_]",
                    Number = (short)index,
                    Artist = "[_MOCK_ARTIST_]"
                })
                .ToArray();
        }
    }
}