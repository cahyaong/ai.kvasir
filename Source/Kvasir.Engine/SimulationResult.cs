// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimulationResult.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, May 29, 2021 6:31:03 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using nGratis.AI.Kvasir.Contract;

public class SimulationResult : ExecutionResult
{
    private SimulationResult()
    {
        this.Tabletop = Engine.Tabletop.Unknown;
    }

    public static SimulationResult Unknown { get; } = new();

    public ITabletop Tabletop { get; private init; }

    public static SimulationResult Create(ITabletop tabletop, IEnumerable<string> messages)
    {
        return new SimulationResult
        {
            Tabletop = tabletop,
            Messages = messages
                .Where(message => !string.IsNullOrEmpty(message))
                .ToImmutableArray()
        };
    }
}