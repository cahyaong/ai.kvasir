﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseMagicHttpFetcherTests.cs" company="nGratis">
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
// <creation_timestamp>Monday, 17 December 2018 11:49:47 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Test
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using nGratis.AI.Kvasir.Contract;
    using Xunit;

    public class BaseMagicHttpFetcherTests
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

                stubFetcher.VerifyRequestHeaders(
                    "Accept: text/html, */*; q=0.8",
                    "Accept-Encoding: deflate",
                    "User-Agent: AI.Kvasir/0.1");
            }
        }

        public class FetchRawCardSetsAsyncMethod
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

                var rawCardSets = await stubFetcher.FetchRawCardSetsAsync();

                // Assert.

                rawCardSets
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
                    .Awaiting(async self => await self.FetchRawCardSetsAsync())
                    .Should().Throw<NotSupportedException>();
            }
        }

        public class FetchRawCardsAsyncMethod
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

                var rawCardSet = new RawCardSet
                {
                    Code = "[_MOCK_CODE_]",
                    Name = "[_MOCK_NAME_]"
                };

                // Act.

                var rawCards = await stubFetcher.FetchRawCardsAsync(rawCardSet);

                // Assert.

                rawCards
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

                var rawCardSet = new RawCardSet
                {
                    Code = "[_MOCK_CODE_]",
                    Name = "[_MOCK_NAME_]"
                };

                // Act & Assert.

                stubFetcher
                    .Awaiting(async self => await self.FetchRawCardsAsync(rawCardSet))
                    .Should().Throw<NotSupportedException>();
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

                var rawCard = new RawCard
                {
                    Name = "[_MOCK_NAME_]"
                };

                // Act.

                var cardImage = await stubFetcher.FetchCardImageAsync(rawCard);

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

                var rawCard = new RawCard
                {
                    Name = "[_MOCK_NAME_]"
                };

                // Act & Assert.

                stubFetcher
                    .Awaiting(async self => await self.FetchCardImageAsync(rawCard))
                    .Should().Throw<NotSupportedException>();
            }
        }
    }
}