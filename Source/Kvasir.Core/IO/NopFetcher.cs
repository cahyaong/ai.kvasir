// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NopFetcher.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, April 9, 2020 6:36:02 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using nGratis.AI.Kvasir.Contract;

public class NopFetcher : IMagicFetcher
{
    public ExternalResources AvailableResources => ExternalResources.All;

    public Task<IReadOnlyCollection<UnparsedBlob.CardSet>> FetchCardSetsAsync()
    {
        throw new NotSupportedException("Fetching card sets is not allowed!");
    }

    public Task<IReadOnlyCollection<UnparsedBlob.Card>> FetchCardsAsync(UnparsedBlob.CardSet cardSet)
    {
        throw new NotSupportedException("Fetching cards is not allowed!");
    }

    public Task<IImage> FetchCardImageAsync(UnparsedBlob.Card card)
    {
        throw new NotSupportedException("Fetching card image is not allowed!");
    }

    public Task<IReadOnlyCollection<UnparsedBlob.Rule>> FetchRulesAsync()
    {
        throw new NotSupportedException("Fetching rules is not allowed!");
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}