// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITarget.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Wednesday, July 6, 2022 6:09:23 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System.Collections.Generic;

public interface ITarget
{
    IPlayer Player { get; set; }

    IReadOnlyCollection<ICard> Cards { get; }
}