// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicJsonFetcherTests.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, 3 November 2018 9:03:40 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.UnitTest;

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Framework;
using Xunit;

public class MagicJsonFetcherTests
{
    public class FetchCardSetsAsyncMethod
    {
        [Fact]
        public async Task WhenGettingSuccessfulResponse_ShouldParseHtml()
        {
            // Arrange.

            var stubHandler = StubHttpMessageHandler
                .Create()
                .WithSuccessfulResponseInSession("https://mtgjson.com/sets.html", "Raw_MTGJSON4");

            var fetcher = new MagicJsonFetcher(stubHandler);

            // Act.

            var cardSets = await fetcher.FetchCardSetsAsync();

            // Assert.

            cardSets
                .Should().NotBeNull()
                .And.HaveCountGreaterOrEqualTo(438);

            foreach (var cardSet in cardSets)
            {
                cardSet
                    .Should().NotBeNull();

                cardSet
                    .Name
                    .Should().NotBeNullOrWhiteSpace()
                    .And.NotMatchRegex(";");

                cardSet
                    .Code
                    .Should().NotBeNullOrEmpty()
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
                .WithResponse("https://mtgjson.com/sets.html", HttpStatusCode.NotFound);

            var fetcher = new MagicJsonFetcher(stubHandler);

            // Act &  Assert.

            fetcher
                .Awaiting(self => self.FetchCardSetsAsync())
                .Should().ThrowAsync<KvasirException>()
                .WithMessage(
                    "Failed to reach MTGJSON4.com when trying to fetch card sets! " +
                    "Status Code: [NotFound].");
        }
    }

    public class FetchCardsAsyncMethod
    {
        [Fact]
        public async Task WhenGettingSuccessfulResponse_ShouldParseJson()
        {
            // Arrange.

            var stubHandler = StubHttpMessageHandler
                .Create()
                .WithSuccessfulResponseInSession("https://mtgjson.com/json/GRN.json", "Raw_MTGJSON4");

            var fetcher = new MagicJsonFetcher(stubHandler);

            var cardSet = new UnparsedBlob.CardSet
            {
                Code = "GRN",
                Name = "[_MOCK_NAME_]",
                ReleasedTimestamp = Constant.EpochTimestamp
            };

            // Act.

            var cards = await fetcher.FetchCardsAsync(cardSet);

            // Assert.

            cards
                .Should().NotBeNull()
                .And.HaveCount(283);

            cards
                .ForEach(card => card.Must().HaveValidContent());
        }

        [Fact]
        public void WhenGettingContentWithMissingCards_ShouldThrowKvasirException()
        {
            // Arrange.

            var stubHandler = StubHttpMessageHandler
                .Create()
                .WithSuccessfulResponse("https://mtgjson.com/json/X42.json", "{ }");

            var fetcher = new MagicJsonFetcher(stubHandler);

            var cardSet = new UnparsedBlob.CardSet
            {
                Code = "X42",
                Name = "[_MOCK_NAME_]",
                ReleasedTimestamp = Constant.EpochTimestamp
            };

            // Act & Assert.

            fetcher
                .Awaiting(self => self.FetchCardsAsync(cardSet))
                .Should().ThrowAsync<KvasirException>()
                .WithMessage("Response from MTGJSON4.com is missing cards!");
        }

        [Fact]
        public void WhenGettingUnsuccessfulResponse_ShouldThrowKvasirException()
        {
            // Arrange.

            var stubHandler = StubHttpMessageHandler
                .Create()
                .WithResponse("https://mtgjson.com/json/X42.json", HttpStatusCode.NotFound);

            var fetcher = new MagicJsonFetcher(stubHandler);

            var cardSet = new UnparsedBlob.CardSet
            {
                Code = "X42",
                Name = "[_MOCK_NAME_]",
                ReleasedTimestamp = Constant.EpochTimestamp
            };

            // Act & Assert.

            fetcher
                .Awaiting(self => self.FetchCardsAsync(cardSet))
                .Should().ThrowAsync<KvasirException>()
                .WithMessage(
                    "Failed to reach MTGJSON4.com when trying to fetch cards! " +
                    "Card Set: [[_MOCK_NAME_]]. " +
                    "Status Code: [NotFound].");
        }
    }
}