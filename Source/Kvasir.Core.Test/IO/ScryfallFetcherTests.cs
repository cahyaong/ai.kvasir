// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScryfallFetcherTests.cs" company="nGratis">
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
// <creation_timestamp>Saturday, 11 May 2019 6:34:46 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Test
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Threading.Tasks;
    using FluentAssertions;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Testing;
    using Xunit;

    public class ScryfallFetcherTests
    {
        public class FetchCardSetsAsyncMethod
        {
            [Fact]
            public async Task WhenGettingSuccessfulResponse_ShouldParseJson()
            {
                // Arrange.

                var stubHandler = StubHttpMessageHandler
                    .Create()
                    .WithSuccessfulResponseInSession("https://api.scryfall.com/sets", "Raw_SCRYFALL", "sets.json");

                var fetcher = new ScryfallFetcher(stubHandler);

                // Act.

                var cardSets = await fetcher.FetchCardSetsAsync();

                // Assert.

                cardSets
                    .Should().NotBeNull()
                    .And.HaveCountGreaterOrEqualTo(544)
                    .And.NotContainNulls();

                foreach (var cardSet in cardSets)
                {
                    cardSet
                        .Name
                        .Should().NotBeNullOrWhiteSpace();

                    cardSet
                        .Code
                        .Should().NotBeNullOrWhiteSpace()
                        .And.MatchRegex(@"\w{2,6}");

                    cardSet
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
                    .WithResponse("https://api.scryfall.com/sets", HttpStatusCode.NotFound);

                var fetcher = new ScryfallFetcher(stubHandler);

                // Act &  Assert.

                fetcher
                    .Awaiting(async self => await self.FetchCardSetsAsync())
                    .Should().Throw<KvasirException>()
                    .WithMessage(
                        "Failed to reach SCRYFALL.com when trying to fetch card sets! " +
                        "Status Code: [NotFound].");
            }
        }

        public class FetchCardsAsyncMethod
        {
            [Fact]
            [SuppressMessage("ReSharper", "StringLiteralTypo")]
            public async Task WhenGettingSuccessfulResponse_ShouldParseJson()
            {
                // Arrange.

                var stubHandler = StubHttpMessageHandler
                    .Create()
                    .WithSuccessfulResponseInSession(
                        "https://api.scryfall.com/cards/search?q=e%3aWAR&unique=prints&order=name&page=1",
                        "Raw_SCRYFALL",
                        "WAR_01.json")
                    .WithSuccessfulResponseInSession(
                        "https://api.scryfall.com/cards/search?q=e%3aWAR&unique=prints&order=name&page=2",
                        "Raw_SCRYFALL",
                        "WAR_02.json");

                var fetcher = new ScryfallFetcher(stubHandler);

                var cardSet = new UnparsedBlob.CardSet
                {
                    Code = "WAR",
                    Name = "[_MOCK_NAME_]",
                    ReleasedTimestamp = Constant.EpochTimestamp
                };

                // Act.

                var cards = await fetcher.FetchCardsAsync(cardSet);

                // Assert.

                cards
                    .Should().NotBeNull()
                    .And.HaveCount(275)
                    .And.NotContainNulls();

                foreach (var card in cards)
                {
                    card
                        .MultiverseId
                        .Should().BePositive();

                    card
                        .ScryfallId
                        .Should().NotBeNull()
                        .And.MatchRegex(@"[0-9a-f]{8}\-([0-9a-f]{4}\-){3}[0-9a-f]{12}");

                    card
                        .ScryfallImageUrl
                        .Should().NotBeNullOrEmpty();

                    card
                        .CardSetCode
                        .Should().NotBeNullOrEmpty()
                        .And.MatchRegex(@"\w{3,6}");

                    card
                        .Name
                        .Should().NotBeNullOrWhiteSpace();

                    card
                        .ManaCost
                        .Should().NotBeNull()
                        .And.MatchRegex(@"(\{[\dWUBRGX/]+\})*");

                    card
                        .Type
                        .Should().NotBeNullOrEmpty()
                        .And.MatchRegex(@"[a-zA-Z\-\s]+");

                    card
                        .Rarity
                        .Should().NotBeNullOrEmpty()
                        .And.MatchRegex(@"[a-zA-Z]+");

                    card
                        .Text
                        .Should().NotBeNull();

                    card
                        .FlavorText
                        .Should().NotBeNull();

                    card
                        .Power
                        .Should().NotBeNull()
                        .And.MatchRegex(@"[\d\*]*");

                    card
                        .Toughness
                        .Should().NotBeNull()
                        .And.MatchRegex(@"[\d\*]*");

                    card
                        .Number
                        .Should().NotBeNull()
                        .And.MatchRegex(@"[\da-z]+");

                    card
                        .Artist
                        .Should().NotBeNullOrEmpty()
                        .And.MatchRegex(@"[a-zA-Z\s]+");
                }
            }

            [Fact]
            [SuppressMessage("ReSharper", "StringLiteralTypo")]
            public void WhenGettingUnsuccessfulResponse_ShouldThrowKvasirException()
            {
                // Arrange.

                var stubHandler = StubHttpMessageHandler
                    .Create()
                    .WithResponse(
                        "https://api.scryfall.com/cards/search?q=e%3aX42&unique=prints&order=name&page=1",
                        HttpStatusCode.NotFound)
                    .WithResponse(
                        "https://api.scryfall.com/cards/search?q=e%3aX42&unique=prints&order=name&page=2",
                        HttpStatusCode.NotFound);

                var fetcher = new ScryfallFetcher(stubHandler);

                var cardSet = new UnparsedBlob.CardSet
                {
                    Code = "X42",
                    Name = "[_MOCK_NAME_]",
                    ReleasedTimestamp = Constant.EpochTimestamp
                };

                // Act & Assert.

                fetcher
                    .Awaiting(async self => await self.FetchCardsAsync(cardSet))
                    .Should().Throw<KvasirException>()
                    .WithMessage(
                        "Failed to reach SCRYFALL.com when trying to fetch cards! " +
                        "Card Set: [[_MOCK_NAME_]]. " +
                        "Status Code: [NotFound].");
            }
        }

        public class FetchCardImageAsyncMethod
        {
            [Fact]
            public async Task WhenGettingSuccessfulResponse_ShouldLoadJpegImage()
            {
                // Arrange.

                var stubHandler = StubHttpMessageHandler
                    .Create()
                    .WithSuccessfulResponseInSession(
                        "https://img.scryfall.com/cards/border_crop/mock_image.jpeg",
                        "Raw_SCRYFALL",
                        "64.jpeg");

                var fetcher = new ScryfallFetcher(stubHandler);

                var card = new UnparsedBlob.Card
                {
                    ScryfallImageUrl = "mock_image.jpeg"
                };

                // Act.

                var cardImage = await fetcher.FetchCardImageAsync(card);

                // Assert.

                cardImage
                    .Should().NotBeNull();

                cardImage
                    .Width
                    .Should().Be(64);

                cardImage
                    .Height
                    .Should().Be(64);
            }

            [Fact]
            public void WhenGettingUnsuccessfulResponse_ShouldThrowKvasirException()
            {
                // Arrange.

                var stubHandler = StubHttpMessageHandler
                    .Create()
                    .WithResponse(
                        "https://img.scryfall.com/cards/border_crop/mock_image.jpeg",
                        HttpStatusCode.NotFound);

                var fetcher = new ScryfallFetcher(stubHandler);

                var card = new UnparsedBlob.Card
                {
                    Name = "[_MOCK_NAME_]",
                    ScryfallImageUrl = "mock_image.jpeg"
                };

                // Act & Assert.

                fetcher
                    .Awaiting(async self => await self.FetchCardImageAsync(card))
                    .Should().Throw<KvasirException>()
                    .WithMessage(
                        "Failed to reach SCRYFALL.com when trying to fetch card image! " +
                        "Card: [[_MOCK_NAME_]]. " +
                        "Status Code: [NotFound].");
            }
        }
    }
}