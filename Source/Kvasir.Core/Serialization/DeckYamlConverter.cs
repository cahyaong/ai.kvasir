// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeckYamlConverter.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, July 30, 2020 3:26:45 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core;

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

    public object ReadYaml(IParser parser, Type _, ObjectDeserializer __)
    {
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

    public void WriteYaml(IEmitter emitter, object? value, Type _, ObjectSerializer __)
    {
        if (value is not DefinedBlob.Deck deck)
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