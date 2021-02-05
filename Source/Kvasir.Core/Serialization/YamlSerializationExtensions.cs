// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SerializationExtensions.cs" company="nGratis">
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
// <creation_timestamp>Thursday, July 30, 2020 3:08:52 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace YamlDotNet.Serialization
{
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
            Guard
                .Require(value, nameof(value))
                .Is.Not.Null();

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
            Guard
                .Require(emitter, nameof(emitter))
                .Is.Not.Null();

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
            Guard
                .Require(emitter, nameof(emitter))
                .Is.Not.Null();

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

        internal static IEmitter EmitField(
            this IEmitter emitter,
            string name,
            IReadOnlyDictionary<string, string> lookup)
        {
            Guard
                .Require(emitter, nameof(emitter))
                .Is.Not.Null();

            var isValid =
                !string.IsNullOrEmpty(name) &&
                lookup != null;

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

        internal static T ParseScalarValue<T>(this IParser parser, Func<string, T> convert = null)
        {
            Guard
                .Require(parser, nameof(parser))
                .Is.Not.Null();

            var value = parser
                .Consume<Scalar>()
                .Value;

            return convert != null
                ? convert(value)
                : (T)Convert.ChangeType(value, typeof(T));
        }

        internal static IEnumerable<T> ParseSequentialValues<T>(this IParser parser, Func<string, T> convert = null)
        {
            Guard
                .Require(parser, nameof(parser))
                .Is.Not.Null();

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
            Func<string, TKey> convertKey = null,
            Func<string, TValue> convertValue = null)
        {
            Guard
                .Require(parser, nameof(parser))
                .Is.Not.Null();

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
}