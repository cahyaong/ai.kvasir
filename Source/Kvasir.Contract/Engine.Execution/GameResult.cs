﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GameResult.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, May 29, 2021 6:31:03 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System.Collections.Generic;

public class GameResult : KvasirResult
{
    public GameResult(IEnumerable<string> messages)
        : base(messages)
    {
    }

    public required ITabletop Tabletop { get; init; }

    public required IPlayer WinningPlayer { get; init; }
}