// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StubHttpMessageHandler.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, 8 November 2018 10:39:54 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Framework;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using nGratis.Cop.Olympus.Contract;

public class StubHttpMessageHandler : Cop.Olympus.Framework.StubHttpMessageHandler
{
    private StubHttpMessageHandler()
        : base(Assembly.GetExecutingAssembly())
    {
    }

    public new static StubHttpMessageHandler Create()
    {
        return new StubHttpMessageHandler();
    }

    public StubHttpMessageHandler WithSuccessfulScryfallResponseInSession(string name)
    {
        Guard
            .Require(name, nameof(name))
            .Is.Not.Empty();

        using (var sessionStream = this.TargetAssembly.FetchSessionStream(name, OlympusMime.Session))
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
                .ForEach(info => this.StubInfoByUriLookup[info.TargetUri] = info);
        }

        this.WithSuccessfulResponseInSession("https://api.scryfall.com/sets", name, "sets.json");

        return this;
    }

    private static class Pattern
    {
        public static readonly Regex CardEntry = new(
            @"(?<code>[A-Z0-9]{3})_(?<page>\d{2})\.json",
            RegexOptions.Compiled);
    }
}