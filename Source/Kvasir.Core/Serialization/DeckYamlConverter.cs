// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeckYamlConverter.cs" company="nGratis">
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
    using YamlFieldReader = System.Action<YamlDotNet.Core.IParser, Contract.DefinedBlob.Deck.Builder>;

    public class DeckYamlConverter : IYamlTypeConverter
    {
        private static readonly IReadOnlyDictionary<string, YamlFieldReader> FieldReaderLookup =
            new Dictionary<string, YamlFieldReader>
            {
                [YamlSerializationExtensions.NamingConvention.Apply(Field.Code)] = (parser, builder) => builder
                    .WithCode(parser.ParseScalarValue<string>()),

                [YamlSerializationExtensions.NamingConvention.Apply(Field.Name)] = (parser, builder) => builder
                    .WithName(parser.ParseScalarValue<string>()),

                [YamlSerializationExtensions.NamingConvention.Apply(Field.Mainboard)] = (parser, builder) => parser
                    .ParseSequentialValues(DeckYamlConverter.ParseEntryQuantity)
                    .ForEach(tuple => builder.WithCardAndQuantity(tuple.Entry, tuple.Quantity))
            };

        private DeckYamlConverter()
        {
        }

        public static DeckYamlConverter Instance { get; } = new();

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
                var field = parser.Consume<Scalar>().Value;

                if (DeckYamlConverter.FieldReaderLookup.TryGetValue(field, out var reader))
                {
                    reader(parser, deckBuilder);
                }
                else
                {
                    throw new KvasirException($"There is no handler to parse field [{field}]!");
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

            var serializedEntryQuantities = deck
                .Entries
                .Select(card => $"{deck[card]} {card.Name} ({card.SetCode}) {card.Number}")
                .ToArray();

            emitter
                .EmitField(Field.Code, deck.Code)
                .EmitField(Field.Name, deck.Name)
                .EmitField(Field.Mainboard, serializedEntryQuantities);

            emitter.Emit(new MappingEnd());
        }

        private static (DefinedBlob.Deck.Entry Entry, ushort Quantity) ParseEntryQuantity(string value)
        {
            Guard
                .Require(value, nameof(value))
                .Is.Not.Empty();

            var match = Pattern.EntryQuantity.Match(value);

            if (!match.Success)
            {
                throw new KvasirException(
                    $"Value [{value}] does not match " +
                    @"format 'Quantity Name (Set-Code) Number'!");
            }

            var entry = new DefinedBlob.Deck.Entry(
                match.Groups["name"].Value,
                match.Groups["set_code"].Value,
                ushort.Parse(match.Groups["number"].Value));

            var quantity = ushort.Parse(match.Groups["quantity"].Value);

            return (entry, quantity);
        }

        private static class Field
        {
            public const string Code = nameof(Field.Code);

            public const string Name = nameof(Field.Name);

            public const string Mainboard = nameof(Field.Mainboard);
        }

        private static class Pattern
        {
            public static readonly Regex EntryQuantity = new(
                @"^(?<quantity>\d+) (?<name>[\w\-' ]+) \((?<set_code>[A-Z0-9]+)\) (?<number>\d+)$",
                RegexOptions.Compiled);
        }
    }
}