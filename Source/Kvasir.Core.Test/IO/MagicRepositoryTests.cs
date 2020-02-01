// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicRepositoryTests.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2020 Cahya Ong
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
// <creation_timestamp>Monday, 5 November 2018 7:58:24 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Moq.AI.Kvasir;
    using nGratis.AI.Kvasir.Contract;
    using Xunit;
    using Arg = Moq.AI.Kvasir.Arg;
    using MockBuilder = Moq.AI.Kvasir.MockBuilder;

    public class MagicRepositoryTests
    {
        public class Constructor
        {
            [Fact]
            public void WhenGettingFetchersWithEachAvailableResource_ShouldNotThrowException()
            {
                // Arrange.

                var mockIndexManager = MockBuilder
                    .CreateMock<IIndexManager>();

                var mockFetchers = new[]
                {
                    MockBuilder
                        .CreateMock<IMagicFetcher>()
                        .WithAvailableResources(ExternalResources.CardSet | ExternalResources.Card),
                    MockBuilder
                        .CreateMock<IMagicFetcher>()
                        .WithAvailableResources(ExternalResources.CardImage),
                    MockBuilder
                        .CreateMock<IMagicFetcher>()
                        .WithAvailableResources(ExternalResources.Rule)
                };

                // Act.

                var action = new Action(() =>
                {
                    var _ = new MagicRepository(mockIndexManager.Object, mockFetchers.ToObjects());
                });

                // Assert.

                action
                    .Should().NotThrow();
            }

            [Fact]
            public void WhenGettingFetcherWithoutAvailableResource_ShouldThrowKvasirException()
            {
                // Arrange.

                var mockIndexManager = MockBuilder
                    .CreateMock<IIndexManager>();

                var mockFetcher = MockBuilder
                    .CreateMock<IMagicFetcher>()
                    .WithAvailableResources(ExternalResources.None);

                // Act.

                var action = new Action(() =>
                {
                    var _ = new MagicRepository(mockIndexManager.Object, mockFetcher.Object);
                });

                // Assert.

                action
                    .Should().Throw<KvasirException>()
                    .WithMessage(
                        "One or more external resource(s) are invalid! " +
                        "Missing Resource(s): [CardSet], [Card], [CardImage], [Rule].");
            }

            [Fact]
            public void WhenGettingFetchersWithDuplicatingAvailableResources_ShouldThrowKvasirException()
            {
                // Arrange.

                var mockIndexManager = MockBuilder
                    .CreateMock<IIndexManager>();

                var mockFetchers = new[]
                {
                    MockBuilder
                        .CreateMock<IMagicFetcher>()
                        .WithAvailableResources(ExternalResources.All),
                    MockBuilder
                        .CreateMock<IMagicFetcher>()
                        .WithAvailableResources(ExternalResources.All)
                };

                // Act.

                var action = new Action(() =>
                {
                    var _ = new MagicRepository(mockIndexManager.Object, mockFetchers.ToObjects());
                });

                // Assert.

                action
                    .Should().Throw<KvasirException>()
                    .WithMessage(
                        "One or more external resource(s) are invalid! " +
                        "Duplicating Resource(s): [CardSet], [Card], [CardImage], [Rule].");
            }

            [Fact]
            public void WhenGettingFetchersWithMissingAndDuplicatingAvailableResources_ShouldThrowKvasirException()
            {
                // Arrange.

                var mockIndexManager = MockBuilder
                    .CreateMock<IIndexManager>();

                var mockFetchers = new[]
                {
                    MockBuilder
                        .CreateMock<IMagicFetcher>()
                        .WithAvailableResources(ExternalResources.CardSet | ExternalResources.Card),
                    MockBuilder
                        .CreateMock<IMagicFetcher>()
                        .WithAvailableResources(ExternalResources.Card)
                };

                // Act.

                var action = new Action(() =>
                {
                    var _ = new MagicRepository(mockIndexManager.Object, mockFetchers.ToObjects());
                });

                // Assert.

                action
                    .Should().Throw<KvasirException>()
                    .WithMessage(
                        "One or more external resource(s) are invalid! " +
                        "Missing Resource(s): [CardImage], [Rule]. " +
                        "Duplicating Resource(s): [Card].");
            }
        }

        public class GetCardSetsAsyncMethod
        {
            [Fact]
            public async Task WhenGettingEmptyIndex_ShouldPopulateItFromFetcher()
            {
                // Arrange.

                var mockIndexManager = MockBuilder
                    .CreateMock<IIndexManager>()
                    .WithDefault(IndexKind.CardSet);

                var mockFetcher = MockBuilder
                    .CreateMock<IMagicFetcher>()
                    .WithAvailableResources(ExternalResources.All)
                    .WithCardSets(MockBuilder.CreateUnparsedCardSets(3));

                var repository = new MagicRepository(mockIndexManager.Object, mockFetcher.Object);

                // Act.

                var cardSets = await repository.GetCardSetsAsync();

                // Assert.

                cardSets
                    .Should().NotBeNullOrEmpty()
                    .And.HaveCount(3)
                    .And.Contain(set =>
                        !string.IsNullOrEmpty(set.Code) &&
                        !string.IsNullOrEmpty(set.Name) &&
                        set.ReleasedTimestamp > DateTime.MinValue);

                mockIndexManager.Verify(
                    mock => mock.FindIndexReader(IndexKind.CardSet),
                    Times.AtLeastOnce);

                mockIndexManager.Verify(
                    mock => mock.FindIndexWriter(IndexKind.CardSet),
                    Times.Once);

                mockFetcher.Verify(
                    mock => mock.FetchCardSetsAsync(),
                    Times.Once);
            }

            [Fact]
            public async Task WhenGettingNotEmptyIndex_ShouldPopulateItFromIndex()
            {
                // Arrange.

                var mockIndexManager = MockBuilder
                    .CreateMock<IIndexManager>()
                    .WithDefault(IndexKind.CardSet)
                    .WithExistingCardSets(MockBuilder.CreateUnparsedCardSets(3));

                var mockFetcher = MockBuilder
                    .CreateMock<IMagicFetcher>()
                    .WithAvailableResources(ExternalResources.All)
                    .WithoutCardSets();

                var repository = new MagicRepository(mockIndexManager.Object, mockFetcher.Object);

                // Act.

                var cardSets = await repository.GetCardSetsAsync();

                // Assert.

                cardSets
                    .Should().NotBeNullOrEmpty()
                    .And.HaveCount(3)
                    .And.NotContainNulls();

                foreach (var cardSet in cardSets)
                {
                    cardSet
                        .Code
                        .Should().NotBeEmpty();

                    cardSet
                        .Name
                        .Should().NotBeNullOrEmpty();

                    cardSet
                        .ReleasedTimestamp
                        .Should().BeOnOrAfter(Constant.EpochTimestamp);
                }

                mockIndexManager.Verify(
                    mock => mock.FindIndexReader(IndexKind.CardSet),
                    Times.AtLeastOnce);

                mockIndexManager.Verify(
                    mock => mock.FindIndexWriter(IndexKind.CardSet),
                    Times.Never);

                mockFetcher.Verify(
                    mock => mock.FetchCardSetsAsync(),
                    Times.Never);
            }
        }

        public class GetCardsAsyncMethod
        {
            [Fact]
            public async Task WhenGettingEmptyIndex_ShouldPopulateItFromFetcher()
            {
                // Arrange.

                var mockIndexManager = MockBuilder
                    .CreateMock<IIndexManager>()
                    .WithDefault(IndexKind.Card);

                var mockFetcher = MockBuilder
                    .CreateMock<IMagicFetcher>()
                    .WithAvailableResources(ExternalResources.All)
                    .WithCards(Enumerable
                        .Empty<UnparsedBlob.Card>()
                        .AppendItems(MockBuilder.CreateUnparsedCards("X02", 2))
                        .AppendItems(MockBuilder.CreateUnparsedCards("X03", 3))
                        .AppendItems(MockBuilder.CreateUnparsedCards("X05", 5))
                        .ToArray());

                var repository = new MagicRepository(mockIndexManager.Object, mockFetcher.Object);

                var cardSet = new UnparsedBlob.CardSet
                {
                    Code = "X03",
                    Name = "[_MOCK_NAME_]",
                    ReleasedTimestamp = Constant.EpochTimestamp
                };

                // Act.

                var cards = await repository.GetCardsAsync(cardSet);

                // Assert.

                cards
                    .Should().NotBeEmpty()
                    .And.HaveCount(3)
                    .And.NotContainNulls();

                foreach (var card in cards)
                {
                    card
                        .MultiverseId
                        .Should().BePositive();

                    card
                        .CardSetCode
                        .Should().BeEquivalentTo("X03");

                    card
                        .Number
                        .Should().NotBeNullOrEmpty();
                }

                mockIndexManager.Verify(
                    mock => mock.FindIndexReader(IndexKind.Card),
                    Times.Never);

                mockIndexManager.Verify(
                    mock => mock.FindIndexWriter(IndexKind.Card),
                    Times.Once);

                mockFetcher.Verify(
                    mock => mock.FetchCardsAsync(It.IsAny<UnparsedBlob.CardSet>()),
                    Times.Once);

                mockFetcher.Verify(
                    mock => mock.FetchCardsAsync(Arg.UnparsedCardSet.Is("X03")),
                    Times.Once);
            }

            [Fact]
            public async Task WhenGettingNotEmptyIndex_ShouldPopulateItFromIndex()
            {
                // Arrange.

                var mockIndexManager = MockBuilder
                    .CreateMock<IIndexManager>()
                    .WithDefault(IndexKind.Card)
                    .WithExistingCards(Enumerable
                        .Empty<UnparsedBlob.Card>()
                        .AppendItems(MockBuilder.CreateUnparsedCards("X02", 2))
                        .AppendItems(MockBuilder.CreateUnparsedCards("X03", 3))
                        .AppendItems(MockBuilder.CreateUnparsedCards("X05", 5))
                        .ToArray());

                var mockFetcher = MockBuilder
                    .CreateMock<IMagicFetcher>()
                    .WithAvailableResources(ExternalResources.All)
                    .WithoutCards();

                var repository = new MagicRepository(mockIndexManager.Object, mockFetcher.Object);

                var cardSet = new UnparsedBlob.CardSet
                {
                    Code = "X03",
                    Name = "[_MOCK_NAME_]",
                    ReleasedTimestamp = Constant.EpochTimestamp
                };

                // Act.

                var cards = await repository.GetCardsAsync(cardSet);

                // Assert.

                cards
                    .Should().NotBeEmpty()
                    .And.HaveCount(3)
                    .And.NotContainNulls();

                foreach (var card in cards)
                {
                    card
                        .MultiverseId
                        .Should().BePositive();

                    card
                        .CardSetCode
                        .Should().BeEquivalentTo("X03");

                    card
                        .Number
                        .Should().NotBeNullOrEmpty();
                }

                mockIndexManager.Verify(
                    mock => mock.FindIndexReader(IndexKind.Card),
                    Times.Once);

                mockIndexManager.Verify(
                    mock => mock.FindIndexWriter(IndexKind.Card),
                    Times.Never);

                mockFetcher.Verify(
                    mock => mock.FetchCardsAsync(It.IsAny<UnparsedBlob.CardSet>()),
                    Times.Never);
            }

            [Fact]
            public async Task WhenGettingNotEmptyIndexButMissingCards_ShouldPopulateItFromFetcher()
            {
                // Arrange.

                var mockIndexManager = MockBuilder
                    .CreateMock<IIndexManager>()
                    .WithDefault(IndexKind.Card)
                    .WithExistingCards(Enumerable
                        .Empty<UnparsedBlob.Card>()
                        .AppendItems(MockBuilder.CreateUnparsedCards("X02", 2))
                        .AppendItems(MockBuilder.CreateUnparsedCards("X05", 5))
                        .ToArray());

                var mockFetcher = MockBuilder
                    .CreateMock<IMagicFetcher>()
                    .WithAvailableResources(ExternalResources.All)
                    .WithCards(MockBuilder.CreateUnparsedCards("X03", 3));

                var repository = new MagicRepository(mockIndexManager.Object, mockFetcher.Object);

                var cardSet = new UnparsedBlob.CardSet
                {
                    Code = "X03",
                    Name = "[_MOCK_NAME_]",
                    ReleasedTimestamp = Constant.EpochTimestamp
                };

                // Act.

                var cards = await repository.GetCardsAsync(cardSet);

                // Assert.

                cards
                    .Should().NotBeEmpty()
                    .And.HaveCount(3)
                    .And.NotContainNulls();

                foreach (var card in cards)
                {
                    card
                        .MultiverseId
                        .Should().BePositive();

                    card
                        .CardSetCode
                        .Should().BeEquivalentTo("X03");

                    card
                        .Number
                        .Should().NotBeNullOrEmpty();
                }

                mockIndexManager.Verify(
                    mock => mock.FindIndexReader(IndexKind.Card),
                    Times.Once);

                mockIndexManager.Verify(
                    mock => mock.FindIndexWriter(IndexKind.Card),
                    Times.Once);

                mockFetcher.Verify(
                    mock => mock.FetchCardsAsync(It.IsAny<UnparsedBlob.CardSet>()),
                    Times.Once);
            }
        }

        public class CardSetIndexedEvent
        {
            [Fact]
            public async Task WhenGettingEmptyIndex_ShouldRaiseEvent()
            {
                // Arrange.

                var mockIndexManager = MockBuilder
                    .CreateMock<IIndexManager>()
                    .WithDefault(IndexKind.CardSet);

                var mockFetcher = MockBuilder
                    .CreateMock<IMagicFetcher>()
                    .WithAvailableResources(ExternalResources.All)
                    .WithCardSets(MockBuilder.CreateUnparsedCardSets(3));

                var repository = new MagicRepository(mockIndexManager.Object, mockFetcher.Object);

                using (var monitoredRepository = repository.Monitor())
                {
                    // Act.

                    var _ = await repository.GetCardSetsAsync();

                    // Assert.

                    monitoredRepository
                        .Should().Raise(nameof(IMagicRepository.CardSetIndexed))
                        .WithSender(repository)
                        .WithArgs<EventArgs>(args => args == EventArgs.Empty);
                }
            }

            [Fact]
            public async Task WhenGettingNotEmptyIndex_ShouldNotRaiseEvent()
            {
                // Arrange.

                var mockIndexManager = MockBuilder
                    .CreateMock<IIndexManager>()
                    .WithDefault(IndexKind.CardSet)
                    .WithExistingCardSets(MockBuilder.CreateUnparsedCardSets(3));

                var mockFetcher = MockBuilder
                    .CreateMock<IMagicFetcher>()
                    .WithAvailableResources(ExternalResources.All)
                    .WithoutCardSets();

                var repository = new MagicRepository(mockIndexManager.Object, mockFetcher.Object);

                using (var monitoredRepository = repository.Monitor())
                {
                    // Act.

                    var _ = await repository.GetCardSetsAsync();

                    // Assert.

                    monitoredRepository
                        .Should().NotRaise(nameof(IMagicRepository.CardSetIndexed));
                }
            }
        }

        public class CardIndexedEvent
        {
            [Fact]
            public async Task WhenGettingEmptyIndex_ShouldFireEvent()
            {
                // Arrange.

                var mockIndexManager = MockBuilder
                    .CreateMock<IIndexManager>()
                    .WithDefault(IndexKind.Card);

                var mockFetcher = MockBuilder
                    .CreateMock<IMagicFetcher>()
                    .WithAvailableResources(ExternalResources.All)
                    .WithCards(MockBuilder.CreateUnparsedCards("X01", 1));

                var repository = new MagicRepository(mockIndexManager.Object, mockFetcher.Object);

                var cardSet = new UnparsedBlob.CardSet
                {
                    Code = "X01",
                    Name = "[_MOCK_NAME_]",
                    ReleasedTimestamp = Constant.EpochTimestamp
                };

                using (var monitoredRepository = repository.Monitor())
                {
                    // Act.

                    var _ = await repository.GetCardsAsync(cardSet);

                    // Assert.

                    monitoredRepository
                        .Should().Raise(nameof(IMagicRepository.CardIndexed))
                        .WithSender(repository)
                        .WithArgs<EventArgs>(args => args == EventArgs.Empty);
                }
            }

            [Fact]
            public async Task WhenGettingNotEmptyIndex_ShouldNotFireEvent()
            {
                // Arrange.

                var mockIndexManager = MockBuilder
                    .CreateMock<IIndexManager>()
                    .WithDefault(IndexKind.Card)
                    .WithExistingCards(MockBuilder.CreateUnparsedCards("X01", 1));

                var mockFetcher = MockBuilder
                    .CreateMock<IMagicFetcher>()
                    .WithAvailableResources(ExternalResources.All)
                    .WithoutCards();

                var repository = new MagicRepository(mockIndexManager.Object, mockFetcher.Object);

                var cardSet = new UnparsedBlob.CardSet
                {
                    Code = "X01",
                    Name = "[_MOCK_NAME_]",
                    ReleasedTimestamp = Constant.EpochTimestamp
                };

                using (var monitoredRepository = repository.Monitor())
                {
                    // Act.

                    var _ = await repository.GetCardsAsync(cardSet);

                    // Assert.

                    monitoredRepository
                        .Should().NotRaise(nameof(IMagicRepository.CardIndexed));
                }
            }

            [Fact]
            public async Task WhenGettingNotEmptyIndexButMissingCards_ShouldFireEvent()
            {
                // Arrange.

                var mockIndexManager = MockBuilder
                    .CreateMock<IIndexManager>()
                    .WithDefault(IndexKind.Card)
                    .WithExistingCards(Enumerable
                        .Empty<UnparsedBlob.Card>()
                        .AppendItems(MockBuilder.CreateUnparsedCards("X02", 2))
                        .AppendItems(MockBuilder.CreateUnparsedCards("X05", 5))
                        .ToArray());

                var mockFetcher = MockBuilder
                    .CreateMock<IMagicFetcher>()
                    .WithAvailableResources(ExternalResources.All)
                    .WithCards(MockBuilder.CreateUnparsedCards("X03", 3));

                var repository = new MagicRepository(mockIndexManager.Object, mockFetcher.Object);

                var cardSet = new UnparsedBlob.CardSet
                {
                    Code = "X03",
                    Name = "[_MOCK_NAME_]",
                    ReleasedTimestamp = Constant.EpochTimestamp
                };

                using (var monitoredRepository = repository.Monitor())
                {
                    // Act.

                    var _ = await repository.GetCardsAsync(cardSet);

                    // Assert.

                    monitoredRepository
                        .Should().Raise(nameof(IMagicRepository.CardIndexed))
                        .WithSender(repository)
                        .WithArgs<EventArgs>(args => args == EventArgs.Empty);
                }
            }
        }
    }
}