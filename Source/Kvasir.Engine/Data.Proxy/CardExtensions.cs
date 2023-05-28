// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CardExtensions.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, April 29, 2022 6:46:39 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public static class CardExtensions
{
    public static Land ToProxyLand(this IPermanent permanent)
    {
        Guard
            .Require(permanent.Card.Kind, $"{nameof(permanent)}.{nameof(IPermanent.Card.Kind)}")
            .Is.EqualTo(CardKind.Land);

        return new Land(permanent);
    }

    public static Creature ToProxyCreature(this IPermanent permanent)
    {
        // TODO (COULD): Add `NameOf<...>` helper class to create field text with full namespace!

        Guard
            .Require(permanent.Card.Kind, $"{nameof(permanent)}.{nameof(IPermanent.Card.Kind)}")
            .Is.EqualTo(CardKind.Creature);

        return new Creature(permanent);
    }
}