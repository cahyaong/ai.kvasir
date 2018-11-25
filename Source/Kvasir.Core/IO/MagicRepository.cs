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

        private IReadOnlyCollection<CardSet> _cardSets;

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
            this._cardSets = new CardSet[0];
        }

        public async Task<IReadOnlyCollection<CardSet>> GetCardSetsAsync()
        {
            if (this._cardSets.Any())
            {
                return this._cardSets;
            }

            if (this._indexManager.HasIndex(IndexKind.CardSet))
            {
                await Task.Run(() =>
                {
                    var indexReader = this._indexManager.FindIndexReader(IndexKind.CardSet);

                    this._cardSets = Enumerable
                        .Range(0, indexReader.NumDocs)
                        // ReSharper disable once AccessToDisposedClosure
                        .Select(index => indexReader.Document(index))
                        .Select(document => document.ToInstance<CardSet>())
                        .ToArray();
                });
            }
            else
            {
                this._cardSets = await this._magicFetcher.GetCardSetsAsync();

                await Task.Run(() =>
                {
                    var indexWriter = this._indexManager.FindIndexWriter(IndexKind.CardSet);

                    this._cardSets
                        .Select(cardSet => cardSet.ToDocument())
                        .ToList()
                        // ReSharper disable once AccessToDisposedClosure
                        .ForEach(indexWriter.AddDocument);

                    indexWriter.Commit();
                    indexWriter.Flush(true, true);
                });
            }

            return this._cardSets;
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
                         // ReSharper disable once AccessToDisposedClosure
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
                });
            }

            return cards;
        }
    }
}