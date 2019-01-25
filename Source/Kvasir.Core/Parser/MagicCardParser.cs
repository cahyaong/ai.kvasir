﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicCardParser.cs" company="nGratis">
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

    public class MagicCardParser : IMagicCardParser
    {
        private MagicCardParser()
        {
        }

        public static MagicCardParser Instance { get; } = new MagicCardParser();

        public ParsingResult Parse(RawCard rawCard)
        {
            Guard
                .Require(rawCard, nameof(rawCard))
                .Is.Not.Null();

            var cardDefinition = new CardDefinition
            {
                Name = !string.IsNullOrEmpty(rawCard.Name)
                    ? rawCard.Name
                    : Text.Undefined
            };

            return ValidParsingResult
                .Create(cardDefinition)
                .ThenParseMultiverseId(rawCard.MultiverseId)
                .ThenParseType(rawCard.Type)
                .ThenParseManaCost(cardDefinition.Kind, rawCard.ManaCost)
                .ThenParsePower(cardDefinition.Kind, rawCard.Power)
                .ThenParseToughness(cardDefinition.Kind, rawCard.Toughness)
                .ThenParseText(rawCard.Text);
        }
    }

    internal static class MagicParserExtensions
    {
        public static ParsingResult ThenParseMultiverseId(this ParsingResult parsingResult, int rawMultiverseId)
        {
            Guard
                .Require(parsingResult, nameof(parsingResult))
                .Is.Not.Null();

            if (rawMultiverseId < 0)
            {
                return parsingResult.WithMessage("<MultiverseId> Value must be zero or positive.");
            }

            return parsingResult
                .WithChildResult(ValidParsingResult.Create((uint)rawMultiverseId))
                .BindToCardDefinition(definition => definition.MultiverseId);
        }

        public static ParsingResult ThenParseType(this ParsingResult parsingResult, string rawType)
        {
            Guard
                .Require(parsingResult, nameof(parsingResult))
                .Is.Not.Null();

            if (string.IsNullOrEmpty(rawType))
            {
                return parsingResult.WithMessage("<Kind> Value must not be <null> or empty.");
            }

            var typeMatch = Pattern.Card.Type.Match(rawType);

            if (!typeMatch.Success)
            {
                return parsingResult.WithMessage($"<Kind> No matching pattern for value [{rawType}].");
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
                .ThenParseKind(kindValue)
                .ThenParseSuperKind(superKindValue)
                .ThenParseSubKinds(subKindValues);
        }

        public static ParsingResult ThenParseManaCost(
            this ParsingResult parsingResult,
            CardKind kind,
            string rawManaCost)
        {
            Guard
                .Require(parsingResult, nameof(parsingResult))
                .Is.Not.Null();

            if (kind == CardKind.Land)
            {
                if (string.IsNullOrEmpty(rawManaCost))
                {
                    return parsingResult
                        .WithChildResult(ValidParsingResult.Create(CostDefinition.Free))
                        .BindToCardDefinition(definition => definition.CostDefinition);
                }

                return parsingResult.WithMessage($"<ManaCost> Non-empty value for type [{nameof(CardKind.Land)}].");
            }

            if (string.IsNullOrEmpty(rawManaCost))
            {
                return parsingResult.WithMessage("<ManaCost> Value must not be <null> or empty.");
            }

            if (!Pattern.Card.ManaCost.IsMatch(rawManaCost))
            {
                return parsingResult.WithMessage($"<ManaCost> Symbol(s) has no mapping for value [{rawManaCost}].");
            }

            var costDefinition = new CostDefinition
            {
                Kind = CostKind.Mana,
                Amount = rawManaCost
            };

            return parsingResult
                .WithChildResult(ValidParsingResult.Create(costDefinition))
                .BindToCardDefinition(definition => definition.CostDefinition);
        }

        public static ParsingResult ThenParsePower(this ParsingResult parsingResult, CardKind kind, string rawPower)
        {
            var power = (ushort)0;

            var isValid =
                !string.IsNullOrEmpty(rawPower) &&
                ushort.TryParse(rawPower, out power);

            if (kind != CardKind.Creature)
            {
                return isValid
                    ? parsingResult.WithMessage($"<Power> Non-empty value for non-creature type [{kind}].")
                    : parsingResult;
            }

            var powerResult = isValid
                ? ValidParsingResult.Create(power)
                : InvalidParsingResult.Create($"<Power> Invalid value [{rawPower}].");

            return parsingResult
                .WithChildResult(powerResult)
                .BindToCardDefinition(definition => definition.Power);
        }

        public static ParsingResult ThenParseToughness(
            this ParsingResult parsingResult,
            CardKind kind,
            string rawToughness)
        {
            var toughness = (ushort)0;

            var isValid =
                !string.IsNullOrEmpty(rawToughness) &&
                ushort.TryParse(rawToughness, out toughness);

            if (kind != CardKind.Creature)
            {
                return isValid
                    ? parsingResult.WithMessage($"<Toughness> Non-empty value for non-creature type [{kind}].")
                    : parsingResult;
            }

            var toughnessResult = isValid
                ? ValidParsingResult.Create(toughness)
                : InvalidParsingResult.Create($"<Toughness> Invalid value [{rawToughness}].");

            return parsingResult
                .WithChildResult(toughnessResult)
                .BindToCardDefinition(definition => definition.Toughness);
        }

        public static ParsingResult ThenParseText(this ParsingResult parsingResult, string rawText)
        {
            var abilities = CardDefinition.Default.AbilityDefinitions;

            if (!string.IsNullOrEmpty(rawText))
            {
                abilities = rawText
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(_ => AbilityDefinition.NotSupported)
                    .ToArray();
            }

            return parsingResult
                .WithChildResult(ValidParsingResult.Create(abilities))
                .BindToCardDefinition(definition => definition.AbilityDefinitions);
        }

        private static ParsingResult ThenParseKind(this ParsingResult parsingResult, string rawKind)
        {
            var kindResult = Enum.TryParse(rawKind, out CardKind kind)
                ? ValidParsingResult.Create(kind)
                : InvalidParsingResult.Create($"<Kind> No mapping for value [{rawKind}].");

            return parsingResult
                .WithChildResult(kindResult)
                .BindToCardDefinition(definition => definition.Kind);
        }

        private static ParsingResult ThenParseSuperKind(this ParsingResult parsingResult, string rawSuperKind)
        {
            var superKindResult = default(ParsingResult);

            if (string.Equals(rawSuperKind, "Tribal", StringComparison.OrdinalIgnoreCase))
            {
                return parsingResult
                    .WithChildResult(ValidParsingResult.Create(true))
                    .BindToCardDefinition(definition => definition.IsTribal)
                    .WithChildResult(ValidParsingResult.Create(CardSuperKind.None))
                    .BindToCardDefinition(definition => definition.SuperKind);
            }

            if (string.IsNullOrEmpty(rawSuperKind))
            {
                superKindResult = ValidParsingResult.Create(CardSuperKind.None);
            }
            else
            {
                superKindResult = Enum.TryParse(rawSuperKind, out CardSuperKind superKind)
                    ? ValidParsingResult.Create(superKind)
                    : InvalidParsingResult.Create($"<SuperKind> No mapping for value [{rawSuperKind}].");
            }

            return parsingResult
                .WithChildResult(superKindResult)
                .BindToCardDefinition(definition => definition.SuperKind);
        }

        private static ParsingResult ThenParseSubKinds(this ParsingResult parsingResult, params string[] rawSubKinds)
        {
            var subKinds = new List<CardSubKind>();
            var invalidValues = new List<string>();

            foreach (var rawSubKind in rawSubKinds)
            {
                if (Enum.TryParse(rawSubKind, out CardSubKind subKind))
                {
                    subKinds.Add(subKind);
                }
                else
                {
                    invalidValues.Add(rawSubKind);
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
                subKindsResult = ValidParsingResult.Create(subKinds.ToArray());
            }

            return parsingResult
                .WithChildResult(subKindsResult)
                .BindToCardDefinition(definition => definition.SubKinds);
        }

        private static ParsingResult BindToCardDefinition(
            this ParsingResult parsingResult,
            Expression<Func<CardDefinition, object>> bindingExpression)
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
                    @"^(?:(?<super>\w+)\s)?(?<kind>\w+){1}[\s-—]*(?:(?<sub>\w+)\s?)*$",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase);

                public static readonly Regex ManaCost = new Regex(
                    @"^(?:{(?:(?<colorless>\d+)|(?<color>[WUBRG]))})+$",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }
    }
}