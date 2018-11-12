// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MockExtensions.cs" company="nGratis">
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
// <creation_timestamp>Monday, 5 November 2018 8:02:55 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Test
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Lucene.Net.Analysis.Core;
    using Lucene.Net.Index;
    using Lucene.Net.Store;
    using Lucene.Net.Util;
    using Moq;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Contract.Magic;
    using nGratis.Cop.Core.Contract;

    public static class MockExtensions
    {
        public static Mock<IIndexManager> WithDefaultIndexWriter(
            this Mock<IIndexManager> mockManager,
            IndexKind indexKind)
        {
            Guard
                .Require(mockManager, nameof(mockManager))
                .Is.Not.Null();

            Guard
                .Require(indexKind, nameof(indexKind))
                .Is.Not.Default();

            var writerConfiguration = new IndexWriterConfig(
                LuceneVersion.LUCENE_48,
                new SimpleAnalyzer(LuceneVersion.LUCENE_48));

            var mockWriter = MockBuilder
                .CreateMock<IndexWriter>(new RAMDirectory(), writerConfiguration);

            mockManager
                .Setup(mock => mock.CreateIndexWriter(indexKind))
                .Returns(mockWriter.Object)
                .Verifiable();

            return mockManager;
        }

        public static Mock<IIndexManager> WithDefaultIndexReader(
            this Mock<IIndexManager> mockManager,
            IndexKind indexKind)
        {
            Guard
                .Require(mockManager, nameof(mockManager))
                .Is.Not.Null();

            Guard
                .Require(indexKind, nameof(indexKind))
                .Is.Not.Default();

            var mockReader = MockBuilder
                .CreateMock<AtomicReader>();

            mockManager
                .Setup(mock => mock.CreateIndexReader(indexKind))
                .Returns(mockReader.Object)
                .Verifiable();

            return mockManager;
        }

        public static Mock<IIndexManager> WithExistingCardSets(
            this Mock<IIndexManager> mockManager,
            params CardSet[] cardSets)
        {
            Guard
                .Require(mockManager, nameof(mockManager))
                .Is.Not.Null();

            mockManager
                .Setup(mock => mock.HasIndex(IndexKind.CardSet))
                .Returns(true)
                .Verifiable();

            var stubDirectory = StubDirectory
                .Create()
                .WithCardSets(cardSets);

            mockManager
                .Setup(mock => mock.CreateIndexReader(IndexKind.CardSet))
                .Returns(DirectoryReader.Open(stubDirectory))
                .Verifiable();

            return mockManager;
        }

        public static Mock<IMagicFetcher> WithCardSets(this Mock<IMagicFetcher> mockFetcher, params CardSet[] cardSets)
        {
            Guard
                .Require(mockFetcher, nameof(mockFetcher))
                .Is.Not.Null();

            Guard
                .Require(cardSets, nameof(cardSets))
                .Is.Not.Null()
                .Is.Not.Empty();

            mockFetcher
                .Setup(mock => mock.GetCardSetsAsync())
                .Returns(Task.FromResult<IReadOnlyCollection<CardSet>>(cardSets))
                .Verifiable();

            return mockFetcher;
        }

        public static Mock<IMagicFetcher> WithoutCardSets(this Mock<IMagicFetcher> mockFetcher)
        {
            Guard
                .Require(mockFetcher, nameof(mockFetcher))
                .Is.Not.Null();

            mockFetcher
                .Setup(mock => mock.GetCardSetsAsync())
                .Returns(Task.FromResult<IReadOnlyCollection<CardSet>>(new CardSet[0]))
                .Verifiable();

            return mockFetcher;
        }
    }
}