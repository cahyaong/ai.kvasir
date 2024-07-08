// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimulationResult.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, May 29, 2021 6:31:03 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System.Collections.Generic;

public class SimulationResult : KvasirResult
{
    public SimulationResult(ITabletop tabletop, IEnumerable<string> messages)
        : base(messages)
    {
        this.Tabletop = tabletop;
    }

    public ITabletop Tabletop { get; private init; }
}