// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicHttpFetcherBaseTests.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Monday, 17 December 2018 11:49:47 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.UnitTest;

using System;
using System.Threading.Tasks;
using FluentAssertions;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Framework;
using Xunit;

public class MagicHttpFetcherBaseTests
{
    public class Constructor
    {
        [Fact]
        public void WhenInvoked_ShouldConfigureRequestHeaders()
        {
            // Arrange.

            var stubHandler = StubHttpMessageHandler
                .Create();

            // Act.

            var stubFetcher = StubMagicHttpFetcher
                .Create(stubHandler);

            // Assert.

            stubFetcher.VerifyRequestHeader(
                "Accept: text/html, */*; q=0.8",
                "Accept-Encoding: deflate",
                "User-Agent: AI.Kvasir/0.1");
        }
    }

    public class FetchCardSetsAsyncMethod
    {
        [Fact]
        public async Task WhenDetectingCardSetAsAvailableResource_ShouldInvokeOverridingMethod()
        {
            // Arrange.

            var stubHandler = StubHttpMessageHandler
                .Create();

            var stubFetcher = StubMagicHttpFetcher
                .Create(stubHandler)
                .WithAvailableResources(ExternalResources.CardSet);

            // Act.

            var cardSets = await stubFetcher.FetchCardSetsAsync();

            // Assert.

            cardSets
                .Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void WhenNotDetectingCardSetAsAvailableResource_ShouldThrowNotSupportedException()
        {
            // Arrange.

            var stubHandler = StubHttpMessageHandler
                .Create();

            var stubFetcher = StubMagicHttpFetcher
                .Create(stubHandler);

            // Act & Assert.

            stubFetcher
                .Awaiting(self => self.FetchCardSetsAsync())
                .Should().ThrowAsync<NotSupportedException>();
        }
    }

    public class FetchCardsAsyncMethod
    {
        [Fact]
        public async Task WhenDetectingCardAsAvailableResource_ShouldInvokeOverridingMethod()
        {
            // Arrange.

            var stubHandler = StubHttpMessageHandler
                .Create();

            var stubFetcher = StubMagicHttpFetcher
                .Create(stubHandler)
                .WithAvailableResources(ExternalResources.Card);

            var cardSet = new UnparsedBlob.CardSet
            {
                Code = "[_MOCK_CODE_]",
                Name = "[_MOCK_NAME_]"
            };

            // Act.

            var cards = await stubFetcher.FetchCardsAsync(cardSet);

            // Assert.

            cards
                .Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void WhenNotDetectingCardAsAvailableResource_ShouldThrowNotSupportedException()
        {
            // Arrange.

            var stubHandler = StubHttpMessageHandler
                .Create();

            var stubFetcher = StubMagicHttpFetcher
                .Create(stubHandler);

            var cardSet = new UnparsedBlob.CardSet
            {
                Code = "[_MOCK_CODE_]",
                Name = "[_MOCK_NAME_]"
            };

            // Act & Assert.

            stubFetcher
                .Awaiting(self => self.FetchCardsAsync(cardSet))
                .Should().ThrowAsync<NotSupportedException>();
        }
    }

    public class FetchCardImageAsyncMethod
    {
        [Fact]
        public async Task WhenDetectingCardImageAsAvailableResource_ShouldInvokeOverridingMethod()
        {
            // Arrange.

            var stubHandler = StubHttpMessageHandler
                .Create();

            var stubFetcher = StubMagicHttpFetcher
                .Create(stubHandler)
                .WithAvailableResources(ExternalResources.CardImage);

            var card = new UnparsedBlob.Card
            {
                Name = "[_MOCK_NAME_]"
            };

            // Act.

            var cardImage = await stubFetcher.FetchCardImageAsync(card);

            // Assert.

            cardImage
                .Should().NotBeNull();
        }

        [Fact]
        public void WhenNotDetectingCardImageAsAvailableResource_ShouldThrowNotSupportedException()
        {
            // Arrange.

            var stubHandler = StubHttpMessageHandler
                .Create();

            var stubFetcher = StubMagicHttpFetcher
                .Create(stubHandler);

            var card = new UnparsedBlob.Card
            {
                Name = "[_MOCK_NAME_]"
            };

            // Act & Assert.

            stubFetcher
                .Awaiting(self => self.FetchCardImageAsync(card))
                .Should().ThrowAsync<NotSupportedException>();
        }
    }
}