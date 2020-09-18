// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WizardFetcherTests.cs" company="nGratis">
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
// <creation_timestamp>Tuesday, 18 December 2018 8:48:40 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Test
{
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using FluentAssertions;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Framework;
    using Xunit;

    public class WizardFetcherTests
    {
        public class FetchCardImageAsyncMethod
        {
            [Fact(Skip = "Fetching card image is supported by Scryfall fetcher.")]
            public async Task WhenGettingSuccessfulResponse_ShouldLoadJpegImage()
            {
                // Arrange.

                var stubHandler = StubHttpMessageHandler
                    .Create()
                    .WithSuccessfulResponseInSession(
                        "http://gatherer.wizards.com/Handlers/Image.ashx?multiverseid=64&type=card",
                        "Raw_WOTC",
                        "64.jpeg");

                var fetcher = new WizardFetcher(stubHandler);

                var card = new UnparsedBlob.Card
                {
                    MultiverseId = 64
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

            [Fact(Skip = "Fetching card image is supported by Scryfall fetcher.")]
            public void WhenGettingUnsuccessfulResponse_ShouldThrowKvasirException()
            {
                // Arrange.

                var stubHandler = StubHttpMessageHandler
                    .Create()
                    .WithResponse(
                        "http://gatherer.wizards.com/Handlers/Image.ashx?multiverseid=64&type=card",
                        HttpStatusCode.NotFound);

                var fetcher = new WizardFetcher(stubHandler);

                var card = new UnparsedBlob.Card
                {
                    Name = "[_MOCK_NAME_]",
                    MultiverseId = 64
                };

                // Act & Assert.

                fetcher
                    .Awaiting(self => self.FetchCardImageAsync(card))
                    .Should().Throw<KvasirException>()
                    .WithMessage(
                        "Failed to reach WIZARD.com when trying to fetch card image! " +
                        "Card: [[_MOCK_NAME_]]. " +
                        "Status Code: [NotFound].");
            }
        }

        public class FetchRulesAsyncMethod
        {
            [Fact]
            public async Task WhenGettingSuccessfulResponse_ShouldLoadRulesText()
            {
                // Arrange.

                var stubHandler = StubHttpMessageHandler
                    .Create()
                    .WithSuccessfulResponseInSession(
                        "https://magic.wizards.com/en/game-info/gameplay/rules-and-formats/rules",
                        "Raw_WOTC",
                        "rules")
                    .WithSuccessfulResponseInSession(
                        "https://media.wizards.com/2019/downloads/MagicCompRules%2020191004.txt",
                        "Raw_WOTC",
                        "MagicCompRules_20191004.txt");

                var fetcher = new WizardFetcher(stubHandler);

                // Act.

                var rules = await fetcher.FetchRulesAsync();

                // Assert.

                rules
                    .Should().NotBeNull()
                    .And.HaveCount(2527)
                    .And.NotContainNulls();

                rules
                    .First().Id
                    .Should().Be("1");

                rules
                    .First().Text
                    .Should().Be("Game Concepts");

                rules
                    .Last().Id
                    .Should().Be("905.6");

                rules
                    .Last().Text
                    .Should().Be(
                        "Once the starting player has been determined, " +
                        "each player sets their life total to 20 and draws a hand of seven cards.");

                foreach (var rule in rules)
                {
                    rule
                        .Id
                        .Should().NotBeNullOrEmpty()
                        .And.MatchRegex(@"^\d{1,3}(\.\d+[a-z]?)?$");

                    rule
                        .Text
                        .Should().NotBeNullOrEmpty();
                }
            }
        }
    }
}