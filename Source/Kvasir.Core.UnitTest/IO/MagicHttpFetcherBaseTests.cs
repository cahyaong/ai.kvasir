// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicHttpFetcherBaseTests.cs" company="nGratis">
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