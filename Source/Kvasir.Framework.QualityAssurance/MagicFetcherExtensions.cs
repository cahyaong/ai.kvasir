// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicFetcherExtensions.cs" company="nGratis">
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
// <creation_timestamp>Monday, May 18, 2020 1:49:28 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Framework
{
    using System.Linq;
    using System.Threading.Tasks;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Core;
    using nGratis.Cop.Olympus.Contract;

    public static class MagicFetcherExtensions
    {
        public static async Task<UnparsedBlob.Card> FetchCardAsync(
            this IMagicFetcher fetcher,
            string cardSetName,
            string cardName)
        {
            Guard
                .Require(fetcher, nameof(fetcher))
                .Is.Not.Null();

            Guard
                .Require(cardSetName, nameof(cardSetName))
                .Is.Not.Empty();

            Guard
                .Require(cardName, nameof(cardName))
                .Is.Not.Empty();

            var cardSets = await fetcher.FetchCardSetsAsync();

            var matchedCardSet = cardSets
                .SingleOrDefault(cardSet => cardSet.Name == cardSetName);

            if (matchedCardSet == null)
            {
                throw new KvasirTestingException($"Card set [{cardSetName}] is missing!");
            }

            var cards = await fetcher.FetchCardsAsync(matchedCardSet);

            var matchedCard = cards
                .SingleOrDefault(card => card.Name == cardName);

            if (cardName == null)
            {
                throw new KvasirTestingException($"Card [{cardName}] is missing!");
            }

            return matchedCard;
        }
    }
}