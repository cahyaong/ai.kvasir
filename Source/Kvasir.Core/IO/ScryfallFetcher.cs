// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScryfallFetcher.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, 10 May 2019 11:11:57 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public class ScryfallFetcher : MagicHttpFetcherBase
{
    public ScryfallFetcher(IStorageManager storageManager)
        : base("SCRYFALL", storageManager, KeyCalculator.Instance)
    {
    }

    internal ScryfallFetcher(HttpMessageHandler messageHandler)
        : base(messageHandler)
    {
    }

    public override ExternalResources AvailableResources =>
        ExternalResources.CardSet |
        ExternalResources.Card |
        ExternalResources.CardImage;

    protected override async Task<IReadOnlyCollection<UnparsedBlob.CardSet>> FetchCardSetsCoreAsync()
    {
        var response = await this.HttpClient.GetAsync(new Uri(Link.ApiUri, "sets"));

        if (!response.IsSuccessStatusCode)
        {
            throw new KvasirException(
                @"Failed to reach SCRYFALL.com when trying to fetch card sets! " +
                $"Status Code: [{response.StatusCode}].");
        }

        var contentToken = JObject.Parse(await response.Content.ReadAsStringAsync());

        var cardSetTokens = contentToken
            .SelectToken("$.data")?
            .Children()
            .ToArray();

        if (cardSetTokens?.Any() != true)
        {
            throw new KvasirException("Failed to find card sets!");
        }

        return cardSetTokens
            .Select(token => new UnparsedBlob.CardSet
            {
                Code = token.ReadValue("code").ToUpperInvariant(),
                Name = token.ReadValue("name"),
                ReleasedTimestamp = token.ReadValue<DateTime>("released_at")
            })
            .ToImmutableArray();
    }

    protected override async Task<IReadOnlyCollection<UnparsedBlob.Card>> FetchCardsCoreAsync(
        UnparsedBlob.CardSet cardSet)
    {
        var cards = new List<UnparsedBlob.Card>();
        var pageNumber = 1;
        var hasAnotherPage = true;

        while (hasAnotherPage)
        {
            var parameters = HttpUtility.ParseQueryString(string.Empty);

            parameters["q"] = $"e:{cardSet.Code}";
            parameters["unique"] = "prints";
            parameters["order"] = "name";
            parameters["page"] = pageNumber.ToString();

            var response = await this.HttpClient.GetAsync(new Uri(Link.ApiUri, $"cards/search?{parameters}"));

            if (!response.IsSuccessStatusCode)
            {
                throw new KvasirException(
                    @"Failed to reach SCRYFALL.com when trying to fetch cards! " +
                    $"Card Set: [{cardSet.Name}]. " +
                    $"Status Code: [{response.StatusCode}].");
            }

            var contentToken = JObject.Parse(await response.Content.ReadAsStringAsync());

            var cardTokens = contentToken
                .SelectToken("$.data")?
                .Children()
                .Where(token => token["lang"]?.Value<string>() == "en")
                .ToArray();

            if (cardTokens?.Any() != true)
            {
                throw new KvasirException(
                    @"Failed to find en-us cards! " +
                    $"Card Set: [{cardSet.Name}].");
            }

            cardTokens
                .Select(token => new UnparsedBlob.Card
                {
                    MultiverseId = token
                        .SelectToken("multiverse_ids")?
                        .Children()
                        .FirstOrDefault()?
                        .Value<int>() ?? 0,
                    ScryfallId = token.ReadValue("id"),
                    ScryfallImageUrl = ScryfallFetcher.FindScryfallImageUrl(token),
                    SetCode = cardSet.Code,
                    Name = token.ReadValue("name"),
                    ManaCost = token.ReadValue("mana_cost"),
                    Type = token.ReadValue("type_line"),
                    Rarity = token.ReadValue("rarity"),
                    Text = token.ReadValue("oracle_text"),
                    FlavorText = token.ReadValue("flavor_text"),
                    Power = token.ReadValue("power"),
                    Toughness = token.ReadValue("toughness"),
                    Number = token.ReadValue("collector_number"),
                    Artist = token.ReadValue("artist")
                })
                .ForEach(cards.Add);

            hasAnotherPage = contentToken
                .SelectToken("$.has_more")?
                .Value<bool>() ?? false;

            if (hasAnotherPage)
            {
                pageNumber++;
            }
        }

        return cards;
    }

    protected override async Task<IImage> FetchCardImageCoreAsync(UnparsedBlob.Card card)
    {
        var response = await this.HttpClient.GetAsync(new Uri(card.ScryfallImageUrl));

        if (!response.IsSuccessStatusCode)
        {
            throw new KvasirException(
                @"Failed to reach SCRYFALL.com when trying to fetch card image! " +
                $"Card: [{card.Name}]. " +
                $"Status Code: [{response.StatusCode}].");
        }

        var cardImage = new WritableImage();
        cardImage.LoadData(await response.Content.ReadAsStreamAsync());

        return cardImage;
    }

    private static string FindScryfallImageUrl(JToken token)
    {
        var imageUrl = token
            .SelectToken("image_uris")?
            .SelectToken("border_crop")?
            .Value<string>();

        if (string.IsNullOrEmpty(imageUrl))
        {
            // TODO: Handle second card face!

            imageUrl = token
                .SelectToken("card_faces")?
                .First()
                .SelectToken("image_uris")?
                .SelectToken("border_crop")?
                .Value<string>();
        }

        if (string.IsNullOrEmpty(imageUrl))
        {
            throw new KvasirException("Failed to find image URL!");
        }

        var urlMatch = Pattern.CardImageUrl.Match(imageUrl);

        if (!urlMatch.Success)
        {
            throw new KvasirException(
                @"Failed to process image URL! " +
                $"URL: [{imageUrl}].");
        }

        return imageUrl;
    }

    private static class Link
    {
        public static readonly Uri ApiUri = new("https://api.scryfall.com");
    }

    private sealed class KeyCalculator : IKeyCalculator
    {
        private KeyCalculator()
        {
        }

        public static KeyCalculator Instance { get; } = new();

        public DataSpec Calculate(Uri uri)
        {
            if (uri.PathAndQuery.EndsWith("sets"))
            {
                return new DataSpec("sets", Mime.Json);
            }

            var urlMatch = Pattern.CardSetPagingUrl.Match(uri.PathAndQuery);

            if (urlMatch.Success)
            {
                var code = urlMatch.Groups["code"].Value.ToUpperInvariant();
                var page = int.Parse(urlMatch.Groups["page"].Value);

                return new DataSpec($"{code}_{page:D2}", Mime.Json);
            }

            urlMatch = Pattern.CardImageUrl.Match(uri.PathAndQuery);

            if (urlMatch.Success)
            {
                return new DataSpec(
                    $"{urlMatch.Groups["salt"].Value}_{urlMatch.Groups["file"].Value.CalculateMd5Hash()}",
                    Mime.Jpeg);
            }

            throw new KvasirException($"Failed to calculate unique key for URL [{uri}].");
        }
    }

    private static class Pattern
    {
        public static readonly Regex CardSetPagingUrl = new(
            @"/cards/search\?q=e%3a(?<code>\w+)&unique=prints&order=name&page=(?<page>\d+)",
            RegexOptions.Compiled);

        public static readonly Regex CardImageUrl = new(
            @"(/cards\.scryfall\.io)?/border_crop/(?<file>[a-z0-9/]+/[a-z0-9\-%]+\.jpg\?(?<salt>\d+))",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }
}