// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnprocessedMagicRepository.cs" company="nGratis">
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
// <creation_timestamp>Thursday, 25 October 2018 10:50:36 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Immutable;

namespace nGratis.AI.Kvasir.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net;
using Lucene.Net.Index;
using Lucene.Net.Search;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public class UnprocessedMagicRepository : IUnprocessedMagicRepository
{
    private readonly IIndexManager _indexManager;

    private readonly IReadOnlyDictionary<ExternalResources, IMagicFetcher> _fetcherByExternalResourceLookup;

    public UnprocessedMagicRepository(IIndexManager indexManager, params IMagicFetcher[] fetchers)
    {
        Guard
            .Require(fetchers, nameof(fetchers))
            .Is.Not.Empty();

        this._indexManager = indexManager;

        var externalResources = Enum
            .GetValues(typeof(ExternalResources))
            .Cast<ExternalResources>()
            .Where(resource => resource != ExternalResources.None)
            .Where(resource => resource != ExternalResources.All)
            .ToArray();

        var fetcherGroupings = externalResources
            .SelectMany(resource => fetchers
                .Where(fetcher => fetcher.AvailableResources.HasFlag(resource))
                .Select(fetcher => new { Resource = resource, Fetcher = fetcher }))
            .GroupBy(anon => anon.Resource, anon => anon.Fetcher)
            .ToArray();

        var missingResources = externalResources
            .Except(fetcherGroupings.Select(grouping => grouping.Key))
            .ToArray();

        var duplicatingResources = fetcherGroupings
            .Where(grouping => grouping.Count() > 1)
            .Select(grouping => grouping.Key)
            .ToArray();

        var messageBuilder = new StringBuilder();

        if (missingResources.Any())
        {
            messageBuilder.AppendFormat(
                "Missing Resource(s): {0}.",
                string.Join(", ", missingResources.Select(resource => $"[{resource}]")));

            if (duplicatingResources.Any())
            {
                messageBuilder.Append(' ');
            }
        }

        if (duplicatingResources.Any())
        {
            messageBuilder.AppendFormat(
                "Duplicating Resource(s): {0}.",
                string.Join(", ", duplicatingResources.Select(resource => $"[{resource}]")));
        }

        if (messageBuilder.Length > 0)
        {
            throw new KvasirException($"One or more external resource(s) are invalid! {messageBuilder}");
        }

        this._fetcherByExternalResourceLookup = fetcherGroupings
            .ToDictionary(grouping => grouping.Key, grouping => grouping.Single());
    }

    public event EventHandler? CardSetIndexed;

    public event EventHandler? CardIndexed;

    public async Task<int> GetCardSetCountAsync()
    {
        var indexReader = this._indexManager.FindIndexReader(IndexKind.CardSet);

        if (indexReader.NumDocs <= 0)
        {
            await this.ReindexCardSetsAsync();
            indexReader = this._indexManager.FindIndexReader(IndexKind.CardSet);
        }

        return indexReader.NumDocs;
    }

    public async Task<int> GetCardCountAsync()
    {
        return await Task.FromResult(this
            ._indexManager
            .FindIndexReader(IndexKind.Card)
            .NumDocs);
    }

    public async Task<int> GetRuleCountAsync()
    {
        var indexReader = this._indexManager.FindIndexReader(IndexKind.Rule);

        if (indexReader.NumDocs <= 0)
        {
            await this.ReindexRulesAsync();
            indexReader = this._indexManager.FindIndexReader(IndexKind.Rule);
        }

        return indexReader.NumDocs;
    }

    public async Task<IReadOnlyCollection<UnparsedBlob.CardSet>> GetCardSetsAsync()
    {
        return await this.GetCardSetsAsync(0, await this.GetCardSetCountAsync());
    }

    public async Task<IReadOnlyCollection<UnparsedBlob.CardSet>> GetCardSetsAsync(int pagingIndex, int itemCount)
    {
        Guard
            .Require(pagingIndex, nameof(pagingIndex))
            .Is.ZeroOrPositive();

        Guard
            .Require(itemCount, nameof(itemCount))
            .Is.Positive();

        var cardSets = default(IReadOnlyCollection<UnparsedBlob.CardSet>);

        if (!this._indexManager.HasIndex(IndexKind.CardSet))
        {
            await this.ReindexCardSetsAsync();
        }

        await Task.Run(() =>
        {
            var indexReader = this._indexManager.FindIndexReader(IndexKind.CardSet);
            var itemIndex = pagingIndex * itemCount;

            itemCount = Math.Min(
                indexReader.MaxDoc - itemIndex,
                itemCount);

            cardSets = Enumerable
                .Range(itemIndex, itemCount)
                .Select(index => indexReader.Document(index))
                .Select(document => document.ToInstance<UnparsedBlob.CardSet>())
                .ToImmutableArray();
        });

        return cardSets ?? ImmutableArray.Create<UnparsedBlob.CardSet>();
    }

    public async Task<IReadOnlyCollection<UnparsedBlob.Card>> GetCardsAsync(UnparsedBlob.CardSet cardSet)
    {
        var cards = default(IReadOnlyCollection<UnparsedBlob.Card>);

        if (this._indexManager.HasIndex(IndexKind.Card))
        {
            await Task.Run(() =>
            {
                var indexReader = this._indexManager.FindIndexReader(IndexKind.Card);
                var indexSearcher = new IndexSearcher(indexReader);
                var query = new TermQuery(new Term("set-code", cardSet.Code));

                cards = indexSearcher
                    .Search(query, int.MaxValue)
                    .ScoreDocs
                    .Select(document => indexReader.Document(document.Doc))
                    .Select(document => document.ToInstance<UnparsedBlob.Card>())
                    .ToArray();
            });
        }

        if (cards?.Any() != true)
        {
            cards = await this
                ._fetcherByExternalResourceLookup[ExternalResources.Card]
                .FetchCardsAsync(cardSet);

            await Task.Run(() =>
            {
                var indexWriter = this._indexManager.FindIndexWriter(IndexKind.Card);

                var documents = cards
                    .Select(card => card.ToLuceneDocument())
                    .ToImmutableArray();

                indexWriter.AddDocuments(documents);
                indexWriter.Commit();
                indexWriter.Flush(true, true);

                this.RaiseCardIndexed();
            });
        }

        return cards;
    }

    public async Task<IImage> GetCardImageAsync(UnparsedBlob.Card card)
    {
        return await this
            ._fetcherByExternalResourceLookup[ExternalResources.CardImage]
            .FetchCardImageAsync(card);
    }

    public async Task<IReadOnlyCollection<UnparsedBlob.Rule>> GetRulesAsync()
    {
        return await this.GetRulesAsync(0, await this.GetRuleCountAsync());
    }

    public async Task<IReadOnlyCollection<UnparsedBlob.Rule>> GetRulesAsync(int pagingIndex, int itemCount)
    {
        Guard
            .Require(pagingIndex, nameof(pagingIndex))
            .Is.ZeroOrPositive();

        Guard
            .Require(itemCount, nameof(itemCount))
            .Is.Positive();

        var rules = default(IReadOnlyCollection<UnparsedBlob.Rule>);

        await Task.Run(() =>
        {
            var indexReader = this._indexManager.FindIndexReader(IndexKind.Rule);
            var itemIndex = pagingIndex * itemCount;

            itemCount = Math.Min(
                indexReader.MaxDoc - itemIndex,
                itemCount);

            rules = Enumerable
                .Range(itemIndex, itemCount)
                .Select(index => indexReader.Document(index))
                .Select(document => document.ToInstance<UnparsedBlob.Rule>())
                .ToImmutableArray();
        });

        return rules ?? ImmutableArray.Create<UnparsedBlob.Rule>();
    }

    private async Task ReindexCardSetsAsync()
    {
        var indexWriter = this._indexManager.FindIndexWriter(IndexKind.CardSet);

        var cardSets = await this
            ._fetcherByExternalResourceLookup[ExternalResources.CardSet]
            .FetchCardSetsAsync();

        var documents = cardSets
            .OrderByDescending(cardSet => cardSet.ReleasedTimestamp)
            .Select(cardSet => cardSet.ToLuceneDocument())
            .ToArray();

        indexWriter.AddDocuments(documents);
        indexWriter.Commit();
        indexWriter.Flush(true, true);

        this.RaiseCardSetIndexed();
    }

    private async Task ReindexRulesAsync()
    {
        var indexWriter = this._indexManager.FindIndexWriter(IndexKind.Rule);

        var rules = await this
            ._fetcherByExternalResourceLookup[ExternalResources.Rule]
            .FetchRulesAsync();

        var documents = rules
            .OrderBy(rule => rule.Id)
            .Select(rule => rule.ToLuceneDocument())
            .ToArray();

        indexWriter.AddDocuments(documents);
        indexWriter.Commit();
        indexWriter.Flush(true, true);
    }

    private void RaiseCardSetIndexed() => this
        .CardSetIndexed?
        .Invoke(this, EventArgs.Empty);

    private void RaiseCardIndexed() => this
        .CardIndexed?
        .Invoke(this, EventArgs.Empty);
}