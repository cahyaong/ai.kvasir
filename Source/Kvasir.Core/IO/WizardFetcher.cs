// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WizardFetcher.cs" company="nGratis">
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
// <creation_timestamp>Monday, 17 December 2018 8:37:05 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using HtmlAgilityPack;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Core.Contract;
    using nGratis.Cop.Core.Vision.Imaging;

    public class WizardFetcher : BaseMagicHttpFetcher
    {
        public override ExternalResources AvailableResources =>
            ExternalResources.Rule;

        public WizardFetcher(IStorageManager storageManager)
            : base("WOTC", storageManager, KeyCalculator.Instance)
        {
        }

        internal WizardFetcher(HttpMessageHandler messageHandler)
            : base(messageHandler)
        {
        }

        protected override async Task<IImage> FetchCardImageCoreAsync(UnparsedBlob.Card card)
        {
            var imageUri = new Uri(
                Link.GathererUri,
                $"Handlers/Image.ashx?multiverseid={card.MultiverseId}&type=card");

            var response = await this.HttpClient.GetAsync(imageUri);

            if (!response.IsSuccessStatusCode)
            {
                throw new KvasirException(
                    @"Failed to reach WIZARD.com when trying to fetch card image! " +
                    $"Card: [{card.Name}]. " +
                    $"Status Code: [{response.StatusCode}].");
            }

            var cardImage = new WritableImage();
            cardImage.LoadData(await response.Content.ReadAsStreamAsync());

            return cardImage;
        }

        protected override async Task<IReadOnlyCollection<UnparsedBlob.Rule>> FetchRulesCoreAsync()
        {
            var landingUri = new Uri(
                Link.MagicUri,
                "en/game-info/gameplay/rules-and-formats/rules");

            var response = await this.HttpClient.GetAsync(landingUri);

            if (!response.IsSuccessStatusCode)
            {
                throw new KvasirException("Failed to reach WIZARD.com when trying to fetch rules!");
            }

            var landingDocument = new HtmlDocument();
            landingDocument.LoadHtml(await response.Content.ReadAsStringAsync());

            var textNode = landingDocument
                .DocumentNode
                .SelectNodes("//a[@class='cta']")
                .Single(node => node.InnerText.Contains("TXT"));

            var textUri = new Uri(textNode.Attributes["href"].Value);

            response = await this.HttpClient.GetAsync(textUri);

            if (!response.IsSuccessStatusCode)
            {
                throw new KvasirException("Failed to reach WIZARD.com when trying to fetch rules TXT!");
            }

            var ruleContent = await response.Content.ReadAsStringAsync();

            var startingIndex = ruleContent.IndexOf("Credits", StringComparison.InvariantCulture);
            var endingIndex = ruleContent.LastIndexOf("Glossary", StringComparison.InvariantCulture);

            var rules = ruleContent
                .Substring(startingIndex, endingIndex - startingIndex)
                .Split(
                    new[]
                    {
                        $"{Environment.NewLine}{Environment.NewLine}",
                        $"{Environment.NewLine} {Environment.NewLine}"
                    },
                    StringSplitOptions.RemoveEmptyEntries)
                .Select(token => token
                    .Replace(Environment.NewLine, " ")
                    .Trim())
                .Select(token => Pattern.Rule.Match(token))
                .Where(match => match.Success)
                .Select(match => new UnparsedBlob.Rule
                {
                    Id = match.Groups["id"].Value,
                    Text = match.Groups["text"].Value
                })
                .ToArray();

            return rules;
        }

        private static class Link
        {
            public static readonly Uri MagicUri = new Uri("https://magic.wizards.com");

            public static readonly Uri GathererUri = new Uri("http://gatherer.wizards.com");
        }

        private sealed class KeyCalculator : IKeyCalculator
        {
            private KeyCalculator()
            {
            }

            public static KeyCalculator Instance { get; } = new KeyCalculator();

            public DataSpec Calculate(Uri uri)
            {
                var imageUrlMatch = Pattern.CardImageUrl.Match(uri.PathAndQuery);

                if (imageUrlMatch.Success)
                {
                    return new DataSpec(imageUrlMatch.Groups["id"].Value, Mime.Png);
                }

                var nameTokens = Path
                    .GetFileName(uri.LocalPath)
                    .Split('.');

                var isText =
                    nameTokens.Length == 2 &&
                    Mime.Text.Extensions.Contains(nameTokens.Last());

                return isText
                    ? new DataSpec(
                        Regex.Replace(nameTokens.First(), @"\s+", "_"),
                        Mime.Text)
                    : DataSpec.None;
            }
        }

        private static class Pattern
        {
            public static readonly Regex CardImageUrl = new Regex(
                @"Image\.ashx\?multiverseid=(?<id>\d+)&type=card",
                RegexOptions.Compiled);

            public static readonly Regex Rule = new Regex(
                @"^(?<id>\d{1,3}(\.\d{1,3}[a-z]?)?)\.? (?<text>.+)$",
                RegexOptions.Compiled);
        }
    }
}