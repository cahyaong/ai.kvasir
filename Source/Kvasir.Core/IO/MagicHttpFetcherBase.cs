// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseMagicHttpFetcher.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Monday, 17 December 2018 8:53:20 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public abstract class MagicHttpFetcherBase : IMagicFetcher
{
    private readonly HttpMessageHandler _messageHandler;

    private bool _isDisposed;

    protected MagicHttpFetcherBase(string id, IStorageManager storageManager, IKeyCalculator keyCalculator)
        : this(MagicHttpFetcherBase.CreateMessageHandler(id, storageManager, keyCalculator))
    {
    }

    protected internal MagicHttpFetcherBase(HttpMessageHandler messageHandler)
    {
        this._messageHandler = messageHandler;
        this.HttpClient = new HttpClient(messageHandler);

        if (!Debugger.IsAttached)
        {
            this.HttpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        this.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        this.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*", 0.8));
        this.HttpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        this.HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AI.Kvasir", "0.1"));
    }

    public abstract ExternalResources AvailableResources { get; }

    // TODO: Consider handling <TaskCanceledException> when application is closing while HTTP client is
    // in a middle of making request!

    protected HttpClient HttpClient { get; }

    public async Task<IReadOnlyCollection<UnparsedBlob.CardSet>> FetchCardSetsAsync()
    {
        if (!this.AvailableResources.HasFlag(ExternalResources.CardSet))
        {
            return await Task.FromException<IReadOnlyCollection<UnparsedBlob.CardSet>>(new NotSupportedException());
        }

        return await this.FetchCardSetsCoreAsync();
    }

    public async Task<IReadOnlyCollection<UnparsedBlob.Card>> FetchCardsAsync(UnparsedBlob.CardSet cardSet)
    {
        if (!this.AvailableResources.HasFlag(ExternalResources.Card))
        {
            return await Task.FromException<IReadOnlyCollection<UnparsedBlob.Card>>(new NotSupportedException());
        }

        return await this.FetchCardsCoreAsync(cardSet);
    }

    public async Task<IImage> FetchCardImageAsync(UnparsedBlob.Card card)
    {
        if (!this.AvailableResources.HasFlag(ExternalResources.CardImage))
        {
            return await Task.FromException<IImage>(new NotSupportedException());
        }

        return await this.FetchCardImageCoreAsync(card);
    }

    public async Task<IReadOnlyCollection<UnparsedBlob.Rule>> FetchRulesAsync()
    {
        if (!this.AvailableResources.HasFlag(ExternalResources.Rule))
        {
            return await Task.FromException<IReadOnlyCollection<UnparsedBlob.Rule>>(new NotSupportedException());
        }

        return await this.FetchRulesCoreAsync();
    }

    protected virtual async Task<IReadOnlyCollection<UnparsedBlob.CardSet>> FetchCardSetsCoreAsync()
    {
        return await Task.FromException<IReadOnlyCollection<UnparsedBlob.CardSet>>(new NotSupportedException());
    }

    protected virtual async Task<IReadOnlyCollection<UnparsedBlob.Card>> FetchCardsCoreAsync(UnparsedBlob.CardSet _)
    {
        return await Task.FromException<IReadOnlyCollection<UnparsedBlob.Card>>(new NotSupportedException());
    }

    protected virtual async Task<IImage> FetchCardImageCoreAsync(UnparsedBlob.Card _)
    {
        return await Task.FromException<IImage>(new NotSupportedException());
    }

    protected virtual async Task<IReadOnlyCollection<UnparsedBlob.Rule>> FetchRulesCoreAsync()
    {
        return await Task.FromException<IReadOnlyCollection<UnparsedBlob.Rule>>(new NotSupportedException());
    }

    protected virtual string CreateUniqueKey(Uri uri)
    {
        return Regex.Replace(uri.Segments.Last(), @"\s+", "_");
    }

    private static HttpMessageHandler CreateMessageHandler(
        string id,
        IStorageManager storageManager,
        IKeyCalculator keyCalculator)
    {
        Guard
            .Require(id, nameof(id))
            .Is.Not.Empty();

        var messageHandler = (HttpMessageHandler)new HttpClientHandler();

        messageHandler = new ThrottlingMessageHandler(TimeSpan.FromMilliseconds(25), messageHandler);
        messageHandler = new CachingMessageHandler($"Raw_{id}", storageManager, keyCalculator, messageHandler);

        return messageHandler;
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool isDisposing)
    {
        if (this._isDisposed)
        {
            return;
        }

        if (isDisposing)
        {
            this._messageHandler.Dispose();
            this.HttpClient.Dispose();
        }

        this._isDisposed = true;
    }
}