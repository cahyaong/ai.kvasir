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
    using Lucene.Net.Analysis.Standard;
    using Lucene.Net.Documents;
    using Lucene.Net.Index;
    using Lucene.Net.Store;
    using Lucene.Net.Util;
    using nGratis.AI.Kvasir.Contract.Magic;
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

        public StubDirectory WithCardSets(params CardSet[] cardSets)
        {
            Guard
                .Require(cardSets, nameof(cardSets))
                .Is.Not.Null()
                .Is.Not.Empty();

            var analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);

            var configuration = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer)
            {
                OpenMode = OpenMode.CREATE_OR_APPEND
            };

            using (var luceneWriter = new IndexWriter(this, configuration))
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
    }
}