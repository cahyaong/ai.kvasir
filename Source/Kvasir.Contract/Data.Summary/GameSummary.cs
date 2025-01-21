// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GameSummary.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, January 17, 2025 3:40:12 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

using System.Diagnostics;

namespace nGratis.AI.Kvasir.Contract;

[DebuggerDisplay("<Game-Summary> Turn-Count: {this.Tabletop.TurnId + 1}, Winner: {this.WinningPlayer.Name}")]
public record GameSummary
{
    public required string Id { get; init; }

    public required int Seed { get; init; }

    public required ITabletop Tabletop { get; init; }

    public required IPlayer WinningPlayer { get; init; }
}