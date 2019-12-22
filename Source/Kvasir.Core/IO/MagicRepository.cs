// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicRepository.cs" company="nGratis">
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
// <creation_timestamp>Thursday, 25 October 2018 10:50:36 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Lucene.Net;
    using Lucene.Net.Index;
    using Lucene.Net.Search;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Core.Contract;
    using nGratis.Cop.Core.Vision.Imaging;

    public class MagicRepository : IMagicRepository
    {
        private readonly IIndexManager _indexManager;

        private readonly IReadOnlyDictionary<ExternalResources, IMagicFetcher> _fetcherLookup;

        public MagicRepository(IIndexManager indexManager, params IMagicFetcher[] fetchers)
        {
            Guard
                .Require(indexManager, nameof(indexManager))
                .Is.Not.Null();

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
                    messageBuilder.Append(" ");
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

            this._fetcherLookup = fetcherGroupings
                .ToDictionary(grouping => grouping.Key, grouping => grouping.Single());
        }

        public event EventHandler RawCardSetIndexed;

        public event EventHandler RawCardIndexed;

        public async Task<int> GetRawCardSetCountAsync()
        {
            var indexReader = this._indexManager.FindIndexReader(IndexKind.CardSet);

            if (indexReader.NumDocs <= 0)
            {
                await this.ReindexCardSetAsync();
                indexReader = this._indexManager.FindIndexReader(IndexKind.CardSet);
            }

            return indexReader.NumDocs;
        }

        public async Task<int> GetRawCardCountAsync()
        {
            return await Task.FromResult(this
                ._indexManager
                .FindIndexReader(IndexKind.Card)
                .NumDocs);
        }

        public async Task<IReadOnlyCollection<RawCardSet>> GetRawCardSetsAsync()
        {
            return await this.GetRawCardSetsAsync(0, await this.GetRawCardSetCountAsync());
        }

        public async Task<IReadOnlyCollection<RawCard>> GetRawCardsAsync(RawCardSet rawCardSet)
        {
            Guard
                .Require(rawCardSet, nameof(rawCardSet))
                .Is.Not.Null();

            var rawCards = default(IReadOnlyCollection<RawCard>);

            if (this._indexManager.HasIndex(IndexKind.Card))
            {
                await Task.Run(() =>
                {
                    var indexReader = this._indexManager.FindIndexReader(IndexKind.Card);
                    var indexSearcher = new IndexSearcher(indexReader);
                    var query = new TermQuery(new Term("card-set-code", rawCardSet.Code));

                    rawCards = indexSearcher
                         .Search(query, int.MaxValue)
                         .ScoreDocs
                         .Select(document => indexReader.Document(document.Doc))
                         .Select(document => document.ToInstance<RawCard>())
                         .ToArray();
                });
            }

            if (rawCards?.Any() != true)
            {
                rawCards = await this
                    ._fetcherLookup[ExternalResources.Card]
                    .FetchRawCardsAsync(rawCardSet);

                Guard
                    .Ensure(rawCards, nameof(rawCards))
                    .Is.Not.Null();

                await Task.Run(() =>
                {
                    var indexWriter = this._indexManager.FindIndexWriter(IndexKind.Card);

                    var documents = rawCards
                        .Select(card => card.ToLuceneDocument())
                        .ToArray();

                    indexWriter.AddDocuments(documents);
                    indexWriter.Commit();
                    indexWriter.Flush(true, true);

                    this.RaiseCardIndexed();
                });
            }

            return rawCards;
        }

        public async Task<IImage> GetCardImageAsync(RawCard rawCard)
        {
            return await this
                ._fetcherLookup[ExternalResources.CardImage]
                .FetchCardImageAsync(rawCard);
        }

        RawCardSet IPagingDataProvider<RawCardSet>.DefaultItem => default;

        async Task<int> IPagingDataProvider<RawCardSet>.GetCountAsync()
        {
            return await this.GetRawCardSetCountAsync();
        }

        async Task<IReadOnlyCollection<RawCardSet>> IPagingDataProvider<RawCardSet>.GetItemsAsync(
            int pagingIndex,
            int itemCount)
        {
            return await this.GetRawCardSetsAsync(pagingIndex, itemCount);
        }

        private async Task<IReadOnlyCollection<RawCardSet>> GetRawCardSetsAsync(int pagingIndex, int itemCount)
        {
            Guard
                .Require(pagingIndex, nameof(pagingIndex))
                .Is.ZeroOrPositive();

            Guard
                .Require(itemCount, nameof(itemCount))
                .Is.Positive();

            var cardSets = default(IReadOnlyCollection<RawCardSet>);

            if (!this._indexManager.HasIndex(IndexKind.CardSet))
            {
                await this.ReindexCardSetAsync();
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
                    .Select(document => document.ToInstance<RawCardSet>())
                    .ToArray();
            });

            return cardSets;
        }

        private async Task ReindexCardSetAsync()
        {
            var indexWriter = this._indexManager.FindIndexWriter(IndexKind.CardSet);

            var cardSets = await this
                ._fetcherLookup[ExternalResources.CardSet]
                .FetchRawCardSetsAsync();

            var documents = cardSets
                .OrderByDescending(cardSet => cardSet.ReleasedTimestamp)
                .Select(cardSet => cardSet.ToLuceneDocument())
                .ToArray();

            indexWriter.AddDocuments(documents);
            indexWriter.Commit();
            indexWriter.Flush(true, true);

            this.RaiseCardSetIndexed();
        }

        private void RaiseCardSetIndexed() => this
            .RawCardSetIndexed?
            .Invoke(this, EventArgs.Empty);

        private void RaiseCardIndexed() => this
            .RawCardIndexed?
            .Invoke(this, EventArgs.Empty);
    }
}