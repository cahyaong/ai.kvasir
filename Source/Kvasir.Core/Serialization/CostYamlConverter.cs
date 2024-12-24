// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CostYamlConverter.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, October 30, 2020 9:57:36 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using nGratis.AI.Kvasir.Contract;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlReader = System.Func<YamlDotNet.Core.IParser, Contract.DefinedBlob.Cost>;
using YamlWriter = System.Action<YamlDotNet.Core.IEmitter, Contract.DefinedBlob.Cost>;

public class CostYamlConverter : IYamlTypeConverter
{
    private static readonly IReadOnlyDictionary<CostKind, YamlReader> ReaderLookup =
        new Dictionary<CostKind, YamlReader>
        {
            [CostKind.Tapping] = _ => DefinedBlob.TappingCost.Instance,
            [CostKind.PayingMana] = CostYamlConverter.ReadPayingManaCost
        };

    private static readonly IReadOnlyDictionary<Type, YamlWriter> WriterLookup =
        new Dictionary<Type, YamlWriter>
        {
            [typeof(DefinedBlob.TappingCost)] = (_, _) => { },
            [typeof(DefinedBlob.PayingManaCost)] = CostYamlConverter.WritePayingManaCost
        };

    private CostYamlConverter()
    {
    }

    public static CostYamlConverter Instance { get; } = new();

    public bool Accepts(Type type) => typeof(DefinedBlob.Cost).IsAssignableFrom(type);

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

        var costKind = (CostKind)Enum.Parse(typeof(CostKind), parser.ParseScalarValue<string>());

        if (!CostYamlConverter.ReaderLookup.TryGetValue(costKind, out var reader))
        {
            throw new KvasirException($"There is no handler to read cost kind [{costKind}]!");
        }

        var cost = reader(parser);

        parser.MoveNext();

        return cost;
    }

    public void WriteYaml(IEmitter emitter, object? value, Type _, ObjectSerializer __)
    {
        if (value is not DefinedBlob.Cost cost)
        {
            return;
        }

        emitter.Emit(new MappingStart());
        emitter.EmitField(Field.Kind, cost.Kind.ToString());

        if (CostYamlConverter.WriterLookup.TryGetValue(cost.GetType(), out var writer))
        {
            writer(emitter, cost);
        }
        else
        {
            throw new KvasirException($"There is no handler to write <{cost.GetType()}>!");
        }

        emitter.Emit(new MappingEnd());
    }

    private static DefinedBlob.Cost ReadPayingManaCost(IParser parser)
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
            return DefinedBlob.PayingManaCost.Free;
        }

        var costBuilder = DefinedBlob.PayingManaCost.Builder.Create();

        foreach (var (mana, quantity) in amountByManaLookup)
        {
            costBuilder.WithAmount(mana, quantity);
        }

        return costBuilder.Build();
    }

    private static void WritePayingManaCost(IEmitter emitter, DefinedBlob.Cost cost)
    {
        var targetCost = (DefinedBlob.PayingManaCost)cost;

        emitter.EmitField(
            Field.Amount,
            Enum
                .GetValues(typeof(Mana))
                .OfType<Mana>()
                .Where(mana => mana != Mana.Unknown)
                .Select(mana => new
                {
                    Mana = mana,
                    Quantity = targetCost[mana]
                })
                .Where(anon => anon.Quantity > 0)
                .ToDictionary(anon => anon.Mana.ToString(), anon => anon.Quantity.ToString()));
    }

    private static class Field
    {
        public static readonly string Kind = YamlSerializationExtensions
            .NamingConvention
            .Apply(nameof(DefinedBlob.Cost.Kind));

        public static readonly string Amount = YamlSerializationExtensions
            .NamingConvention
            .Apply(nameof(Field.Amount));
    }
}