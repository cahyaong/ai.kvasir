// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RandomGenerator.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Wednesday, 23 January 2019 11:25:35 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using System.Linq;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;
using DefinedText = nGratis.Cop.Olympus.Contract.DefinedText;

public class RandomGenerator : IRandomGenerator
{
    private readonly Random _random;

    public RandomGenerator(string id)
        : this(id, (int)DateTime.UtcNow.Ticks)
    {
    }

    public RandomGenerator(string id, int seed)
    {
        this._random = new Random(seed);

        this.Id = id;
    }

    public static RandomGenerator Default { get; } = new(DefinedText.Default, Environment.TickCount);

    public string Id { get; init; }

    public int RollDice(int sideCount)
    {
        Guard
            .Require(sideCount, nameof(sideCount))
            .Is.GreaterThan(0);

        return this._random.Next(sideCount) + 1;
    }

    public IEnumerable<int> GenerateShufflingIndexes(int entityCount)
    {
        Guard
            .Require(entityCount, nameof(entityCount))
            .Is.GreaterThan(0);

        var indexes = Enumerable
            .Range(0, entityCount)
            .ToArray();

        // NOTE: This implementation is based on Fisher-Yates algorithm.

        for (var count = indexes.Length - 1; count >= 0; count--)
        {
            var index = this._random.Next(count);

            yield return (ushort)indexes[index];

            indexes[index] = indexes[count];
        }
    }
}