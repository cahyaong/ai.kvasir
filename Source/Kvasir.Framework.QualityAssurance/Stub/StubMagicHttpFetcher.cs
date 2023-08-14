// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StubMagicHttpFetcher.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Monday, 17 December 2018 1:28:17 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Framework;

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Core;
using nGratis.Cop.Olympus.Contract;

public class StubMagicHttpFetcher : MagicHttpFetcherBase
{
    private ExternalResources _availableResources;

    private StubMagicHttpFetcher(HttpMessageHandler messageHandler)
        : base(messageHandler)
    {
    }

    public static StubMagicHttpFetcher Create(HttpMessageHandler messageHandler)
    {
        return new(messageHandler);
    }

    public override ExternalResources AvailableResources => this._availableResources;

    public StubMagicHttpFetcher WithAvailableResources(ExternalResources availableResources)
    {
        Guard
            .Require(availableResources, nameof(availableResources))
            .Is.Not.Default();

        this._availableResources |= availableResources;

        return this;
    }

    protected override async Task<IReadOnlyCollection<UnparsedBlob.CardSet>> FetchCardSetsCoreAsync()
    {
        var cardSet = new UnparsedBlob.CardSet
        {
            Code = "[_MOCK_CODE_]",
            Name = "[_MOCK_NAME_]",
            ReleasedTimestamp = Constant.EpochTimestamp
        };

        return await Task.FromResult(new[] { cardSet });
    }

    protected override async Task<IReadOnlyCollection<UnparsedBlob.Card>> FetchCardsCoreAsync(
        UnparsedBlob.CardSet cardSet)
    {
        var card = new UnparsedBlob.Card
        {
            SetCode = cardSet.Code,
            Name = "[_MOCK_NAME_]"
        };

        return await Task.FromResult(new[] { card });
    }

    protected override async Task<IImage> FetchCardImageCoreAsync(UnparsedBlob.Card card)
    {
        return await Task.FromResult(Image.Empty);
    }

    public void VerifyRequestHeader(params string[] expectedHeaders)
    {
        Guard
            .Require(expectedHeaders, nameof(expectedHeaders))
            .Is.Not.Empty();

        var actualHeaders = this
            .HttpClient.DefaultRequestHeaders
            .Select(header => $"{header.Key}: {string.Join(", ", header.Value)}")
            .ToArray();

        var isValid =
            actualHeaders.Length == expectedHeaders.Length &&
            !actualHeaders.Except(expectedHeaders).Any();

        string FormatHeaders(IReadOnlyCollection<string> headers)
        {
            return headers?.Any() == true
                ? string.Join(" AND ", headers.Select(header => $"({header})"))
                : DefinedText.Unknown;
        }

        if (!isValid)
        {
            throw new KvasirTestingException(
                @"Verification failed because <HttpClient> " +
                $"is configured with [{FormatHeaders(actualHeaders)}], " +
                $"but expected to be configured with [{FormatHeaders(expectedHeaders)}]!");
        }
    }
}