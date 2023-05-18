// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicJsonFetcher.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, 25 October 2018 10:50:04 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public class MagicJsonFetcher : MagicHttpFetcherBase
{
    public MagicJsonFetcher(IStorageManager storageManager)
        : base("MTGJSON4", storageManager, SimpleKeyCalculator.Instance)
    {
    }

    internal MagicJsonFetcher(HttpMessageHandler messageHandler)
        : base(messageHandler)
    {
    }

    public override ExternalResources AvailableResources => ExternalResources.CardSet | ExternalResources.Card;

    protected override async Task<IReadOnlyCollection<UnparsedBlob.CardSet>> FetchCardSetsCoreAsync()
    {
        var response = await this.HttpClient.GetAsync(new Uri(Link.LandingUri, "sets.html"));

        if (!response.IsSuccessStatusCode)
        {
            throw new KvasirException(
                @"Failed to reach MTGJSON4.com when trying to fetch card sets! " +
                $"Status Code: [{response.StatusCode}].");
        }

        var content = await response.Content.ReadAsStringAsync();

        var document = new HtmlDocument();
        document.LoadHtml(content);

        return document
            .DocumentNode
            .SelectNodes("//table//tbody//tr//td")
            .Where(node => node.ChildNodes.Count > 1)
            .Select(MagicJsonFetcher.ConvertToCardSet)
            .ToImmutableArray();
    }

    protected override async Task<IReadOnlyCollection<UnparsedBlob.Card>> FetchCardsCoreAsync(
        UnparsedBlob.CardSet cardSet)
    {
        var response = await this.HttpClient.GetAsync(new Uri(Link.LandingUri, $"json/{cardSet.Code}.json"));

        if (!response.IsSuccessStatusCode)
        {
            throw new KvasirException(
                @"Failed to reach MTGJSON4.com when trying to fetch cards! " +
                $"Card Set: [{cardSet.Name}]. " +
                $"Status Code: [{response.StatusCode}].");
        }

        var cardsToken = JObject
            .Parse(await response.Content.ReadAsStringAsync())
            .SelectToken("cards");

        if (cardsToken == null)
        {
            throw new KvasirException("Response from MTGJSON4.com is missing cards!");
        }

        return cardsToken
            .Children()
            .Select(cardToken =>
            {
                // TODO (COULD): Add logging when getting a <null> card!

                cardToken[nameof(UnparsedBlob.Card.SetCode)] = cardSet.Code;

                return cardToken.ToObject<UnparsedBlob.Card>();
            })
            .Where(card => card != null)
            .ToImmutableArray();
    }

    private static UnparsedBlob.CardSet ConvertToCardSet(HtmlNode rootNode)
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
            .Select(node => Pattern.SetCodeWithReleasedTimestamp.Match(node.InnerText))
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

        return new UnparsedBlob.CardSet
        {
            Name = HttpUtility.HtmlDecode(nameNode.InnerText),
            Code = foundMatch.Groups["set_code"].Value,
            ReleasedTimestamp = releasedTimestamp
        };
    }

    private static class Link
    {
        public static readonly Uri LandingUri = new("https://mtgjson.com");
    }

    private static class Pattern
    {
        public static readonly Regex SetCodeWithReleasedTimestamp = new(
            @"^(?<set_code>\w{2,6})( — (?<timestamp>\d{4}-\d{2}-\d{2}))?$",
            RegexOptions.Compiled);
    }
}