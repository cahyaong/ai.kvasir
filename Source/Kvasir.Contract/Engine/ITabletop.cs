// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITabletop.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, April 16, 2022 5:18:03 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System.Collections.Generic;

public interface ITabletop
{
    IReadOnlyCollection<IPlayer> Players { get; }

    IZone<IPermanent> Battlefield { get; }

    IZone<IAction> Stack { get; }

    IZone<ICard> Exile { get; }

    int TurnId { get; set; }

    Phase Phase { get; set; }

    IPlayer ActivePlayer { get; set; }

    IPlayer NonActivePlayer { get; set; }

    IPlayer PrioritizedPlayer { get; set; }

    bool IsFirstTurn { get; }

    int PlayedLandCount { get; set; }
}