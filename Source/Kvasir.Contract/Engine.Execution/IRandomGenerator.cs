// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRandomGenerator.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Wednesday, 23 January 2019 11:31:53 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System.Collections.Generic;

public interface IRandomGenerator
{
    int RollDice(int sideCount);

    IEnumerable<int> GenerateShufflingIndexes(int entityCount);
}