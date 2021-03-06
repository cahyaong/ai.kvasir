﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MockExtensions.cs" company="nGratis">
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
// <creation_timestamp>Monday, 5 November 2018 8:02:55 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace Moq.AI.Kvasir
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Lucene.Net.Analysis.Core;
    using Lucene.Net.Index;
    using Lucene.Net.Store;
    using Lucene.Net.Util;
    using Moq;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Core;
    using nGratis.AI.Kvasir.Core.Test;
    using nGratis.Cop.Olympus.Contract;

    internal static partial class MockExtensions
    {
        public static Mock<IIndexManager> WithDefault(this Mock<IIndexManager> mockManager, IndexKind indexKind)
        {
            Guard
                .Require(mockManager, nameof(mockManager))
                .Is.Not.Null();

            return mockManager
                .WithDefaultIndexReader(indexKind)
                .WithDefaultIndexWriter(indexKind);
        }

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

            mockWriter
                .Setup(mock => mock.AddDocuments(It.IsAny<IEnumerable<IEnumerable<IIndexableField>>>()))
                .Callback<IEnumerable<IEnumerable<IIndexableField>>>(documents =>
                {
                    mockManager
                        .Setup(mock => mock.HasIndex(indexKind))
                        .Returns(true)
                        .Verifiable();

                    var stubDirectory = StubDirectory
                        .Create()
                        .WithDocuments(documents.ToArray());

                    mockManager
                        .Setup(mock => mock.FindIndexReader(indexKind))
                        .Returns(DirectoryReader.Open(stubDirectory))
                        .Verifiable();
                });

            mockManager
                .Setup(mock => mock.FindIndexWriter(indexKind))
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
                .Setup(mock => mock.FindIndexReader(indexKind))
                .Returns(mockReader.Object)
                .Verifiable();

            return mockManager;
        }

        public static Mock<IIndexManager> WithExistingCardSets(
            this Mock<IIndexManager> mockManager,
            params UnparsedBlob.CardSet[] cardSets)
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
                .WithUnparsedCardSets(cardSets);

            mockManager
                .Setup(mock => mock.FindIndexReader(IndexKind.CardSet))
                .Returns(DirectoryReader.Open(stubDirectory))
                .Verifiable();

            return mockManager;
        }

        public static Mock<IIndexManager> WithExistingCards(
            this Mock<IIndexManager> mockManager,
            params UnparsedBlob.Card[] cards)
        {
            Guard
                .Require(mockManager, nameof(mockManager))
                .Is.Not.Null();

            mockManager
                .Setup(mock => mock.HasIndex(IndexKind.Card))
                .Returns(true)
                .Verifiable();

            var stubDirectory = StubDirectory
                .Create()
                .WithUnparsedCards(cards);

            mockManager
                .Setup(mock => mock.FindIndexReader(IndexKind.Card))
                .Returns(DirectoryReader.Open(stubDirectory))
                .Verifiable();

            return mockManager;
        }

        public static Mock<IMagicFetcher> WithAvailableResources(
            this Mock<IMagicFetcher> mockFetcher,
            ExternalResources availableResources)
        {
            Guard
                .Require(mockFetcher, nameof(mockFetcher))
                .Is.Not.Null();

            mockFetcher
                .Setup(mock => mock.AvailableResources)
                .Returns(availableResources)
                .Verifiable();

            return mockFetcher;
        }

        public static Mock<IMagicFetcher> WithCardSets(
            this Mock<IMagicFetcher> mockFetcher,
            params UnparsedBlob.CardSet[] cardSets)
        {
            Guard
                .Require(mockFetcher, nameof(mockFetcher))
                .Is.Not.Null();

            Guard
                .Require(cardSets, nameof(cardSets))
                .Is.Not.Empty();

            mockFetcher
                .Setup(mock => mock.FetchCardSetsAsync())
                .Returns(Task.FromResult<IReadOnlyCollection<UnparsedBlob.CardSet>>(cardSets))
                .Verifiable();

            return mockFetcher;
        }

        public static Mock<IMagicFetcher> WithoutCardSets(this Mock<IMagicFetcher> mockFetcher)
        {
            Guard
                .Require(mockFetcher, nameof(mockFetcher))
                .Is.Not.Null();

            mockFetcher
                .Setup(mock => mock.FetchCardSetsAsync())
                .Returns(Task.FromResult<IReadOnlyCollection<UnparsedBlob.CardSet>>(new UnparsedBlob.CardSet[0]))
                .Verifiable();

            return mockFetcher;
        }

        public static Mock<IMagicFetcher> WithCards(this Mock<IMagicFetcher> mockFetcher, params UnparsedBlob.Card[] cards)
        {
            Guard
                .Require(mockFetcher, nameof(mockFetcher))
                .Is.Not.Null();

            Guard
                .Require(cards, nameof(cards))
                .Is.Not.Empty();

            cards
                .GroupBy(card => card.SetCode)
                .Select(grouping => new
                {
                    CardSetCode = grouping.Key,
                    Cards = grouping.ToArray()
                })
                .ForEach(anon =>
                {
                    mockFetcher
                        .Setup(mock => mock.FetchCardsAsync(Arg.UnparsedCardSet.Is(anon.CardSetCode)))
                        .Returns(Task.FromResult<IReadOnlyCollection<UnparsedBlob.Card>>(anon.Cards))
                        .Verifiable();
                });

            return mockFetcher;
        }

        public static Mock<IMagicFetcher> WithoutCards(this Mock<IMagicFetcher> mockFetcher)
        {
            Guard
                .Require(mockFetcher, nameof(mockFetcher))
                .Is.Not.Null();

            mockFetcher
                .Setup(mock => mock.FetchCardsAsync(It.IsAny<UnparsedBlob.CardSet>()))
                .Returns(Task.FromResult<IReadOnlyCollection<UnparsedBlob.Card>>(new UnparsedBlob.Card[0]))
                .Verifiable();

            return mockFetcher;
        }

        public static Mock<IKeyCalculator> WithMapping(
            this Mock<IKeyCalculator> mockCalculator,
            string url,
            string key)
        {
            Guard
                .Require(mockCalculator, nameof(mockCalculator))
                .Is.Not.Null();

            Guard
                .Require(url, nameof(url))
                .Is.Not.Empty();

            Guard
                .Require(key, nameof(key))
                .Is.Not.Empty();

            var name = Path.GetFileNameWithoutExtension(key);
            var extension = Path.GetExtension(key);

            var mime = !string.IsNullOrEmpty(extension)
                ? Mime.ParseByExtension(extension)
                : Mime.Text;

            mockCalculator
                .Setup(mock => mock.Calculate(It.Is<Uri>(uri => uri.ToString() == url)))
                .Returns(new DataSpec(name, mime))
                .Verifiable();

            return mockCalculator;
        }
    }
}