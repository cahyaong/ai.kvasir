﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StubMagicHttpFetcher.cs" company="nGratis">
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
// <creation_timestamp>Monday, 17 December 2018 1:28:17 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Core.Contract;
    using nGratis.Cop.Core.Vision.Imaging;

    public class StubMagicHttpFetcher : BaseMagicHttpFetcher
    {
        private ExternalResources _availableResources;

        private StubMagicHttpFetcher(HttpMessageHandler messageHandler)
            : base(messageHandler)
        {
        }

        public static StubMagicHttpFetcher Create(HttpMessageHandler messageHandler)
        {
            return new StubMagicHttpFetcher(messageHandler);
        }

        public override ExternalResources AvailableResources => this._availableResources;

        public StubMagicHttpFetcher WithAvailableResources(ExternalResources availableResources)
        {
            Guard
                .Require(availableResources, nameof(availableResources))
                .Is.Not.Default();

            this._availableResources |= availableResources;

            return this;
        }

        protected override async Task<IReadOnlyCollection<RawCardSet>> FetchCardSetsCoreAsync()
        {
            var cardSet = new RawCardSet
            {
                Code = "[_MOCK_CODE_]",
                Name = "[_MOCK_NAME_]",
                ReleasedTimestamp = Constant.EpochTimestamp
            };

            return await Task.FromResult(new[] { cardSet });
        }

        protected override async Task<IReadOnlyCollection<RawCard>> FetchCardsCoreAsync(RawCardSet rawCardSet)
        {
            var card = new RawCard
            {
                CardSetCode = rawCardSet.Code,
                Name = "[_MOCK_NAME_]"
            };

            return await Task.FromResult(new[] { card });
        }

        protected override async Task<IImage> FetchCardImageCoreAsync(RawCard rawCard)
        {
            return await Task.FromResult(EmptyImage.Instance);
        }

        public void VerifyRequestHeaders(params string[] expectedHeaders)
        {
            Guard
                .Require(expectedHeaders, nameof(expectedHeaders))
                .Is.Not.Empty();

            var actualHeaders = this
                .HttpClient.DefaultRequestHeaders
                .Select(header => $"{header.Key}: {string.Join(", ", header.Value)}")
                .ToArray();

            var isValid =
                actualHeaders.Length == expectedHeaders.Length &&
                !actualHeaders.Except(expectedHeaders).Any();

            string FormatHeaders(IReadOnlyCollection<string> headers)
            {
                return headers?.Any() == true
                    ? string.Join(" AND ", headers.Select(header => $"({header})"))
                    : Text.Unknown;
            }

            if (!isValid)
            {
                throw new KvasirTestingException(
                    @"Verification failed because <HttpClient> " +
                    $"is configured with [{FormatHeaders(actualHeaders)}], " +
                    $"but expected to be configured with [{FormatHeaders(expectedHeaders)}]!");
            }
        }
    }
}