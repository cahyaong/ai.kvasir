// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EffectYamlConverter.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, November 19, 2020 5:59:18 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using nGratis.AI.Kvasir.Contract;
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

    public object ReadYaml(IParser parser, Type _, ObjectDeserializer __)
    {
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

    public void WriteYaml(IEmitter emitter, object? value, Type _, ObjectSerializer __)
    {
        if (value is not DefinedBlob.Effect effect)
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
        var amountByManaLookup = default(IReadOnlyDictionary<Mana, ushort>);

        while (parser.Current?.GetType() != typeof(MappingEnd))
        {
            var field = parser.Consume<Scalar>().Value;

            if (field == Field.Amount)
            {
                amountByManaLookup = parser.ParseLookup(
                    mana => (Mana)Enum.Parse(typeof(Mana), mana, true),
                    ushort.Parse);
            }
        }

        if (amountByManaLookup?.Any() != true)
        {
            throw new KvasirException("Producing mana effect must have valid non-empty amount!");
        }

        var effectBuilder = DefinedBlob.ProducingManaEffect.Builder.Create();

        foreach (var (mana, quantity) in amountByManaLookup)
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