// --------------------------------------------------------------------------------------------------------------------
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

        public ParsingResult Parse(UnparsedBlob.Card unparsedCard)
        {
            Guard
                .Require(unparsedCard, nameof(unparsedCard))
                .Is.Not.Null();

            var definedCard = new DefinedBlob.Card
            {
                Name = !string.IsNullOrEmpty(unparsedCard.Name)
                    ? unparsedCard.Name
                    : Text.Undefined
            };

            return ValidParsingResult
                .Create(definedCard)
                .ThenParseMultiverseId(unparsedCard.MultiverseId)
                .ThenParseType(unparsedCard.Type)
                .ThenParseManaCost(definedCard.Kind, unparsedCard.ManaCost)
                .ThenParsePower(definedCard.Kind, unparsedCard.Power)
                .ThenParseToughness(definedCard.Kind, unparsedCard.Toughness)
                .ThenParseText(unparsedCard.Text);
        }
    }

    internal static class MagicParserExtensions
    {
        public static ParsingResult ThenParseMultiverseId(this ParsingResult parsingResult, int unparsedMultiverseId)
        {
            Guard
                .Require(parsingResult, nameof(parsingResult))
                .Is.Not.Null();

            if (unparsedMultiverseId < 0)
            {
                return parsingResult.WithMessage("<MultiverseId> Value must be zero or positive.");
            }

            return parsingResult
                .WithChildResult(ValidParsingResult.Create((uint)unparsedMultiverseId))
                .BindToDefinedCard(card => card.MultiverseId);
        }

        public static ParsingResult ThenParseType(this ParsingResult parsingResult, string unparsedType)
        {
            Guard
                .Require(parsingResult, nameof(parsingResult))
                .Is.Not.Null();

            if (string.IsNullOrEmpty(unparsedType))
            {
                return parsingResult.WithMessage("<Kind> Value must not be <null> or empty.");
            }

            var typeMatch = Pattern.Card.Type.Match(unparsedType);

            if (!typeMatch.Success)
            {
                return parsingResult.WithMessage($"<Kind> No matching pattern for value [{unparsedType}].");
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
            CardKind definedKind,
            string unparsedManaCost)
        {
            Guard
                .Require(parsingResult, nameof(parsingResult))
                .Is.Not.Null();

            if (definedKind == CardKind.Land)
            {
                if (string.IsNullOrEmpty(unparsedManaCost))
                {
                    return parsingResult
                        .WithChildResult(ValidParsingResult.Create(DefinedBlob.Cost.Free))
                        .BindToDefinedCard(card => card.Cost);
                }

                return parsingResult.WithMessage($"<ManaCost> Non-empty value for type [{nameof(CardKind.Land)}].");
            }

            if (string.IsNullOrEmpty(unparsedManaCost))
            {
                return parsingResult.WithMessage("<ManaCost> Value must not be <null> or empty.");
            }

            if (!Pattern.Card.ManaCost.IsMatch(unparsedManaCost))
            {
                return parsingResult.WithMessage($"<ManaCost> Symbol(s) has no mapping for value [{unparsedManaCost}].");
            }

            var definedCost = new DefinedBlob.Cost
            {
                Kind = CostKind.Mana,
                Amount = unparsedManaCost
            };

            return parsingResult
                .WithChildResult(ValidParsingResult.Create(definedCost))
                .BindToDefinedCard(card => card.Cost);
        }

        public static ParsingResult ThenParsePower(
            this ParsingResult parsingResult,
            CardKind definedKind,
            string unparsedPower)
        {
            var definedPower = (ushort)0;

            var isValid =
                !string.IsNullOrEmpty(unparsedPower) &&
                ushort.TryParse(unparsedPower, out definedPower);

            if (definedKind != CardKind.Creature)
            {
                return isValid
                    ? parsingResult.WithMessage($"<Power> Non-empty value for non-creature type [{definedKind}].")
                    : parsingResult;
            }

            var powerResult = isValid
                ? ValidParsingResult.Create(definedPower)
                : InvalidParsingResult.Create($"<Power> Invalid value [{unparsedPower}].");

            return parsingResult
                .WithChildResult(powerResult)
                .BindToDefinedCard(card => card.Power);
        }

        public static ParsingResult ThenParseToughness(
            this ParsingResult parsingResult,
            CardKind definedKind,
            string unparsedToughness)
        {
            var definedToughness = (ushort)0;

            var isValid =
                !string.IsNullOrEmpty(unparsedToughness) &&
                ushort.TryParse(unparsedToughness, out definedToughness);

            if (definedKind != CardKind.Creature)
            {
                return isValid
                    ? parsingResult.WithMessage($"<Toughness> Non-empty value for non-creature type [{definedKind}].")
                    : parsingResult;
            }

            var toughnessResult = isValid
                ? ValidParsingResult.Create(definedToughness)
                : InvalidParsingResult.Create($"<Toughness> Invalid value [{unparsedToughness}].");

            return parsingResult
                .WithChildResult(toughnessResult)
                .BindToDefinedCard(card => card.Toughness);
        }

        public static ParsingResult ThenParseText(this ParsingResult parsingResult, string unparsedText)
        {
            var definedAbilities = DefinedBlob.Card.Default.Abilities;

            if (!string.IsNullOrEmpty(unparsedText))
            {
                definedAbilities = unparsedText
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(_ => DefinedBlob.Ability.NotSupported)
                    .ToArray();
            }

            return parsingResult
                .WithChildResult(ValidParsingResult.Create(definedAbilities))
                .BindToDefinedCard(card => card.Abilities);
        }

        private static ParsingResult ThenParseKind(this ParsingResult parsingResult, string unparsedKind)
        {
            var kindResult = Enum.TryParse(unparsedKind, out CardKind definedKind)
                ? ValidParsingResult.Create(definedKind)
                : InvalidParsingResult.Create($"<Kind> No mapping for value [{unparsedKind}].");

            return parsingResult
                .WithChildResult(kindResult)
                .BindToDefinedCard(card => card.Kind);
        }

        private static ParsingResult ThenParseSuperKind(this ParsingResult parsingResult, string unparsedSuperKind)
        {
            var superKindResult = default(ParsingResult);

            if (string.Equals(unparsedSuperKind, "Tribal", StringComparison.OrdinalIgnoreCase))
            {
                return parsingResult
                    .WithChildResult(ValidParsingResult.Create(true))
                    .BindToDefinedCard(card => card.IsTribal)
                    .WithChildResult(ValidParsingResult.Create(CardSuperKind.None))
                    .BindToDefinedCard(card => card.SuperKind);
            }

            if (string.IsNullOrEmpty(unparsedSuperKind))
            {
                superKindResult = ValidParsingResult.Create(CardSuperKind.None);
            }
            else
            {
                superKindResult = Enum.TryParse(unparsedSuperKind, out CardSuperKind definedSuperKind)
                    ? ValidParsingResult.Create(definedSuperKind)
                    : InvalidParsingResult.Create($"<SuperKind> No mapping for value [{unparsedSuperKind}].");
            }

            return parsingResult
                .WithChildResult(superKindResult)
                .BindToDefinedCard(card => card.SuperKind);
        }

        private static ParsingResult ThenParseSubKinds(this ParsingResult parsingResult, params string[] unparsedSubKinds)
        {
            var definedSubKinds = new List<CardSubKind>();
            var invalidValues = new List<string>();

            foreach (var unparsedSubKind in unparsedSubKinds)
            {
                if (Enum.TryParse(unparsedSubKind, out CardSubKind definedSubKind))
                {
                    definedSubKinds.Add(definedSubKind);
                }
                else
                {
                    invalidValues.Add(unparsedSubKind);
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
                subKindsResult = ValidParsingResult.Create(definedSubKinds.ToArray());
            }

            return parsingResult
                .WithChildResult(subKindsResult)
                .BindToDefinedCard(card => card.SubKinds);
        }

        private static ParsingResult BindToDefinedCard(
            this ParsingResult parsingResult,
            Expression<Func<DefinedBlob.Card, object>> bindingExpression)
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