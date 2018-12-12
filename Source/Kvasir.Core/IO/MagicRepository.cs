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
    using System.Threading.Tasks;
    using Lucene.Net;
    using Lucene.Net.Index;
    using Lucene.Net.Search;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Contract.Magic;
    using nGratis.Cop.Core.Contract;

    public class MagicRepository : IMagicRepository
    {
        private readonly IIndexManager _indexManager;

        private readonly IMagicFetcher _magicFetcher;

        public MagicRepository(IIndexManager indexManager, IMagicFetcher magicFetcher)
        {
            Guard
                .Require(indexManager, nameof(indexManager))
                .Is.Not.Null();

            Guard
                .Require(magicFetcher, nameof(magicFetcher))
                .Is.Not.Null();

            this._indexManager = indexManager;
            this._magicFetcher = magicFetcher;
        }

        public event EventHandler CardSetIndexed;

        public event EventHandler CardIndexed;

        public async Task<int> GetCardSetCountAsync()
        {
            var indexReader = this._indexManager.FindIndexReader(IndexKind.CardSet);

            if (indexReader.NumDocs <= 0)
            {
                await this.RebuildCardSetAsync();
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

        public async Task<IReadOnlyCollection<CardSet>> GetCardSetsAsync()
        {
            return await this.GetCardSetsAsync(0, await this.GetCardSetCountAsync());
        }

        public async Task<IReadOnlyCollection<Card>> GetCardsAsync(CardSet cardSet)
        {
            Guard
                .Require(cardSet, nameof(cardSet))
                .Is.Not.Null();

            var cards = default(IReadOnlyCollection<Card>);

            if (this._indexManager.HasIndex(IndexKind.Card))
            {
                await Task.Run(() =>
                {
                    var indexReader = this._indexManager.FindIndexReader(IndexKind.Card);
                    var indexSearcher = new IndexSearcher(indexReader);
                    var query = new TermQuery(new Term("card-set-code", cardSet.Code));

                    cards = indexSearcher
                         .Search(query, 1000)
                         .ScoreDocs
                         .Select(document => indexReader.Document(document.Doc))
                         .Select(document => document.ToInstance<Card>())
                         .ToArray();
                });
            }

            if (cards?.Any() != true)
            {
                cards = await this._magicFetcher.GetCardsAsync(cardSet);

                Guard
                    .Ensure(cards, nameof(cards))
                    .Is.Not.Null();

                // ReSharper disable once ImplicitlyCapturedClosure

                await Task.Run(() =>
                {
                    var indexWriter = this._indexManager.FindIndexWriter(IndexKind.Card);

                    var documents = cards
                        .Select(card => card.ToDocument())
                        .ToArray();

                    indexWriter.AddDocuments(documents);
                    indexWriter.Commit();
                    indexWriter.Flush(true, true);

                    this.RaiseCardIndexed();
                });
            }

            return cards;
        }

        CardSet IPagingDataProvider<CardSet>.DefaultItem => default(CardSet);

        async Task<int> IPagingDataProvider<CardSet>.GetCountAsync()
        {
            return await this.GetCardSetCountAsync();
        }

        async Task<IReadOnlyCollection<CardSet>> IPagingDataProvider<CardSet>.GetItemsAsync(
            int pagingIndex,
            int itemCount)
        {
            return await this.GetCardSetsAsync(pagingIndex, itemCount);
        }

        private async Task<IReadOnlyCollection<CardSet>> GetCardSetsAsync(int pagingIndex, int itemCount)
        {
            Guard
                .Require(pagingIndex, nameof(pagingIndex))
                .Is.ZeroOrPositive();

            Guard
                .Require(itemCount, nameof(itemCount))
                .Is.Positive();

            var cardSets = default(IReadOnlyCollection<CardSet>);

            if (!this._indexManager.HasIndex(IndexKind.CardSet))
            {
                await this.RebuildCardSetAsync();
            }

            await Task.Run(() =>
            {
                var indexReader = this._indexManager.FindIndexReader(IndexKind.CardSet);

                itemCount = Math.Min(
                    indexReader.MaxDoc - pagingIndex * itemCount,
                    itemCount);

                cardSets = Enumerable
                    .Range(pagingIndex * itemCount, itemCount)
                    .Select(index => indexReader.Document(index))
                    .Select(document => document.ToInstance<CardSet>())
                    .ToArray();
            });

            return cardSets;
        }

        private async Task RebuildCardSetAsync()
        {
            var indexWriter = this._indexManager.FindIndexWriter(IndexKind.CardSet);
            var cardSets = await this._magicFetcher.GetCardSetsAsync();

            var documents = cardSets
                .Select(cardSet => cardSet.ToDocument())
                .ToArray();

            indexWriter.AddDocuments(documents);
            indexWriter.Commit();
            indexWriter.Flush(true, true);

            this.RaiseCardSetIndexed();
        }

        private void RaiseCardSetIndexed() => this
            .CardSetIndexed?
            .Invoke(this, EventArgs.Empty);

        private void RaiseCardIndexed() => this
            .CardIndexed?
            .Invoke(this, EventArgs.Empty);
    }
}