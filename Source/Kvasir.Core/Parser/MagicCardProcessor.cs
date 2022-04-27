// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicCardProcessor.cs" company="nGratis">
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
// <creation_timestamp>Tuesday, 25 December 2018 11:50:07 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Parser;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public class MagicCardProcessor : IMagicCardProcessor
{
    public ProcessingResult Process(UnparsedBlob.Card unparsedCard)
    {
        var definedCard = new DefinedBlob.Card
        {
            Name = !string.IsNullOrEmpty(unparsedCard.Name)
                ? unparsedCard.Name
                : DefinedText.Unknown,
            SetCode = !string.IsNullOrEmpty(unparsedCard.SetCode)
                ? unparsedCard.SetCode
                : DefinedText.Unknown
        };

        return ValidProcessingResult
            .Create(definedCard)
            .ThenParseNumber(unparsedCard.Number)
            .ThenProcessMultiverseId(unparsedCard.MultiverseId)
            .ThenParseType(unparsedCard.Type)
            .ThenParseManaCost(definedCard.Kind, unparsedCard.ManaCost)
            .ThenParsePower(definedCard.Kind, unparsedCard.Power)
            .ThenParseToughness(definedCard.Kind, unparsedCard.Toughness)
            .ThenParseText(unparsedCard.Text);
    }
}

internal static class MagicParserExtensions
{
    public static ProcessingResult ThenParseNumber(this ProcessingResult processingResult, string unparsedNumber)
    {
        var definedNumber = (ushort)0;

        var isValid =
            !string.IsNullOrEmpty(unparsedNumber) &&
            ushort.TryParse(unparsedNumber, out definedNumber) &&
            definedNumber > 0;

        if (!isValid)
        {
            return processingResult.WithMessage("<Number> Value must be positive.");
        }

        return processingResult
            .WithChildResult(ValidProcessingResult.Create(definedNumber))
            .BindToDefinedCard(card => card.Number);
    }

    public static ProcessingResult ThenProcessMultiverseId(this ProcessingResult processingResult, int multiverseId)
    {
        if (multiverseId < 0)
        {
            return processingResult.WithMessage("<MultiverseId> Value must be zero or positive.");
        }

        return processingResult
            .WithChildResult(ValidProcessingResult.Create((uint)multiverseId))
            .BindToDefinedCard(card => card.MultiverseId);
    }

    public static ProcessingResult ThenParseType(this ProcessingResult processingResult, string unparsedType)
    {
        if (string.IsNullOrEmpty(unparsedType))
        {
            return processingResult.WithMessage("<Kind> Value must not be <null> or empty.");
        }

        var typeMatch = Pattern.Card.Type.Match(unparsedType);

        if (!typeMatch.Success)
        {
            return processingResult.WithMessage($"<Kind> No matching pattern for value [{unparsedType}].");
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

        return processingResult
            .ThenParseKind(kindValue)
            .ThenParseSuperKind(superKindValue ?? string.Empty)
            .ThenParseSubKinds(subKindValues);
    }

    public static ProcessingResult ThenParseManaCost(
        this ProcessingResult processingResult,
        CardKind definedKind,
        string unparsedManaCost)
    {
        if (definedKind == CardKind.Land)
        {
            if (string.IsNullOrEmpty(unparsedManaCost))
            {
                return processingResult
                    .WithChildResult(ValidProcessingResult.Create(DefinedBlob.PayingManaCost.Free))
                    .BindToDefinedCard(card => card.Cost);
            }

            return processingResult.WithMessage($"<ManaCost> Non-empty value for type [{nameof(CardKind.Land)}].");
        }

        if (string.IsNullOrEmpty(unparsedManaCost))
        {
            return processingResult.WithMessage("<ManaCost> Value must not be <null> or empty.");
        }

        var parsingResult = MagicCardParser.ParseCost(unparsedManaCost);

        if (parsingResult.HasError || parsingResult.Value == null)
        {
            return processingResult.WithMessage($"<ManaCost> Value [{unparsedManaCost}] has invalid symbol.");
        }

        return processingResult
            .WithChildResult(ValidProcessingResult.Create(parsingResult.Value))
            .BindToDefinedCard(card => card.Cost);
    }

    public static ProcessingResult ThenParsePower(
        this ProcessingResult processingResult,
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
                ? processingResult.WithMessage($"<Power> Non-empty value for non-creature type [{definedKind}].")
                : processingResult;
        }

        var powerResult = isValid
            ? ValidProcessingResult.Create(definedPower)
            : InvalidProcessingResult.Create($"<Power> Invalid value [{unparsedPower}].");

        return processingResult
            .WithChildResult(powerResult)
            .BindToDefinedCard(card => card.Power);
    }

