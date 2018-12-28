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

            var parsingResult = ValidParsingResult.Create(new CardInfo());

            var typeMatch = Pattern.CardType.Match(rawCard.Type);

            if (!typeMatch.Success)
            {
                parsingResult.WithMessage($"<Kind> No matching pattern for value [{rawCard.Type}].");
            }
            else
            {
                var kindValue = typeMatch
                    .FindCaptureValues("kind")
                    .Single();

                var superValue = typeMatch
                    .FindCaptureValues("super")
                    .SingleOrDefault();

                var subValues = typeMatch
                    .FindCaptureValues("sub")
                    .ToArray();

                parsingResult
                    .WithChildResult(MagicParser.ParseCardKind(kindValue))
                    .BindToCardInfo(info => info.Kind)
                    .WithChildResult(MagicParser.ParseCardSuperKind(superValue))
                    .BindToCardInfo(info => info.SuperKind)
                    .WithChildResult(MagicParser.ParseCardSubKinds(subValues))
                    .BindToCardInfo(info => info.SubKinds);
            }

            return parsingResult;
        }

        private static ParsingResult ParseCardKind(string value)
        {
            return Enum.TryParse(value, out CardKind kind)
                ? ValidParsingResult.Create(kind)
                : InvalidParsingResult.Create($"<Kind> No mapping for value [{value}].");
        }

        private static ParsingResult ParseCardSuperKind(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return ValidParsingResult.Create(CardSuperKind.None);
            }

            return Enum.TryParse(value, out CardSuperKind superKind)
                ? ValidParsingResult.Create(superKind)
                : InvalidParsingResult.Create($"<SuperKind> No mapping for value [{value}].");
        }

        private static ParsingResult ParseCardSubKinds(IReadOnlyCollection<string> values)
        {
            var subKinds = new List<CardSubKind>();

            if (!values.Any())
            {
                return ValidParsingResult.Create(subKinds);
            }

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

            if (invalidValues.Any())
            {
                var formattedValues = invalidValues.Select(value => $"[{value}]");
                var message = $"<SubKind> No mapping for value {string.Join(", ", formattedValues)}.";

                return InvalidParsingResult.Create(message);
            }

            return ValidParsingResult.Create(subKinds);
        }

        private static class Pattern
        {
            public static readonly Regex CardType = new Regex(
                @"^((?<super>\w+)\s)?(?<kind>\w+){1}[\s-]*((?<sub>\w+)\s?)*$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
    }
}