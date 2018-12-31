// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicParser.cs" company="nGratis">
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
// <creation_timestamp>Tuesday, 25 December 2018 11:50:07 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text.RegularExpressions;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Core.Contract;

    public class MagicParser : IMagicParser
    {
        public ParsingResult ParseRawCard(RawCard rawCard)
        {
            Guard
                .Require(rawCard, nameof(rawCard))
                .Is.Not.Null();

            var cardInfo = new CardInfo
            {
                Name = !string.IsNullOrEmpty(rawCard.Name)
                    ? rawCard.Name
                    : Text.Undefined
            };

            return ValidParsingResult
                .Create(cardInfo)
                .ThenParseMultiverseId(rawCard.MultiverseId)
                .ThenParseCardType(rawCard.Type)
                .ThenParseManaCost(rawCard.ManaCost);
        }
    }

    internal static class MagicParserExtensions
    {
        private static readonly IReadOnlyDictionary<string, Mana> ManaLookup = new Dictionary<string, Mana>
        {
            ["W"] = Mana.White,
            ["U"] = Mana.Blue,
            ["B"] = Mana.Black,
            ["R"] = Mana.Red,
            ["G"] = Mana.Green
        };

        public static ParsingResult ThenParseMultiverseId(this ParsingResult parsingResult, int value)
        {
            Guard
                .Require(parsingResult, nameof(parsingResult))
                .Is.Not.Null();

            if (value < 0)
            {
                return parsingResult.WithMessage("<MultiverseId> Value must be zero or positive.");
            }

            return parsingResult
                .WithChildResult(ValidParsingResult.Create((uint)value))
                .BindToCardInfo(info => info.MultiverseId);
        }

        public static ParsingResult ThenParseCardType(this ParsingResult parsingResult, string value)
        {
            Guard
                .Require(parsingResult, nameof(parsingResult))
                .Is.Not.Null();

            if (string.IsNullOrEmpty(value))
            {
                return parsingResult.WithMessage("<Kind> Value must not be <null> or empty.");
            }

            var typeMatch = Pattern.Card.Type.Match(value);

            if (!typeMatch.Success)
            {
                return parsingResult.WithMessage($"<Kind> No matching pattern for value [{value}].");
            }

            var kindValue = typeMatch
                .FindCaptureValues("kind")
                .Single();

            var superKindValue = typeMatch
                .FindCaptureValues("super")
                .SingleOrDefault();

            var subKindValues = typeMatch
                .FindCaptureValues("sub")
                .ToArray();

            return parsingResult
                .ThenParseCardKind(kindValue)
                .ThenParseCardSuperKind(superKindValue)
                .ThenParseCardSubKinds(subKindValues);
        }

        public static ParsingResult ThenParseManaCost(this ParsingResult parsingResult, string value)
        {
            Guard
                .Require(parsingResult, nameof(parsingResult))
                .Is.Not.Null();

            if (string.IsNullOrEmpty(value))
            {
                return parsingResult.WithMessage("<ManaCost> Value must not be <null> or empty.");
            }

            var manaCostMatch = Pattern.Card.ManaCost.Match(value);

            if (!manaCostMatch.Success)
            {
                return parsingResult.WithMessage($"<ManaCost> Symbol(s) has no mapping for value [{value}].");
            }

            var manaCost = new ManaCost();

            var colorlessValue = manaCostMatch
                .FindCaptureValues("colorless")
                .SingleOrDefault();

            if (!string.IsNullOrEmpty(colorlessValue))
            {
                manaCost[Mana.Colorless] = ushort.Parse(colorlessValue);
            }

            manaCostMatch
                .FindCaptureValues("color")
                .Select(symbol => MagicParserExtensions.ManaLookup[symbol])
                .GroupBy(mana => mana)
                .ForEach(grouping => manaCost[grouping.Key] = (ushort)grouping.Count());

            return parsingResult
                .WithChildResult(ValidParsingResult.Create(manaCost))
                .BindToCardInfo(info => info.ManaCost);
        }

        private static ParsingResult ThenParseCardKind(this ParsingResult parsingResult, string value)
        {
            var kindResult = Enum.TryParse(value, out CardKind kind)
                ? ValidParsingResult.Create(kind)
                : InvalidParsingResult.Create($"<Kind> No mapping for value [{value}].");

            return parsingResult
                .WithChildResult(kindResult)
                .BindToCardInfo(info => info.Kind);
        }

        private static ParsingResult ThenParseCardSuperKind(this ParsingResult parsingResult, string value)
        {
            var superKindResult = default(ParsingResult);

            if (string.IsNullOrEmpty(value))
            {
                superKindResult = ValidParsingResult.Create(CardSuperKind.None);
            }
            else
            {
                superKindResult = Enum.TryParse(value, out CardSuperKind superKind)
                    ? ValidParsingResult.Create(superKind)
                    : InvalidParsingResult.Create($"<SuperKind> No mapping for value [{value}].");
            }

            return parsingResult
                .WithChildResult(superKindResult)
                .BindToCardInfo(info => info.SuperKind);
        }

        private static ParsingResult ThenParseCardSubKinds(this ParsingResult parsingResult, params string[] values)
        {
            var subKinds = new List<CardSubKind>();
            var invalidValues = new List<string>();

            foreach (var value in values)
            {
                if (Enum.TryParse(value, out CardSubKind subKind))
                {
                    subKinds.Add(subKind);
                }
                else
                {
                    invalidValues.Add(value);
                }
            }

            var subKindsResult = default(ParsingResult);

            if (invalidValues.Any())
            {
                var formattedValues = invalidValues.Select(value => $"[{value}]");
                var message = $"<SubKind> No mapping for value {string.Join(", ", formattedValues)}.";

                subKindsResult = InvalidParsingResult.Create(message);
            }
            else
            {
                subKindsResult = ValidParsingResult.Create(subKinds);
            }

            return parsingResult
                .WithChildResult(subKindsResult)
                .BindToCardInfo(info => info.SubKinds);
        }

        private static ParsingResult BindToCardInfo(
            this ParsingResult parsingResult,
            Expression<Func<CardInfo, object>> bindingExpression)
        {
            Guard
                .Require(parsingResult, nameof(parsingResult))
                .Is.Not.Null();

            Guard
                .Require(bindingExpression, nameof(bindingExpression))
                .Is.Not.Null();

            return parsingResult.BindTo(bindingExpression.GetPropertyInfo());
        }

        private static class Pattern
        {
            public static class Card
            {
                public static readonly Regex Type = new Regex(
                    @"^((?<super>\w+)\s)?(?<kind>\w+){1}[\s-]*((?<sub>\w+)\s?)*$",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase);

                public static readonly Regex ManaCost = new Regex(
                    @"^({((?<colorless>\d+)|(?<color>[WUBRG]))})+$",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }
    }
}