    public static ProcessingResult ThenParseToughness(
        this ProcessingResult processingResult,
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
                ? processingResult.WithMessage($"<Toughness> Non-empty value for non-creature type [{definedKind}].")
                : processingResult;
        }

        var toughnessResult = isValid
            ? ValidProcessingResult.Create(definedToughness)
            : InvalidProcessingResult.Create($"<Toughness> Invalid value [{unparsedToughness}].");

        return processingResult
            .WithChildResult(toughnessResult)
            .BindToDefinedCard(card => card.Toughness);
    }

    public static ProcessingResult ThenParseText(this ProcessingResult processingResult, string unparsedText)
    {
        var definedAbilities = Array.Empty<DefinedBlob.Ability>();

        if (!string.IsNullOrEmpty(unparsedText))
        {
            var parsingResult = MagicCardParser.ParseAbility(unparsedText);

            if (!parsingResult.HasError && parsingResult.Value != null)
            {
                definedAbilities = new[]
                {
                    parsingResult.Value
                };
            }
            else
            {
                definedAbilities = new[]
                {
                    DefinedBlob.Ability.NotSupported
                };

                var prettifiedText = Pattern.NewLine.Replace(unparsedText, " ");
                processingResult.WithMessage($"<Ability> No support for value [{prettifiedText}].");
            }
        }

        return processingResult
            .WithChildResult(ValidProcessingResult.Create(definedAbilities))
            .BindToDefinedCard(card => card.Abilities);
    }

    private static ProcessingResult ThenParseKind(this ProcessingResult processingResult, string unparsedKind)
    {
        var kindResult = Enum.TryParse(unparsedKind, out CardKind definedKind)
            ? ValidProcessingResult.Create(definedKind)
            : InvalidProcessingResult.Create($"<Kind> No mapping for value [{unparsedKind}].");

        return processingResult
            .WithChildResult(kindResult)
            .BindToDefinedCard(card => card.Kind);
    }

    private static ProcessingResult ThenParseSuperKind(
        this ProcessingResult processingResult,
        string unparsedSuperKind)
    {
        var superKindResult = default(ProcessingResult);

        if (string.Equals(unparsedSuperKind, "Tribal", StringComparison.OrdinalIgnoreCase))
        {
            return processingResult
                .WithChildResult(ValidProcessingResult.Create(true))
                .BindToDefinedCard(card => card.IsTribal)
                .WithChildResult(ValidProcessingResult.Create(CardSuperKind.None))
                .BindToDefinedCard(card => card.SuperKind);
        }

        if (string.IsNullOrEmpty(unparsedSuperKind))
        {
            superKindResult = ValidProcessingResult.Create(CardSuperKind.None);
        }
        else
        {
            superKindResult = Enum.TryParse(unparsedSuperKind, out CardSuperKind definedSuperKind)
                ? ValidProcessingResult.Create(definedSuperKind)
                : InvalidProcessingResult.Create($"<SuperKind> No mapping for value [{unparsedSuperKind}].");
        }

        return processingResult
            .WithChildResult(superKindResult)
            .BindToDefinedCard(card => card.SuperKind);
    }

    private static ProcessingResult ThenParseSubKinds(
        this ProcessingResult processingResult,
        params string[] unparsedSubKinds)
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

        var subKindsResult = default(ProcessingResult);

        if (invalidValues.Any())
        {
            var formattedValues = invalidValues.Select(value => $"[{value}]");
            var message = $"<SubKind> No mapping for value {string.Join(", ", formattedValues)}.";

            subKindsResult = InvalidProcessingResult.Create(message);
        }
        else
        {
            subKindsResult = ValidProcessingResult.Create(definedSubKinds.ToArray());
        }

        return processingResult
            .WithChildResult(subKindsResult)
            .BindToDefinedCard(card => card.SubKinds);
    }

    private static ProcessingResult BindToDefinedCard(
        this ProcessingResult processingResult,
        Expression<Func<DefinedBlob.Card, object>> bindingExpression)
    {
        return processingResult.BindTo(bindingExpression.GetPropertyInfo());
    }

    private static class Pattern
    {
        public static class Card
        {
            // TODO: Need to support card with second face, e.g. Bloodline Keeper!

            public static readonly Regex Type = new(
                @"^(?:(?<super>\w+)\s)?(?<kind>\w+){1}[—\s\-]*(?:(?<sub>\w+)\s?)*$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

            public static readonly Regex ManaCost = new(
                @"^(?:{(?:(?<colorless>\d+)|(?<color>[WUBRG]))})+$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public static readonly Regex NewLine = new(
            @"[\r\n]+",
            RegexOptions.Compiled);
    }
}