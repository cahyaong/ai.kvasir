// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SerializationExtensions.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, July 30, 2020 3:08:52 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace YamlDotNet.Serialization;

using System;
using System.Collections.Generic;
using System.Linq;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Core;
using nGratis.Cop.Olympus.Contract;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization.NamingConventions;

public static class YamlSerializationExtensions
{
    public static readonly INamingConvention NamingConvention = HyphenatedNamingConvention.Instance;

    private static readonly ISerializer Serializer = new SerializerBuilder()
        .WithNamingConvention(YamlSerializationExtensions.NamingConvention)
        .WithTypeConverter(CostYamlConverter.Instance)
        .WithTypeConverter(EffectYamlConverter.Instance)
        .WithTypeConverter(DeckYamlConverter.Instance)
        .DisableAliases()
        .Build();

    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(YamlSerializationExtensions.NamingConvention)
        .WithTypeConverter(CostYamlConverter.Instance)
        .WithTypeConverter(EffectYamlConverter.Instance)
        .WithTypeConverter(DeckYamlConverter.Instance)
        .Build();

    public static string SerializeToYaml<T>(this T value)
        where T : class
    {
        return YamlSerializationExtensions
            .Serializer
            .Serialize(value)
            .Trim();
    }

    public static T DeserializeFromYaml<T>(this string value)
    {
        Guard
            .Require(value, nameof(value))
            .Is.Not.Empty();

        return YamlSerializationExtensions
            .Deserializer
            .Deserialize<T>(value.Trim());
    }

    internal static IEmitter EmitField(this IEmitter emitter, string name, string value)
    {
        var isValid =
            !string.IsNullOrEmpty(name) &&
            !string.IsNullOrEmpty(value);

        if (isValid)
        {
            emitter.Emit(new Scalar(YamlSerializationExtensions.NamingConvention.Apply(name)));
            emitter.Emit(new Scalar(value));
        }

        return emitter;
    }

    internal static IEmitter EmitField(this IEmitter emitter, string name, params string[] values)
    {
        values = values
            .Where(value => !string.IsNullOrEmpty(value))
            .ToArray();

        var isValid = !string.IsNullOrEmpty(name);

        if (isValid)
        {
            emitter.Emit(new Scalar(YamlSerializationExtensions.NamingConvention.Apply(name)));

            emitter.Emit(new SequenceStart(default, default, false, SequenceStyle.Block));

            values
                .Select(value => new Scalar(value))
                .ForEach(emitter.Emit);

            emitter.Emit(new SequenceEnd());
        }

        return emitter;
    }

    internal static IEmitter EmitField(this IEmitter emitter, string name, IReadOnlyDictionary<string, string> lookup)
    {
        var isValid = !string.IsNullOrEmpty(name);

        if (isValid)
        {
            emitter.Emit(new Scalar(YamlSerializationExtensions.NamingConvention.Apply(name)));

            emitter.Emit(new MappingStart());

            foreach (var (key, value) in lookup)
            {
                emitter.EmitField(key, value);
            }

            emitter.Emit(new MappingEnd());
        }

        return emitter;
    }

    internal static T ParseScalarValue<T>(this IParser parser, Func<string, T>? convert = null)
    {
        var value = parser
            .Consume<Scalar>()
            .Value;

        return convert != null
            ? convert(value)
            : (T)Convert.ChangeType(value, typeof(T));
    }

    internal static IEnumerable<T> ParseSequentialValues<T>(this IParser parser, Func<string, T>? convert = null)
    {
        if (parser.Current?.GetType() != typeof(SequenceStart))
        {
            throw new KvasirException($"Parser current token does not begin with <{typeof(SequenceStart)}>!");
        }

        parser.MoveNext();

        while (parser.Current?.GetType() != typeof(SequenceEnd))
        {
            yield return parser.ParseScalarValue(convert);
        }

        parser.MoveNext();
    }

    internal static IReadOnlyDictionary<TKey, TValue> ParseLookup<TKey, TValue>(
        this IParser parser,
        Func<string, TKey>? convertKey = null,
        Func<string, TValue>? convertValue = null)
        where TKey : notnull
    {
        if (parser.Current?.GetType() != typeof(MappingStart))
        {
            throw new KvasirException($"Parser current token does not begin with <{typeof(MappingStart)}>!");
        }

        var lookup = new Dictionary<TKey, TValue>();

        parser.MoveNext();

        while (parser.Current?.GetType() != typeof(MappingEnd))
        {
            var key = parser.ParseScalarValue(convertKey);
            var value = parser.ParseScalarValue(convertValue);

            lookup[key] = value;
        }

        parser.MoveNext();

        return lookup;
    }
}