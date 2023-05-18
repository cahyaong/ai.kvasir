// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMagicFetcher.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, 25 October 2018 10:49:23 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using nGratis.AI.Kvasir.Contract;

public interface IMagicFetcher : IDisposable
{
    ExternalResources AvailableResources { get; }

    Task<IReadOnlyCollection<UnparsedBlob.CardSet>> FetchCardSetsAsync();

    Task<IReadOnlyCollection<UnparsedBlob.Card>> FetchCardsAsync(UnparsedBlob.CardSet cardSet);

    Task<IImage> FetchCardImageAsync(UnparsedBlob.Card card);

    Task<IReadOnlyCollection<UnparsedBlob.Rule>> FetchRulesAsync();
}