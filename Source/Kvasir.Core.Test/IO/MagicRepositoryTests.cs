// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicRepositoryTests.cs" company="nGratis">
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
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Contract.Magic;
    using Xunit;

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

                var mockMagicFetchers = new[]
                {
                    MockBuilder
                        .CreateMock<IMagicFetcher>()
                        .WithAvailableResources(ExternalResources.CardSet | ExternalResources.Card),
                    MockBuilder
                        .CreateMock<IMagicFetcher>()
                        .WithAvailableResources(ExternalResources.CardImage)
                };

                // Act.

                var action = new Action(() =>
                {
                    var _ = new MagicRepository(mockIndexManager.Object, mockMagicFetchers.ToObjects());
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

                var mockMagicFetcher = MockBuilder
                    .CreateMock<IMagicFetcher>()
                    .WithAvailableResources(ExternalResources.None);

                // Act.

                var action = new Action(() =>
                {
                    var _ = new MagicRepository(mockIndexManager.Object, mockMagicFetcher.Object);
                });

                // Assert.

                action
                    .Should().Throw<KvasirException>()
                    .WithMessage(
                        "One or more external resource(s) are invalid! " +
                        "Missing Resource(s): [CardSet], [Card], [CardImage].");
            }

            [Fact]
            public void WhenGettingFetchersWithDuplicatingAvailableResources_ShouldThrowKvasirException()
            {
                // Arrange.

                var mockIndexManager = MockBuilder
                    .CreateMock<IIndexManager>();

                var mockMagicFetchers = new[]
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
                    var _ = new MagicRepository(mockIndexManager.Object, mockMagicFetchers.ToObjects());
                });

                // Assert.

                action
                    .Should().Throw<KvasirException>()
                    .WithMessage(
                        "One or more external resource(s) are invalid! " +
                        "Duplicating Resource(s): [CardSet], [Card], [CardImage].");
            }

            [Fact]
            public void WhenGettingFetchersWithMissingAndDuplicatingAvailableResources_ShouldThrowKvasirException()
            {
                // Arrange.

                var mockIndexManager = MockBuilder
                    .CreateMock<IIndexManager>();

                var mockMagicFetchers = new[]
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
                    var _ = new MagicRepository(mockIndexManager.Object, mockMagicFetchers.ToObjects());
                });

                // Assert.

                action
                    .Should().Throw<KvasirException>()
                    .WithMessage(
                        "One or more external resource(s) are invalid! " +
                        "Missing Resource(s): [CardImage]. " +
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

                var mockMagicFetcher = MockBuilder
                    .CreateMock<IMagicFetcher>()
                    .WithAvailableResources(ExternalResources.All)
                    .WithCardSets(MockBuilder.CreateCardSets(3));

                var magicRepository = new MagicRepository(mockIndexManager.Object, mockMagicFetcher.Object);

                // Act.

                var cardSets = await magicRepository.GetCardSetsAsync();

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

                mockMagicFetcher.Verify(
                    mock => mock.GetCardSetsAsync(),
                    Times.Once);
            }

            [Fact]
            public async Task WhenGettingNotEmptyIndex_ShouldPopulateItFromIndex()
            {
                // Arrange.

                var mockIndexManager = MockBuilder
                    .CreateMock<IIndexManager>()
                    .WithDefault(IndexKind.CardSet)
                    .WithExistingCardSets(MockBuilder.CreateCardSets(3));

                var mockMagicFetcher = MockBuilder
                    .CreateMock<IMagicFetcher>()
                    .WithAvailableResources(ExternalResources.All)
                    .WithoutCardSets();

                var magicRepository = new MagicRepository(mockIndexManager.Object, mockMagicFetcher.Object);

                // Act.

                var cardSets = await magicRepository.GetCardSetsAsync();

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

                mockMagicFetcher.Verify(
                    mock => mock.GetCardSetsAsync(),
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

                var mockMagicFetcher = MockBuilder
                    .CreateMock<IMagicFetcher>()
                    .WithAvailableResources(ExternalResources.All)
                    .WithCards(Enumerable
                        .Empty<Card>()
                        .Append(MockBuilder.CreteCards("X02", 2))
                        .Append(MockBuilder.CreteCards("X03", 3))
                        .Append(MockBuilder.CreteCards("X05", 5))
                        .ToArray());

                var magicRepository = new MagicRepository(mockIndexManager.Object, mockMagicFetcher.Object);

                var cardSet = new CardSet
                {
                    Code = "X03",
                    Name = "[_MOCK_NAME_]",
                    ReleasedTimestamp = Constant.EpochTimestamp
                };

                // Act.

                var cards = await magicRepository.GetCardsAsync(cardSet);

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

                mockMagicFetcher.Verify(
                    mock => mock.GetCardsAsync(Moq.It.IsAny<CardSet>()),
                    Times.Once);

                mockMagicFetcher.Verify(
                    mock => mock.GetCardsAsync(Arg.CardSet.Is("X03")),
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
                        .Empty<Card>()
                        .Append(MockBuilder.CreteCards("X02", 2))
                        .Append(MockBuilder.CreteCards("X03", 3))
                        .Append(MockBuilder.CreteCards("X05", 5))
                        .ToArray());

                var mockMagicFetcher = MockBuilder
                    .CreateMock<IMagicFetcher>()
                    .WithAvailableResources(ExternalResources.All)
                    .WithoutCards();

                var magicRepository = new MagicRepository(mockIndexManager.Object, mockMagicFetcher.Object);

                var cardSet = new CardSet
                {
                    Code = "X03",
                    Name = "[_MOCK_NAME_]",
                    ReleasedTimestamp = Constant.EpochTimestamp
                };

                // Act.

                var cards = await magicRepository.GetCardsAsync(cardSet);

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

                mockMagicFetcher.Verify(
                    mock => mock.GetCardsAsync(Moq.It.IsAny<CardSet>()),
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
                        .Empty<Card>()
                        .Append(MockBuilder.CreteCards("X02", 2))
                        .Append(MockBuilder.CreteCards("X05", 5))
                        .ToArray());

                var mockMagicFetcher = MockBuilder
                    .CreateMock<IMagicFetcher>()
                    .WithAvailableResources(ExternalResources.All)
                    .WithCards(MockBuilder.CreteCards("X03", 3));

                var magicRepository = new MagicRepository(mockIndexManager.Object, mockMagicFetcher.Object);

                var cardSet = new CardSet
                {
                    Code = "X03",
                    Name = "[_MOCK_NAME_]",
                    ReleasedTimestamp = Constant.EpochTimestamp
                };

                // Act.

                var cards = await magicRepository.GetCardsAsync(cardSet);

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

                mockMagicFetcher.Verify(
                    mock => mock.GetCardsAsync(Moq.It.IsAny<CardSet>()),
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

                var mockMagicFetcher = MockBuilder
                    .CreateMock<IMagicFetcher>()
                    .WithAvailableResources(ExternalResources.All)
                    .WithCardSets(MockBuilder.CreateCardSets(3));

                var magicRepository = new MagicRepository(mockIndexManager.Object, mockMagicFetcher.Object);

                using (var monitoredRepository = magicRepository.Monitor())
                {
                    // Act.

                    var _ = await magicRepository.GetCardSetsAsync();

                    // Assert.

                    monitoredRepository
                        .Should().Raise(nameof(IMagicRepository.CardSetIndexed))
                        .WithSender(magicRepository)
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
                    .WithExistingCardSets(MockBuilder.CreateCardSets(3));

                var mockMagicFetcher = MockBuilder
                    .CreateMock<IMagicFetcher>()
                    .WithAvailableResources(ExternalResources.All)
                    .WithoutCardSets();

                var magicRepository = new MagicRepository(mockIndexManager.Object, mockMagicFetcher.Object);

                using (var monitoredRepository = magicRepository.Monitor())
                {
                    // Act.

                    var _ = await magicRepository.GetCardSetsAsync();

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

                var mockMagicFetcher = MockBuilder
                    .CreateMock<IMagicFetcher>()
                    .WithAvailableResources(ExternalResources.All)
                    .WithCards(MockBuilder.CreteCards("X01", 1));

                var magicRepository = new MagicRepository(mockIndexManager.Object, mockMagicFetcher.Object);

                var cardSet = new CardSet
                {
                    Code = "X01",
                    Name = "[_MOCK_NAME_]",
                    ReleasedTimestamp = Constant.EpochTimestamp
                };

                using (var monitoredRepository = magicRepository.Monitor())
                {
                    // Act.

                    var _ = await magicRepository.GetCardsAsync(cardSet);

                    // Assert.

                    monitoredRepository
                        .Should().Raise(nameof(IMagicRepository.CardIndexed))
                        .WithSender(magicRepository)
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
                    .WithExistingCards(MockBuilder.CreteCards("X01", 1));

                var mockMagicFetcher = MockBuilder
                    .CreateMock<IMagicFetcher>()
                    .WithAvailableResources(ExternalResources.All)
                    .WithoutCards();

                var magicRepository = new MagicRepository(mockIndexManager.Object, mockMagicFetcher.Object);

                var cardSet = new CardSet
                {
                    Code = "X01",
                    Name = "[_MOCK_NAME_]",
                    ReleasedTimestamp = Constant.EpochTimestamp
                };

                using (var monitoredRepository = magicRepository.Monitor())
                {
                    // Act.

                    var _ = await magicRepository.GetCardsAsync(cardSet);

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
                        .Empty<Card>()
                        .Append(MockBuilder.CreteCards("X02", 2))
                        .Append(MockBuilder.CreteCards("X05", 5))
                        .ToArray());

                var mockMagicFetcher = MockBuilder
                    .CreateMock<IMagicFetcher>()
                    .WithAvailableResources(ExternalResources.All)
                    .WithCards(MockBuilder.CreteCards("X03", 3));

                var magicRepository = new MagicRepository(mockIndexManager.Object, mockMagicFetcher.Object);

                var cardSet = new CardSet
                {
                    Code = "X03",
                    Name = "[_MOCK_NAME_]",
                    ReleasedTimestamp = Constant.EpochTimestamp
                };

                using (var monitoredRepository = magicRepository.Monitor())
                {
                    // Act.

                    var _ = await magicRepository.GetCardsAsync(cardSet);

                    // Assert.

                    monitoredRepository
                        .Should().Raise(nameof(IMagicRepository.CardIndexed))
                        .WithSender(magicRepository)
                        .WithArgs<EventArgs>(args => args == EventArgs.Empty);
                }
            }
        }
    }
}