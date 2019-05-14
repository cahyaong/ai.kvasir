// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScryfallFetcher.cs" company="nGratis">
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
    using nGratis.Cop.Core.Contract;
    using nGratis.Cop.Core.Vision.Imaging;

    public class ScryfallFetcher : BaseMagicHttpFetcher
    {
        private static readonly Uri LandingUri = new Uri("https://api.scryfall.com");

        public ScryfallFetcher(IStorageManager storageManager)
            : base("SCRYFALL", ScryfallFetcher.LandingUri, storageManager, UniqueKeyCalculator.Instance)
        {
        }

        internal ScryfallFetcher(HttpMessageHandler messageHandler)
            : base(ScryfallFetcher.LandingUri, messageHandler)
        {
        }

        public override ExternalResources AvailableResources => ExternalResources.All;

        protected override async Task<IReadOnlyCollection<RawCardSet>> GetCardSetsCoreAsync()
        {
            var response = await this.HttpClient.GetAsync("sets");

            if (!response.IsSuccessStatusCode)
            {
                throw new KvasirException(
                    @"Failed to reach SCRYFALL.com when trying to fetch card sets! " +
                    $"Status Code: [{response.StatusCode}].");
            }

            var contentToken = JObject.Parse(await response.Content.ReadAsStringAsync());

            return contentToken
                .SelectToken("$.data")
                .Children()
                .Select(token => new RawCardSet
                {
                    Code = token["code"].Value<string>().ToUpperInvariant(),
                    Name = token["name"].Value<string>(),
                    ReleasedTimestamp = token["released_at"].Value<DateTime>()
                })
                .ToArray();
        }

        protected override async Task<IReadOnlyCollection<RawCard>> GetCardsCoreAsync(RawCardSet cardSet)
        {
            var rawCards = new List<RawCard>();
            var pageCount = 1;
            var hasAnotherPage = true;

            while (hasAnotherPage)
            {
                var parameters = HttpUtility.ParseQueryString(string.Empty);

                parameters["q"] = $"e:{cardSet.Code}";
                parameters["unique"] = "prints";
                parameters["order"] = "name";
                parameters["page"] = pageCount.ToString();

                var response = await this.HttpClient.GetAsync($"cards/search?{parameters}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new KvasirException(
                        @"Failed to reach SCRYFALL.com when trying to fetch cards! " +
                        $"Card Set: [{cardSet.Name}]. " +
                        $"Status Code: [{response.StatusCode}].");
                }

                var contentToken = JObject.Parse(await response.Content.ReadAsStringAsync());

                contentToken
                    .SelectToken("$.data")
                    .Children()
                    .Where(token => token["lang"]?.Value<string>() == "en")
                    .Select(token => new RawCard
                    {
                        MultiverseId = token
                            .SelectToken("multiverse_ids")?
                            .Children()
                            .FirstOrDefault()?
                            .Value<int>() ?? 0,
                        CardSetCode = cardSet.Code,
                        Name = token["name"]?.Value<string>() ?? string.Empty,
                        ManaCost = token["mana_cost"]?.Value<string>() ?? string.Empty,
                        Type = token["type_line"]?.Value<string>() ?? string.Empty,
                        Rarity = token["rarity"]?.Value<string>(),
                        Text = token["oracle_text"]?.Value<string>() ?? string.Empty,
                        FlavorText = token["flavor_text"]?.Value<string>() ?? string.Empty,
                        Power = token["power"]?.Value<string>() ?? string.Empty,
                        Toughness = token["toughness"]?.Value<string>() ?? string.Empty,
                        Number = token["collector_number"]?.Value<string>() ?? string.Empty,
                        Artist = token["artist"]?.Value<string>() ?? string.Empty
                    })
                    .ForEach(rawCards.Add);

                hasAnotherPage = contentToken
                    .SelectToken("$.has_more")
                    .Value<bool>();

                if (hasAnotherPage)
                {
                    pageCount++;
                }
            }

            return rawCards;
        }

        protected override async Task<IImage> GetCardImageCoreAsync(RawCard card)
        {
            return await Task.FromResult(EmptyImage.Instance);
        }

        private sealed class UniqueKeyCalculator : IUniqueKeyCalculator
        {
            private UniqueKeyCalculator()
            {
            }

            public static UniqueKeyCalculator Instance { get; } = new UniqueKeyCalculator();

            public string Calculate(Uri uri)
            {
                if (uri.PathAndQuery.EndsWith("sets"))
                {
                    return $"sets{Mime.Json.FileExtension}";
                }

                var match = Pattern.CardSetPaging.Match(uri.PathAndQuery);

                if (match.Success)
                {
                    var code = match.Groups["code"].Value.ToUpperInvariant();
                    var page = int.Parse(match.Groups["page"].Value);

                    return $"{code}_{page:D2}{Mime.Json.FileExtension}";
                }

                throw new KvasirException($"Failed to calculate unique key for URI [{uri}].");
            }
        }

        private static class Pattern
        {
            public static readonly Regex CardSetPaging = new Regex(
                @"/cards/search\?q=e%3a(?<code>\w+)&unique=prints&order=name&page=(?<page>\d+)",
                RegexOptions.Compiled);
        }
    }
}