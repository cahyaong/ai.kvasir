// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActionTarget.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Wednesday, July 6, 2022 6:08:28 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;

public class ActionTarget : IActionTarget
{
    public ActionTarget()
    {
        this.Player = Engine.Player.Unknown;
        this.Cards = Array.Empty<ICard>();
    }

    public static IActionTarget Unknown => UnknownActionTarget.Instance;

    public static IActionTarget None => NoneActionTarget.Instance;

    public IPlayer Player { get; set; }

    public IReadOnlyCollection<ICard> Cards { get; init; }
}