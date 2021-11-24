// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RandomGenerator.cs" company="nGratis">
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
// <creation_timestamp>Wednesday, 23 January 2019 11:25:35 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Olympus.Contract;

    public class RandomGenerator : IRandomGenerator
    {
        private readonly Random _random;

        public RandomGenerator(int seed)
        {
            this._random = new Random(seed);
        }

        public static RandomGenerator Default { get; } = new(Environment.TickCount);

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
}