// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WizardFetcher.cs" company="nGratis">
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
// <creation_timestamp>Monday, 17 December 2018 8:37:05 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core
{
    using System;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Core.Contract;
    using nGratis.Cop.Core.Vision.Imaging;

    public class WizardFetcher : BaseMagicHttpFetcher
    {
        public override ExternalResources AvailableResources => ExternalResources.CardImage;

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
            var path =
                @"Handlers/Image.ashx" +
                $"?multiverseid={card.MultiverseId}&type=card";

            var response = await this.HttpClient.GetAsync(new Uri(Link.GathererUri, path));

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

        private static class Link
        {
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
                var match = Pattern.CardImageUrl.Match(uri.PathAndQuery);

                if (!match.Success)
                {
                    throw new KvasirException($"Failed to calculate unique key for URL [{uri}].");
                }

                return new DataSpec(match.Groups["id"].Value, Mime.Png);
            }
        }

        private static class Pattern
        {
            public static readonly Regex CardImageUrl = new Regex(
                @"Image\.ashx\?multiverseid=(?<id>\d+)&type=card",
                RegexOptions.Compiled);
        }
    }
}