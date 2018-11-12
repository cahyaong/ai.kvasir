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
    using Lucene.Net.Documents;
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
                    using (var indexReader = this._indexManager.CreateIndexReader(IndexKind.CardSet))
                    {
                        this._cardSets = Enumerable
                            .Range(0, indexReader.NumDocs)

                            // ReSharper disable once AccessToDisposedClosure
                            .Select(index => indexReader.Document(index))
                            .Select(document => new CardSet
                            {
                                Code = document
                                    .GetField(Index.Field.Code)
                                    .GetStringValue() ?? Text.Empty,
                                Name = document
                                    .GetField(Index.Field.Name)
                                    .GetStringValue() ?? Text.Empty,
                                ReleasedTimestamp = new DateTime(document
                                    .GetField(Index.Field.ReleasedTimestamp)
                                    .GetInt64Value() ?? 0, DateTimeKind.Utc)
                            })
                            .ToArray();
                    }
                });
            }
            else
            {
                this._cardSets = await this._magicFetcher.GetCardSetsAsync();

                await Task.Run(() =>
                {
                    using (var indexWriter = this._indexManager.CreateIndexWriter(IndexKind.CardSet))
                    {
                        foreach (var cardSet in this._cardSets)
                        {
                            var document = new Document();

                            document.AddStringField(Index.Field.Code, cardSet.Code, Field.Store.YES);
                            document.AddStringField(Index.Field.Name, cardSet.Name, Field.Store.YES);

                            document.AddInt64Field(
                                Index.Field.ReleasedTimestamp,
                                cardSet.ReleasedTimestamp.Ticks,
                                Field.Store.YES);

                            indexWriter.AddDocument(document);
                        }

                        indexWriter.Commit();
                    }
                });
            }

            return this._cardSets;
        }

        private static class Index
        {
            public static class Field
            {
                public const string Code = "code";

                public const string Name = "name";

                public const string ReleasedTimestamp = "released-timestamp";
            }
        }
    }
}