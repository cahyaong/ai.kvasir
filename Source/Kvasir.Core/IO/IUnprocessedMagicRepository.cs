// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUnprocessedMagicRepository.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, 25 October 2018 10:48:23 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using nGratis.AI.Kvasir.Contract;

// TODO: Consider implementing paging for <Card> after allowing native sort and filter capabilities?

public interface IUnprocessedMagicRepository
{
    event EventHandler CardSetIndexed;

    event EventHandler CardIndexed;

    Task<int> GetCardSetCountAsync();

    Task<int> GetCardCountAsync();

    Task<int> GetRuleCountAsync();

    Task<IReadOnlyCollection<UnparsedBlob.CardSet>> GetCardSetsAsync();

    Task<IReadOnlyCollection<UnparsedBlob.CardSet>> GetCardSetsAsync(int pagingIndex, int itemCount);

    Task<IReadOnlyCollection<UnparsedBlob.Card>> GetCardsAsync(UnparsedBlob.CardSet cardSet);

    Task<IImage> GetCardImageAsync(UnparsedBlob.Card card);

    Task<IReadOnlyCollection<UnparsedBlob.Rule>> GetRulesAsync();

    Task<IReadOnlyCollection<UnparsedBlob.Rule>> GetRulesAsync(int pagingIndex, int itemCount);
}