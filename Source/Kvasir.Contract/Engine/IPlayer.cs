// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPlayer.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, April 15, 2022 2:47:31 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

public interface IPlayer
{
    PlayerKind Kind { get; }

    string Name { get; }

    IDeck Deck { get; }

    IStrategy Strategy { get; }

    IZone<ICard> Library { get; }

    IZone<ICard> Hand { get; }

    IZone<ICard> Graveyard { get; }

    IManaPool ManaPool { get; }

    int Life { get; set; }
}