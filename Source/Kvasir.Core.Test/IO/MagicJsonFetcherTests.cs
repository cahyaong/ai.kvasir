// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicJsonFetcherTests.cs" company="nGratis">
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
// <creation_timestamp>Saturday, 3 November 2018 9:03:40 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Test
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Threading.Tasks;
    using FluentAssertions;
    using nGratis.AI.Kvasir.Contract;
    using Xunit;

    public class MagicJsonFetcherTests
    {
        public class GetCardSetsAsyncMethod
        {
            [Fact]
            public async Task WhenGettingSuccessfulResponse_ShouldParseHtml()
            {
                // Arrange.

                var stubHandler = StubHttpMessageHandler
                    .Create()
                    .WithSuccessfulResponseInSession("https://mtgjson.com/v4/sets.html", "Raw_MTGJSON4");

                var fetcher = new MagicJsonFetcher(stubHandler);

                // Act.

                var rawCardSets = await fetcher.GetRawCardSetsAsync();

                // Assert.

                rawCardSets
                    .Should().NotBeNull()
                    .And.HaveCountGreaterOrEqualTo(428);

                foreach (var rawCardSet in rawCardSets)
                {
                    rawCardSet
                        .Should().NotBeNull();

                    rawCardSet
                        .Name
                        .Should().NotBeNullOrWhiteSpace()
                        .And.NotMatchRegex(";");

                    rawCardSet
                        .Code
                        .Should().NotBeNullOrEmpty()
                        .And.MatchRegex(@"\w{3,6}");

                    rawCardSet
                        .ReleasedTimestamp
                        .Should().BeAfter(new DateTime(1993, 1, 1));
                }
            }

            [Fact]
            public void WhenGettingUnsuccessfulResponse_ShouldThrowKvasirException()
            {
                // Arrange.

                var stubHandler = StubHttpMessageHandler
                    .Create()
                    .WithResponse("https://mtgjson.com/v4/sets.html", HttpStatusCode.NotFound);

                var fetcher = new MagicJsonFetcher(stubHandler);

                // Act &  Assert.

                fetcher
                    .Awaiting(async self => await self.GetRawCardSetsAsync())
                    .Should().Throw<KvasirException>()
                    .WithMessage(
                        "Failed to reach MTGJSON4.com when trying to fetch card sets! " +
                        "Status Code: [NotFound].");
            }
        }

        public class GetCardsAsyncMethod
        {
            [Fact]
            [SuppressMessage("ReSharper", "StringLiteralTypo")]
            public async Task WhenGettingSuccessfulResponse_ShouldParseJson()
            {
                // Arrange.

                var stubHandler = StubHttpMessageHandler
                    .Create()
                    .WithSuccessfulResponseInSession("https://mtgjson.com/v4/json/GRN.json", "Raw_MTGJSON4");

                var fetcher = new MagicJsonFetcher(stubHandler);

                var rawCardSet = new RawCardSet
                {
                    Code = "GRN",
                    Name = "[_MOCK_NAME_]",
                    ReleasedTimestamp = Constant.EpochTimestamp
                };

                // Act.

                var rawCards = await fetcher.GetRawCardsAsync(rawCardSet);

                // Assert.

                rawCards
                    .Should().NotBeNull()
                    .And.HaveCount(283);

                foreach (var rawCard in rawCards)
                {
                    rawCard
                        .Should().NotBeNull();

                    rawCard
                        .MultiverseId
                        .Should().BePositive();

                    rawCard
                        .CardSetCode
                        .Should().NotBeNullOrEmpty()
                        .And.MatchRegex(@"\w{3,6}");

                    rawCard
                        .Name
                        .Should().NotBeNullOrWhiteSpace();

                    rawCard
                        .ManaCost
                        .Should().NotBeNull()
                        .And.MatchRegex(@"(\{[\dWUBRGX/]+\})*");

                    rawCard
                        .Type
                        .Should().NotBeNullOrEmpty()
                        .And.MatchRegex(@"[a-zA-Z\-\s]+");

                    rawCard
                        .Rarity
                        .Should().NotBeNullOrEmpty()
                        .And.MatchRegex(@"[a-zA-Z]+");

                    rawCard
                        .Text
                        .Should().NotBeNull();

                    rawCard
                        .FlavorText
                        .Should().NotBeNull();

                    rawCard
                        .Power
                        .Should().NotBeNull()
                        .And.MatchRegex(@"[\d\*]*");

                    rawCard
                        .Toughness
                        .Should().NotBeNull()
                        .And.MatchRegex(@"[\d\*]*");

                    rawCard
                        .Number
                        .Should().NotBeNull()
                        .And.MatchRegex(@"[\da-z]+");

                    rawCard
                        .Artist
                        .Should().NotBeNullOrEmpty()
                        .And.MatchRegex(@"[a-zA-Z\s]+");
                }
            }

            [Fact]
            public void WhenGettingContentWithMissingCards_ShouldThrowKvasirException()
            {
                // Arrange.

                var stubHandler = StubHttpMessageHandler
                    .Create()
                    .WithSuccessfulResponse("https://mtgjson.com/v4/json/X42.json", "{ }");

                var fetcher = new MagicJsonFetcher(stubHandler);

                var rawCardSet = new RawCardSet
                {
                    Code = "X42",
                    Name = "[_MOCK_NAME_]",
                    ReleasedTimestamp = Constant.EpochTimestamp
                };

                // Act & Assert.

                fetcher
                     .Awaiting(async self => await self.GetRawCardsAsync(rawCardSet))
                     .Should().Throw<KvasirException>()
                     .WithMessage("Response from MTGJSON4.com is missing cards!");
            }

            [Fact]
            public void WhenGettingUnsuccessfulResponse_ShouldThrowKvasirException()
            {
                // Arrange.

                var stubHandler = StubHttpMessageHandler
                    .Create()
                    .WithResponse("https://mtgjson.com/v4/json/X42.json", HttpStatusCode.NotFound);

                var fetcher = new MagicJsonFetcher(stubHandler);

                var rawCardSet = new RawCardSet
                {
                    Code = "X42",
                    Name = "[_MOCK_NAME_]",
                    ReleasedTimestamp = Constant.EpochTimestamp
                };

                // Act & Assert.

                fetcher
                     .Awaiting(async self => await self.GetRawCardsAsync(rawCardSet))
                     .Should().Throw<KvasirException>()
                     .WithMessage(
                        "Failed to reach MTGJSON4.com when trying to fetch cards! " +
                        "Card Set: [[_MOCK_NAME_]]. " +
                        "Status Code: [NotFound].");
            }
        }
    }
}