// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StubHttpMessageHandler.cs" company="nGratis">
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
// <creation_timestamp>Thursday, 8 November 2018 10:39:54 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Framework;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly IDictionary<Uri, StubInfo> _stubInfoByUriLookup;

    private StubHttpMessageHandler()
    {
        this._stubInfoByUriLookup = new Dictionary<Uri, StubInfo>();
    }

    public static StubHttpMessageHandler Create()
    {
        return new();
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

        if (this._stubInfoByUriLookup.ContainsKey(targetUri))
        {
            throw new KvasirTestingException($"Target URI [{targetUri}] must be registered exactly once!");
        }

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(content)
        };

        this._stubInfoByUriLookup[targetUri] = new StubInfo(targetUri, response);

        return this;
    }

    public StubHttpMessageHandler WithSuccessfulScryfallResponse(string name)
    {
        Guard
            .Require(name, nameof(name))
            .Is.Not.Empty();

        using (var sessionStream = name.FetchSessionStream())
        using (var sessionArchive = new ZipArchive(sessionStream, ZipArchiveMode.Read))
        {
            sessionArchive
                .Entries
                .Select(entry => new
                {
                    Entry = entry,
                    Match = Pattern.CardEntry.Match(entry.Name)
                })
                .Where(anon => anon.Match.Success)
                .Select(anon => new
                {
                    TargetUri = new Uri(
                        @"https://api.scryfall.com/cards/search?" +
                        $"q=e%3a{anon.Match.Groups["code"].Value}&" +
                        @"unique=prints&" +
                        @"order=name&" +
                        $"page={int.Parse(anon.Match.Groups["page"].Value)}"),
                    ResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = StubHttpMessageHandler.CreateHttpContent(anon.Entry)
                    }
                })
                .Select(anon => new StubInfo(anon.TargetUri, anon.ResponseMessage))
                .ForEach(info => this._stubInfoByUriLookup[info.TargetUri] = info);
        }

        return this
            .WithSuccessfulResponseInSession("https://api.scryfall.com/sets", name, "sets.json");
    }

    public StubHttpMessageHandler WithSuccessfulResponseInSession(
        string targetUrl,
        string name,
        string entryKey = "")
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

        if (this._stubInfoByUriLookup.ContainsKey(targetUri))
        {
            throw new KvasirTestingException($"Target URL [{targetUri}] must be registered exactly once!");
        }

        using (var sessionStream = name.FetchSessionStream())
        using (var sessionArchive = new ZipArchive(sessionStream, ZipArchiveMode.Read))
        {
            if (string.IsNullOrEmpty(entryKey))
            {
                entryKey = targetUri.Segments.Last();
            }

            var matchedEntry = sessionArchive
                .Entries
                .SingleOrDefault(entry => entry.Name == entryKey);

            if (matchedEntry == null)
            {
                var entryKeys = targetUri
                    .Query[1..]
                    .Split('&')
                    .Select(parameter => parameter.Split('=')[1])
                    .ToArray();

                matchedEntry = sessionArchive
                    .Entries
                    .SingleOrDefault(entry => entryKeys.Contains(entry.Name));
            }

            if (matchedEntry == null)
            {
                throw new KvasirTestingException($"Response for [{entryKey}] is missing from [{name}]!");
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = StubHttpMessageHandler.CreateHttpContent(matchedEntry)
            };

            this._stubInfoByUriLookup[targetUri] = new StubInfo(targetUri, response);
        }

        return this;
    }

    public StubHttpMessageHandler WithResponse(string targetUrl, HttpStatusCode statusCode, string content = "")
    {
        Guard
            .Require(targetUrl, nameof(targetUrl))
            .Is.Not.Empty();

        var targetUri = new Uri(targetUrl);

        Guard
            .Require(targetUri, nameof(targetUri))
            .Is.Url();

        if (this._stubInfoByUriLookup.ContainsKey(targetUri))
        {
            throw new KvasirTestingException($"Target URL [{targetUri}] must be registered exactly once!");
        }

        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content)
        };

        this._stubInfoByUriLookup[targetUri] = new StubInfo(targetUri, response);

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
            this._stubInfoByUriLookup.TryGetValue(targetUri, out var stubInfo) &&
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
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var targetUri = request.RequestUri;

        if (targetUri == null)
        {
            throw new KvasirException("Request message must have valid URI!");
        }

        if (!this._stubInfoByUriLookup.TryGetValue(targetUri, out var stubInfo))
        {
            stubInfo = new StubInfo(targetUri, new HttpResponseMessage(HttpStatusCode.NotFound));
        }

        stubInfo.InvocationCount++;

        return await Task.FromResult(stubInfo.Response);
    }

    private static HttpContent CreateHttpContent(ZipArchiveEntry archiveEntry)
    {
        using var entryStream = archiveEntry.Open();

        return Path.GetExtension(archiveEntry.Name) == Mime.Jpeg.FileExtension
            ? new ByteArrayContent(entryStream.ReadBlob())
            : new StringContent(entryStream.ReadText(Encoding.UTF8));
    }

    private sealed class StubInfo
    {
        public StubInfo(Uri targetUri, HttpResponseMessage response)
        {
            Guard
                .Require(targetUri, nameof(targetUri))
                .Is.Url();

            this.TargetUri = targetUri;
            this.Response = response;
            this.InvocationCount = 0;
        }

        public Uri TargetUri { get; }

        public HttpResponseMessage Response { get; }

        public int InvocationCount { get; set; }
    }

    private static class Pattern
    {
        public static readonly Regex CardEntry = new(
            @"(?<code>[A-Z0-9]{3})_(?<page>\d{2})\.json",
            RegexOptions.Compiled);
    }
}