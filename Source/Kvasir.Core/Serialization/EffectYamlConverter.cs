// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EffectYamlConverter.cs" company="nGratis">
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
// <creation_timestamp>Thursday, November 19, 2020 5:59:18 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlReader = System.Func<YamlDotNet.Core.IParser, Contract.DefinedBlob.Effect>;
using YamlWriter = System.Action<YamlDotNet.Core.IEmitter, Contract.DefinedBlob.Effect>;

public class EffectYamlConverter : IYamlTypeConverter
{
    private static readonly IReadOnlyDictionary<EffectKind, YamlReader> ReaderLookup =
        new Dictionary<EffectKind, YamlReader>
        {
            [EffectKind.ProducingMana] = EffectYamlConverter.ReadProducingManaEffect
        };

    private static readonly IReadOnlyDictionary<Type, YamlWriter> WriterLookup =
        new Dictionary<Type, YamlWriter>
        {
            [typeof(DefinedBlob.ProducingManaEffect)] = EffectYamlConverter.WriteProducingManaEffect
        };

    private EffectYamlConverter()
    {
    }

    public static EffectYamlConverter Instance { get; } = new();

    public bool Accepts(Type type) => typeof(DefinedBlob.Effect).IsAssignableFrom(type);

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

        var field = parser.Consume<Scalar>().Value;

        if (field != Field.Kind)
        {
            throw new KvasirException($"Expecting field [{Field.Kind}] before parsing can continue!");
        }

        var effectKind = (EffectKind)Enum.Parse(typeof(EffectKind), parser.ParseScalarValue<string>());

        if (!EffectYamlConverter.ReaderLookup.TryGetValue(effectKind, out var reader))
        {
            throw new KvasirException($"There is no handler to read cost kind [{effectKind}]!");
        }

        var effect = reader(parser);

        parser.MoveNext();

        return effect;
    }

    public void WriteYaml(IEmitter emitter, object value, Type type)
    {
        Guard
            .Require(emitter, nameof(emitter))
            .Is.Not.Null();

        if (!(value is DefinedBlob.Effect effect))
        {
            return;
        }

        emitter.Emit(new MappingStart());
        emitter.EmitField(Field.Kind, effect.Kind.ToString());

        if (EffectYamlConverter.WriterLookup.TryGetValue(effect.GetType(), out var writer))
        {
            writer(emitter, effect);
        }
        else
        {
            throw new KvasirException($"There is no handler to write <{effect.GetType()}>!");
        }

        emitter.Emit(new MappingEnd());
    }

    private static DefinedBlob.Effect ReadProducingManaEffect(IParser parser)
    {
        var amountLookup = default(IReadOnlyDictionary<Mana, ushort>);

        while (parser.Current?.GetType() != typeof(MappingEnd))
        {
            var field = parser.Consume<Scalar>().Value;

            if (field == Field.Amount)
            {
                amountLookup = parser.ParseLookup(
                    mana => (Mana)Enum.Parse(typeof(Mana), mana, true),
                    ushort.Parse);
            }
        }

        if (amountLookup?.Any() != true)
        {
            throw new KvasirException("Producing mana effect must have valid non-empty amount!");
        }

        var effectBuilder = DefinedBlob.ProducingManaEffect.Builder.Create();

        foreach (var (mana, quantity) in amountLookup)
        {
            effectBuilder.WithAmount(mana, quantity);
        }

        return effectBuilder.Build();
    }

    private static void WriteProducingManaEffect(IEmitter emitter, DefinedBlob.Effect effect)
    {
        var targetEffect = (DefinedBlob.ProducingManaEffect)effect;

        emitter.EmitField(
            Field.Amount,
            Enum
                .GetValues(typeof(Mana))
                .OfType<Mana>()
                .Where(mana => mana != Mana.Unknown)
                .Select(mana => new
                {
                    Mana = mana,
                    Quantity = targetEffect[mana]
                })
                .Where(anon => anon.Quantity > 0)
                .ToDictionary(anon => anon.Mana.ToString(), anon => anon.Quantity.ToString()));
    }

    private static class Field
    {
        public static readonly string Kind = YamlSerializationExtensions
            .NamingConvention
            .Apply(nameof(DefinedBlob.Effect.Kind));

        public static readonly string Amount = YamlSerializationExtensions
            .NamingConvention
            .Apply(nameof(Field.Amount));
    }
}