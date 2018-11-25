// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StubHttpMessageHandler.cs" company="nGratis">
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
// <creation_timestamp>Thursday, 8 November 2018 10:39:54 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Core;
    using nGratis.Cop.Core.Contract;

    internal class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly IDictionary<Uri, int> _invocationCountLookup;

        private readonly IDictionary<Uri, HttpResponseMessage> _responseMessageLookup;

        private StubHttpMessageHandler()
        {
            this._invocationCountLookup = new Dictionary<Uri, int>();

            this._responseMessageLookup = new Dictionary<Uri, HttpResponseMessage>();
        }

        public static StubHttpMessageHandler Create()
        {
            return new StubHttpMessageHandler();
        }

        public StubHttpMessageHandler WithSuccessfulResponse(string targetUrl, string content)
        {
            Guard
                .Require(targetUrl, nameof(targetUrl))
                .Is.Not.Empty();

            Guard
                .Require(content, nameof(content))
                .Is.Not.Empty();

            var targetUri = new Uri(targetUrl);

            Guard
                .Require(targetUri, nameof(targetUri))
                .Is.Url();

            if (this._responseMessageLookup.ContainsKey(targetUri))
            {
                throw new KvasirTestingException($"Target URL [{targetUri}] must be registered exactly once!");
            }

            this._responseMessageLookup[targetUri] = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content)
            };

            return this;
        }

        public StubHttpMessageHandler WithSuccessfulResponseInSession(string targetUrl, string name)
        {
            Guard
                .Require(targetUrl, nameof(targetUrl))
                .Is.Not.Empty();

            Guard
                .Require(name, nameof(name))
                .Is.Not.Empty();

            var targetUri = new Uri(targetUrl);

            Guard
                .Require(targetUri, nameof(targetUri))
                .Is.Url();

            if (this._responseMessageLookup.ContainsKey(targetUri))
            {
                throw new KvasirTestingException($"Target URL [{targetUri}] must be registered exactly once!");
            }

            var archiveStream = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream($"nGratis.AI.Kvasir.Core.Test.Session.{name}.ngts");

            if (archiveStream == null)
            {
                throw new KvasirTestingException($"Session [{name}] must be embedded!");
            }

            using (archiveStream)
            using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read))
            {
                var entryKey = targetUri.Segments.Last();

                var foundEntry = archive
                    .Entries
                    .SingleOrDefault(entry => entry.Name == entryKey);

                if (foundEntry == null)
                {
                    throw new KvasirTestingException($"Response for [{entryKey}] is missing from [{name}]!");
                }

                using (var entryStream = foundEntry.Open())
                {
                    this._responseMessageLookup[targetUri] = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(entryStream.AsString())
                    };
                }
            }

            return this;
        }

        public StubHttpMessageHandler WithResponse(string targetUrl, HttpStatusCode statusCode, string content = null)
        {
            Guard
                .Require(targetUrl, nameof(targetUrl))
                .Is.Not.Empty();

            var targetUri = new Uri(targetUrl);

            Guard
                .Require(targetUri, nameof(targetUri))
                .Is.Url();

            if (this._responseMessageLookup.ContainsKey(targetUri))
            {
                throw new KvasirTestingException($"Target URL [{targetUri}] must be registered exactly once!");
            }

            this._responseMessageLookup[targetUri] = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content ?? Text.Empty)
            };

            return this;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage requestMessage,
            CancellationToken cancellationToken)
        {
            Guard
                .Require(requestMessage, nameof(requestMessage))
                .Is.Not.Null();

            var targetUri = requestMessage.RequestUri;

            if (this._invocationCountLookup.ContainsKey(targetUri))
            {
                this._invocationCountLookup[targetUri]++;
            }
            else
            {
                this._invocationCountLookup[targetUri] = 1;
            }

            if (this._responseMessageLookup.TryGetValue(requestMessage.RequestUri, out var responseMessage))
            {
                return await Task.FromResult(responseMessage);
            }

            return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }

        public void VerifyInvoked(string targetUrl, int expectedCount)
        {
            Guard
                .Require(targetUrl, nameof(targetUrl))
                .Is.Not.Empty();

            Guard
                .Require(expectedCount, nameof(expectedCount))
                .Is.ZeroOrPositive();

            var targetUri = new Uri(targetUrl);

            Guard
                .Require(targetUri, nameof(targetUri))
                .Is.Url();

            var isValid = this._invocationCountLookup.TryGetValue(targetUri, out var count)
                ? expectedCount == count
                : count <= 0;

            if (!isValid)
            {
                throw new KvasirTestingException(
                    $"Verification failed because URL [{targetUri}] is invoked [{count}] time(s), " +
                    $"but expected to be invoked [{expectedCount}] time(s)!");
            }
        }
    }
}