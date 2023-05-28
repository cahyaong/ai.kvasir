// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBlockingDecision.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, April 16, 2022 5:39:19 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System.Collections.Generic;

public interface IBlockingDecision
{
    IReadOnlyCollection<ICombat> Combats { get; }
}