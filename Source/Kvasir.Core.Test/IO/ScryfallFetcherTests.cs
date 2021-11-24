// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScryfallFetcherTests.cs" company="nGratis">
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
// <creation_timestamp>Saturday, 11 May 2019 6:34:46 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using FluentAssertions;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Framework;
    using nGratis.Cop.Olympus.Contract;
    using nGratis.Cop.Olympus.Framework;
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
                        .And.MatchRegex(@"^\w{2,6}$");

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
                    .Awaiting(self => self.FetchCardSetsAsync())
                    .Should().Throw<KvasirException>()
                    .WithMessage(
                        "Failed to reach SCRYFALL.com when trying to fetch card sets! " +
                        "Status Code: [NotFound].");
            }
        }

        public class FetchCardsAsyncMethod
        {
            [Theory]
            [MemberData(nameof(TestData.FetchingCardsTheories), MemberType = typeof(TestData))]
            public async Task WhenGettingSuccessfulResponse_ShouldParseJson(FetchingCardsTheory theory)
            {
                // Arrange.

                var stubHandler = StubHttpMessageHandler
                    .Create();

                Enumerable
                    .Range(1, theory.PageCount)
                    .Select(number => new
                    {
                        EntryKey = $"{theory.CardSetCode}_{number:D2}.json",
                        TargetUrl = string.Format(
                            "https://api.scryfall.com/cards/search?q=e%3a{0}&unique=prints&order=name&page={1}",
                            theory.CardSetCode,
                            number)
                    })
                    .ForEach(anon =>
                    {
                        stubHandler.WithSuccessfulResponseInSession(anon.TargetUrl, "Raw_SCRYFALL", anon.EntryKey);
                    });

                var fetcher = new ScryfallFetcher(stubHandler);

                var cardSet = new UnparsedBlob.CardSet
                {
                    Code = theory.CardSetCode,
                    Name = "[_MOCK_NAME_]",
                    ReleasedTimestamp = Constant.EpochTimestamp
                };

                // Act.

                var cards = await fetcher.FetchCardsAsync(cardSet);

                // Assert.

                cards
                    .Should().NotBeNull()
                    .And.HaveCount(theory.ExpectedCardCount)
                    .And.NotContainNulls();

                cards
                    .ForEach(card => card.Must().HaveValidContent());
            }

            [Fact]
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
                    .Awaiting(self => self.FetchCardsAsync(cardSet))
                    .Should().Throw<KvasirException>()
                    .WithMessage(
                        "Failed to reach SCRYFALL.com when trying to fetch cards! " +
                        "Card Set: [[_MOCK_NAME_]]. " +
                        "Status Code: [NotFound].");
            }

            public static class TestData
            {
                public static IEnumerable<object[]> FetchingCardsTheories
                {
                    get
                    {
                        yield return FetchingCardsTheory
                            .Create("WAR", 2)
                            .Expect(275)
                            .WithLabel(1, "Fetching cards with standard single face")
                            .ToXunitTheory();

                        yield return FetchingCardsTheory
                            .Create("ISD", 2)
                            .Expect(239)
                            .WithLabel(2, "Fetching cards with second face")
                            .ToXunitTheory();
                    }
                }
            }

            public class FetchingCardsTheory : CopTheory
            {
                private FetchingCardsTheory()
                {
                }

                public string CardSetCode { get; private init; }

                public int PageCount { get; private init; }

                public int ExpectedCardCount { get; private set; }

                public static FetchingCardsTheory Create(string code, int pageCount)
                {
                    Guard
                        .Require(code, nameof(code))
                        .Is.Not.Empty();

                    Guard
                        .Require(pageCount, nameof(pageCount))
                        .Is.Positive();

                    return new FetchingCardsTheory
                    {
                        CardSetCode = code,
                        PageCount = pageCount
                    };
                }

                public FetchingCardsTheory Expect(int cardCount)
                {
                    Guard
                        .Require(cardCount, nameof(cardCount))
                        .Is.Positive();

                    this.ExpectedCardCount = cardCount;

                    return this;
                }
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
                    .Awaiting(self => self.FetchCardImageAsync(card))
                    .Should().Throw<KvasirException>()
                    .WithMessage(
                        "Failed to reach SCRYFALL.com when trying to fetch card image! " +
                        "Card: [[_MOCK_NAME_]]. " +
                        "Status Code: [NotFound].");
            }
        }
    }
}