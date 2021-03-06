﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScryfallFetcher.cs" company="nGratis">
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
// <creation_timestamp>Friday, 10 May 2019 11:11:57 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;
    using Newtonsoft.Json.Linq;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Olympus.Contract;

    public class ScryfallFetcher : MagicHttpFetcherBase
    {
        public ScryfallFetcher(IStorageManager storageManager)
            : base("SCRYFALL", storageManager, KeyCalculator.Instance)
        {
        }

        internal ScryfallFetcher(HttpMessageHandler messageHandler)
            : base(messageHandler)
        {
        }

        public override ExternalResources AvailableResources =>
            ExternalResources.CardSet |
            ExternalResources.Card |
            ExternalResources.CardImage;

        protected override async Task<IReadOnlyCollection<UnparsedBlob.CardSet>> FetchCardSetsCoreAsync()
        {
            var response = await this.HttpClient.GetAsync(new Uri(Link.ApiUri, "sets"));

            if (!response.IsSuccessStatusCode)
            {
                throw new KvasirException(
                    @"Failed to reach SCRYFALL.com when trying to fetch card sets! " +
                    $"Status Code: [{response.StatusCode}].");
            }

            var contentToken = JObject.Parse(await response.Content.ReadAsStringAsync());

            var cardSetTokens = contentToken
                .SelectToken("$.data")?
                .Children()
                .ToArray();

            if (cardSetTokens?.Any() != true)
            {
                throw new KvasirException("Failed to find card sets!");
            }

            return cardSetTokens
                .Select(token => new UnparsedBlob.CardSet
                {
                    Code = token.ReadValue("code").ToUpperInvariant(),
                    Name = token.ReadValue("name"),
                    ReleasedTimestamp = token.ReadValue<DateTime>("released_at")
                })
                .ToArray();
        }

        protected override async Task<IReadOnlyCollection<UnparsedBlob.Card>> FetchCardsCoreAsync(
            UnparsedBlob.CardSet cardSet)
        {
            var cards = new List<UnparsedBlob.Card>();
            var pageNumber = 1;
            var hasAnotherPage = true;

            while (hasAnotherPage)
            {
                var parameters = HttpUtility.ParseQueryString(string.Empty);

                parameters["q"] = $"e:{cardSet.Code}";
                parameters["unique"] = "prints";
                parameters["order"] = "name";
                parameters["page"] = pageNumber.ToString();

                var response = await this.HttpClient.GetAsync(new Uri(Link.ApiUri, $"cards/search?{parameters}"));

                if (!response.IsSuccessStatusCode)
                {
                    throw new KvasirException(
                        @"Failed to reach SCRYFALL.com when trying to fetch cards! " +
                        $"Card Set: [{cardSet.Name}]. " +
                        $"Status Code: [{response.StatusCode}].");
                }

                var contentToken = JObject.Parse(await response.Content.ReadAsStringAsync());

                var cardTokens = contentToken
                    .SelectToken("$.data")?
                    .Children()
                    .Where(token => token["lang"]?.Value<string>() == "en")
                    .ToArray();

                if (cardTokens?.Any() != true)
                {
                    throw new KvasirException(
                        @"Failed to find en-us cards! " +
                        $"Card Set: [{cardSet.Name}].");
                }

                cardTokens
                    .Select(token => new UnparsedBlob.Card
                    {
                        MultiverseId = token
                            .SelectToken("multiverse_ids")?
                            .Children()
                            .FirstOrDefault()?
                            .Value<int>() ?? 0,
                        ScryfallId = token.ReadValue("id"),
                        ScryfallImageUrl = ScryfallFetcher.FindScryfallImageUrl(token),
                        SetCode = cardSet.Code,
                        Name = token.ReadValue("name"),
                        ManaCost = token.ReadValue("mana_cost"),
                        Type = token.ReadValue("type_line"),
                        Rarity = token.ReadValue("rarity"),
                        Text = token.ReadValue("oracle_text"),
                        FlavorText = token.ReadValue("flavor_text"),
                        Power = token.ReadValue("power"),
                        Toughness = token.ReadValue("toughness"),
                        Number = token.ReadValue("collector_number"),
                        Artist = token.ReadValue("artist")
                    })
                    .ForEach(cards.Add);

                hasAnotherPage = contentToken
                    .SelectToken("$.has_more")?
                    .Value<bool>() ?? false;

                if (hasAnotherPage)
                {
                    pageNumber++;
                }
            }

            return cards;
        }

        protected override async Task<IImage> FetchCardImageCoreAsync(UnparsedBlob.Card card)
        {
            var path = $"cards/border_crop/{card.ScryfallImageUrl}";

            var response = await this.HttpClient.GetAsync(new Uri(Link.ImageUri, path));

            if (!response.IsSuccessStatusCode)
            {
                throw new KvasirException(
                    @"Failed to reach SCRYFALL.com when trying to fetch card image! " +
                    $"Card: [{card.Name}]. " +
                    $"Status Code: [{response.StatusCode}].");
            }

            var cardImage = new WritableImage();
            cardImage.LoadData(await response.Content.ReadAsStreamAsync());

            return cardImage;
        }

        private static string FindScryfallImageUrl(JToken token)
        {
            Guard
                .Require(token, nameof(token))
                .Is.Not.Null();

            var imageUrl = token
                .SelectToken("image_uris")?
                .SelectToken("border_crop")?
                .Value<string>();

            if (string.IsNullOrEmpty(imageUrl))
            {
                // TODO: Handle second card face!

                imageUrl = token
                    .SelectToken("card_faces")?
                    .First()
                    .SelectToken("image_uris")?
                    .SelectToken("border_crop")?
                    .Value<string>();
            }

            if (string.IsNullOrEmpty(imageUrl))
            {
                throw new KvasirException("Failed to find image URL!");
            }

            var urlMatch = Pattern.CardImageUrl.Match(imageUrl);

            if (!urlMatch.Success)
            {
                throw new KvasirException(
                    @"Failed to process image URL! " +
                    $"URL: [{imageUrl}].");
            }

            return urlMatch.Groups["url"].Value;
        }

        private static class Link
        {
            public static readonly Uri ApiUri = new("https://api.scryfall.com");

            public static readonly Uri ImageUri = new("https://img.scryfall.com/");
        }

        private sealed class KeyCalculator : IKeyCalculator
        {
            private KeyCalculator()
            {
            }

            public static KeyCalculator Instance { get; } = new();

            public DataSpec Calculate(Uri uri)
            {
                if (uri.PathAndQuery.EndsWith("sets"))
                {
                    return new DataSpec("sets", Mime.Json);
                }

                var urlMatch = Pattern.CardSetPagingUrl.Match(uri.PathAndQuery);

                if (urlMatch.Success)
                {
                    var code = urlMatch.Groups["code"].Value.ToUpperInvariant();
                    var page = int.Parse(urlMatch.Groups["page"].Value);

                    return new DataSpec($"{code}_{page:D2}", Mime.Json);
                }

                urlMatch = Pattern.CardImageUrl.Match(uri.PathAndQuery);

                if (urlMatch.Success)
                {
                    return new DataSpec(
                        $"{urlMatch.Groups["salt"].Value}_{urlMatch.Groups["url"].Value.CalculateMd5Hash()}",
                        Mime.Jpeg);
                }

                throw new KvasirException($"Failed to calculate unique key for URL [{uri}].");
            }
        }

        private static class Pattern
        {
            public static readonly Regex CardSetPagingUrl = new(
                @"/cards/search\?q=e%3a(?<code>\w+)&unique=prints&order=name&page=(?<page>\d+)",
                RegexOptions.Compiled);

            public static readonly Regex CardImageUrl = new(
                @"/(scryfall\-)?cards/border_crop/(?<url>[a-z0-9/]+/[a-z0-9\-%]+\.jpg\?(?<salt>\d+))",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
    }
}