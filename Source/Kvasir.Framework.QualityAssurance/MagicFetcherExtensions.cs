// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicFetcherExtensions.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Monday, May 18, 2020 1:49:28 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Framework;

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

        if (matchedCard == null)
        {
            throw new KvasirTestingException($"Card [{cardName}] is missing!");
        }

        return matchedCard;
    }
}