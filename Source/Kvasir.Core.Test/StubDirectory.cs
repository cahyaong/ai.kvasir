// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StubDirectory.cs" company="nGratis">
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
// <creation_timestamp>Wednesday, 7 November 2018 9:20:10 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Test
{
    using System.Collections.Generic;
    using Lucene.Net.Analysis.Standard;
    using Lucene.Net.Documents;
    using Lucene.Net.Index;
    using Lucene.Net.Store;
    using Lucene.Net.Util;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Core.Contract;

    internal class StubDirectory : RAMDirectory
    {
        private StubDirectory()
        {
        }

        public static StubDirectory Create()
        {
            return new StubDirectory();
        }

        public StubDirectory WithUnparsedCardSets(params UnparsedBlob.CardSet[] cardSets)
        {
            Guard
                .Require(cardSets, nameof(cardSets))
                .Is.Not.Empty();

            using (var luceneWriter = new IndexWriter(this, StubDirectory.CreateLuceneConfiguration()))
            {
                foreach (var cardSet in cardSets)
                {
                    var document = new Document();

                    document.AddStringField("code", cardSet.Code, Field.Store.YES);
                    document.AddStringField("name", cardSet.Name, Field.Store.YES);
                    document.AddInt64Field("released-timestamp", cardSet.ReleasedTimestamp.Ticks, Field.Store.YES);

                    luceneWriter.AddDocument(document);
                }

                luceneWriter.Commit();
            }

            return this;
        }

        public StubDirectory WithUnparsedCards(params UnparsedBlob.Card[] cards)
        {
            Guard
                .Require(cards, nameof(cards))
                .Is.Not.Empty();

            using (var luceneWriter = new IndexWriter(this, StubDirectory.CreateLuceneConfiguration()))
            {
                foreach (var card in cards)
                {
                    var document = new Document();

                    document.AddInt32Field("multiverse-id", card.MultiverseId, Field.Store.YES);
                    document.AddStringField("scryfall-id", card.ScryfallId, Field.Store.YES);
                    document.AddStringField("scryfall-image-url", card.ScryfallImageUrl, Field.Store.YES);
                    document.AddStringField("card-set-code", card.CardSetCode, Field.Store.YES);
                    document.AddStringField("name", card.Name, Field.Store.YES);
                    document.AddStringField("mana-cost", card.ManaCost, Field.Store.YES);
                    document.AddStringField("type", card.Type, Field.Store.YES);
                    document.AddStringField("rarity", card.Rarity, Field.Store.YES);
                    document.AddStringField("text", card.Text, Field.Store.YES);
                    document.AddStringField("flavor-text", card.FlavorText, Field.Store.YES);
                    document.AddStringField("power", card.Power, Field.Store.YES);
                    document.AddStringField("toughness", card.Toughness, Field.Store.YES);
                    document.AddStringField("number", card.Number, Field.Store.YES);
                    document.AddStringField("artist", card.Artist, Field.Store.YES);

                    luceneWriter.AddDocument(document);
                }

                luceneWriter.Commit();
            }

            return this;
        }

        public StubDirectory WithDocuments(params IEnumerable<IIndexableField>[] documents)
        {
            Guard
                .Require(documents, nameof(documents))
                .Is.Not.Empty();

            using (var luceneWriter = new IndexWriter(this, StubDirectory.CreateLuceneConfiguration()))
            {
                luceneWriter.AddDocuments(documents);
                luceneWriter.Commit();
            }

            return this;
        }

        private static IndexWriterConfig CreateLuceneConfiguration()
        {
            var analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);

            return new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer)
            {
                OpenMode = OpenMode.CREATE_OR_APPEND
            };
        }
    }
}