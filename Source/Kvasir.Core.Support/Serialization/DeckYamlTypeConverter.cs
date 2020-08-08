// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeckYamlTypeConverter.cs" company="nGratis">
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
// <creation_timestamp>Thursday, July 30, 2020 3:26:45 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Olympus.Contract;
    using YamlDotNet.Core;
    using YamlDotNet.Core.Events;
    using YamlDotNet.Serialization;

    public class DeckYamlTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type == typeof(DefinedBlob.Deck);

        public object ReadYaml(IParser parser, Type type)
        {
            Guard
                .Require(parser, nameof(parser))
                .Is.Not.Null();

            if (parser.Current?.GetType() != typeof(MappingStart))
            {
                throw new KvasirException($"Parser current token does not begin with <{typeof(MappingStart)}>!");
            }

            parser.MoveNext();

            var deckBuilder = DefinedBlob.Deck.Builder.Create();

            while (parser.Current?.GetType() != typeof(MappingEnd))
            {
                var property = parser.Consume<Scalar>().Value;

                switch (property)
                {
                    case Property.Code:
                        deckBuilder.WithCode(parser.ParseScalarValue<string>());
                        break;

                    case Property.Name:
                        deckBuilder.WithName(parser.ParseScalarValue<string>());
                        break;

                    case Property.Mainboard:
                        parser
                            .ParseSequentialValues(DeckYamlTypeConverter.ParseCardQuantity)
                            .ForEach(tuple => deckBuilder.WithCardAndQuantity(tuple.Card, tuple.Quantity));
                        break;

                    default:
                        throw new KvasirException("");
                }
            }

            parser.MoveNext();

            return deckBuilder.Build();
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            Guard
                .Require(emitter, nameof(emitter))
                .Is.Not.Null();

            if (!(value is DefinedBlob.Deck deck))
            {
                return;
            }

            emitter.Emit(new MappingStart());

            var serializedCardQuantities = deck
                .Cards
                .Select(card => $"{deck[card]} {card.Name} ({card.CardSetCode}) {card.Number}")
                .ToArray();

            emitter
                .EmitProperty(Property.Code, deck.Code)
                .EmitProperty(Property.Name, deck.Name)
                .EmitProperty(Property.Mainboard, serializedCardQuantities);

            emitter.Emit(new MappingEnd());
        }

        private static (DefinedBlob.Deck.Card Card, ushort Quantity) ParseCardQuantity(string value)
        {
            Guard
                .Require(value, nameof(value))
                .Is.Not.Empty();

            var match = Pattern.CardQuantity.Match(value);

            if (!match.Success)
            {
                throw new KvasirException(
                    $"Value [{value}] does not match " +
                    @"format 'Quantity Name (CardQuantity-Set-Code) Number'!");
            }

            var card = new DefinedBlob.Deck.Card(
                match.Groups["name"].Value,
                match.Groups["code"].Value,
                ushort.Parse(match.Groups["number"].Value));

            var quantity = ushort.Parse(match.Groups["quantity"].Value);

            return (card, quantity);
        }

        private static class Property
        {
            public const string Code = "code";

            public const string Name = "name";

            public const string Mainboard = "mainboard";
        }

        private static class Pattern
        {
            public static readonly Regex CardQuantity = new Regex(
                @"^(?<quantity>\d+) (?<name>[\w\-' ]+) \((?<code>[A-Z0-9]+)\) (?<number>\d+)$",
                RegexOptions.Compiled);
        }
    }
}