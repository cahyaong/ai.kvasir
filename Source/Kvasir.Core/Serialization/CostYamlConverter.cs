// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CostYamlConverter.cs" company="nGratis">
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

    public object ReadYaml(IParser parser, Type type)
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

    public void WriteYaml(IEmitter emitter, object? value, Type type)
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