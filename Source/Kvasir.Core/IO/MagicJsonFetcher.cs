// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicJsonFetcher.cs" company="nGratis">
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
// <creation_timestamp>Thursday, 25 October 2018 10:50:04 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;
    using HtmlAgilityPack;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Contract.Magic;

    public class MagicJsonFetcher : IMagicFetcher
    {
        private static readonly Uri LandingUrl = new Uri("https://mtgjson.com/v4/");

        private readonly HttpClient _httpClient;

        public MagicJsonFetcher()
            : this(null)
        {
        }

        internal MagicJsonFetcher(HttpMessageHandler messageHandler)
        {
            this._httpClient = messageHandler != null
                ? new HttpClient(messageHandler)
                : new HttpClient();

            this._httpClient.BaseAddress = MagicJsonFetcher.LandingUrl;
            this._httpClient.Timeout = TimeSpan.FromSeconds(5);

            this._httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
            this._httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            this._httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AI.Kvasir", "0.1"));
        }

        public async Task<IReadOnlyCollection<CardSet>> GetCardSetsAsync()
        {
            var response = await this._httpClient.GetAsync("sets.html");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                var document = new HtmlDocument();
                document.LoadHtml(content);

                return document
                    .DocumentNode
                    .SelectNodes("//table//tbody//tr//td")
                    .Where(node => node.ChildNodes.Any())
                    .Select(MagicJsonFetcher.ConvertToCardSet)
                    .ToArray();
            }
            else
            {
                throw new KvasirException(
                    @"Failed to reach MTGJSON4.com when trying to fetch card sets! " +
                    $"Status Code: [{response.StatusCode}].");
            }
        }

        private static CardSet ConvertToCardSet(HtmlNode rootNode)
        {
            var nameNode = rootNode
                .ChildNodes
                .SingleOrDefault(node => node.Name == "strong");

            if (nameNode == null)
            {
                throw new KvasirException("Card set name is not found!");
            }

            var foundMatch = rootNode
                .ChildNodes
                .Where(node => node.Name == "#text")
                .Select(node => Pattern.CodeWithReleasedTimestamp.Match(node.InnerText))
                .SingleOrDefault(match => match.Success);

            if (foundMatch == null)
            {
                throw new KvasirException("Card set code and released timestamp are not found!");
            }

            var releasedTimestamp = DateTime.MaxValue;

            if (foundMatch.Groups["timestamp"].Success)
            {
                releasedTimestamp = DateTime.ParseExact(
                    foundMatch.Groups["timestamp"].Value,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal);
            }

            return new CardSet
            {
                Name = HttpUtility.HtmlDecode(nameNode.InnerText),
                Code = foundMatch.Groups["code"].Value,
                ReleasedTimestamp = releasedTimestamp
            };
        }

        private static class Pattern
        {
            public static readonly Regex CodeWithReleasedTimestamp = new Regex(
                @"^(?<code>\w{3,6})( — (?<timestamp>\d{4}-\d{2}-\d{2}))?$",
                RegexOptions.Compiled);
        }
    }
}