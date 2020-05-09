// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StubHttpMessageHandler.cs" company="nGratis">
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
// <creation_timestamp>Thursday, 8 November 2018 10:39:54 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Olympus.Contract;

    public class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly IDictionary<Uri, StubInfo> _infoLookup;

        private StubHttpMessageHandler()
        {
            this._infoLookup = new Dictionary<Uri, StubInfo>();
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

            if (this._infoLookup.ContainsKey(targetUri))
            {
                throw new KvasirTestingException($"Target URL [{targetUri}] must be registered exactly once!");
            }

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content)
            };

            this._infoLookup[targetUri] = new StubInfo(targetUri, responseMessage);

            return this;
        }

        public StubHttpMessageHandler WithSuccessfulResponseInSession(
            string targetUrl,
            string name,
            string entryKey = null)
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

            if (this._infoLookup.ContainsKey(targetUri))
            {
                throw new KvasirTestingException($"Target URL [{targetUri}] must be registered exactly once!");
            }

            var archiveStream = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream($"nGratis.AI.Kvasir.Framework.Session.{name}.ngts");

            if (archiveStream == null)
            {
                throw new KvasirTestingException($"Session [{name}] must be embedded!");
            }

            using (archiveStream)
            using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read))
            {
                if (string.IsNullOrEmpty(entryKey))
                {
                    entryKey = targetUri.Segments.Last();
                }

                var matchedEntry = archive
                    .Entries
                    .SingleOrDefault(entry => entry.Name == entryKey);

                if (matchedEntry == null)
                {
                    var entryKeys = targetUri
                        .Query
                        .Substring(1)
                        .Split('&')
                        .Select(parameter => parameter.Split('=')[1])
                        .ToArray();

                    matchedEntry = archive
                        .Entries
                        .SingleOrDefault(entry => entryKeys.Contains(entry.Name));
                }

                if (matchedEntry == null)
                {
                    throw new KvasirTestingException($"Response for [{entryKey}] is missing from [{name}]!");
                }

                var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = StubHttpMessageHandler.CreateHttpContent(matchedEntry)
                };

                this._infoLookup[targetUri] = new StubInfo(targetUri, responseMessage);
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

            if (this._infoLookup.ContainsKey(targetUri))
            {
                throw new KvasirTestingException($"Target URL [{targetUri}] must be registered exactly once!");
            }

            var responseMessage = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content ?? Text.Empty)
            };

            this._infoLookup[targetUri] = new StubInfo(targetUri, responseMessage);

            return this;
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

            var isValid =
                this._infoLookup.TryGetValue(targetUri, out var stubInfo) &&
                stubInfo.InvocationCount == expectedCount;

            if (!isValid)
            {
                throw new KvasirTestingException(
                    $"Verification failed because URL [{targetUri}] " +
                    $"is invoked [{stubInfo?.InvocationCount ?? 0}] time(s), " +
                    $"but expected to be invoked [{expectedCount}] time(s)!");
            }
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage requestMessage,
            CancellationToken cancellationToken)
        {
            Guard
                .Require(requestMessage, nameof(requestMessage))
                .Is.Not.Null();

            var targetUri = requestMessage.RequestUri;

            if (!this._infoLookup.TryGetValue(targetUri, out var stubInfo))
            {
                stubInfo = new StubInfo(targetUri, new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            stubInfo.InvocationCount++;

            return await Task.FromResult(stubInfo.ResponseMessage);
        }

        private static HttpContent CreateHttpContent(ZipArchiveEntry archiveEntry)
        {
            using (var entryStream = archiveEntry.Open())
            {
                if (Path.GetExtension(archiveEntry.Name) == Mime.Jpeg.FileExtension)
                {
                    return new ByteArrayContent(entryStream.ReadBlob());
                }

                return new StringContent(entryStream.ReadText(Encoding.UTF8));
            }
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        private sealed class StubInfo
        {
            public StubInfo(Uri targetUri, HttpResponseMessage responseMessage)
            {
                Guard
                    .Require(targetUri, nameof(targetUri))
                    .Is.Not.Null()
                    .Is.Url();

                Guard
                    .Require(responseMessage, nameof(responseMessage))
                    .Is.Not.Null();

                this.TargetUri = targetUri;
                this.ResponseMessage = responseMessage;
                this.InvocationCount = 0;
            }

            public Uri TargetUri { get; }

            public HttpResponseMessage ResponseMessage { get; }

            public int InvocationCount { get; set; }
        }
    }
}