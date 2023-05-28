// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimulationConfig.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, June 5, 2021 5:58:56 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System;
using System.Collections.Generic;

public class SimulationConfig
{
    public SimulationConfig()
    {
        this.MaxTurnCount = -42;
        this.ShouldTerminateOnIllegalAction = true;
        this.DefinedPlayers = Array.Empty<DefinedBlob.Player>();
    }

    public int MaxTurnCount { get; init; }

    public bool ShouldTerminateOnIllegalAction { get; init; }

    public IReadOnlyCollection<DefinedBlob.Player> DefinedPlayers { get; init; }
}