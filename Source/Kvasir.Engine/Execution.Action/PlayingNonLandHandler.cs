// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayingNonLandHandler.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, May 27, 2023 4:32:35 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Linq;
using nGratis.AI.Kvasir.Contract;

public class PlayingNonLandHandler : BaseActionHandler
{
    public override ActionKind ActionKind => ActionKind.PlayingNonLand;

    protected override void ResolveCore(ITabletop tabletop, IAction action)
    {
        action.Target.Player.Hand.MoveToZone(
            action.Target.Cards.Single(),
            tabletop.Battlefield,
            card => PlayingNonLandHandler.CreatePermanent(card, action.Target.Player));
    }

    private static IPermanent CreatePermanent(ICard card, IPlayer player)
    {
        var permanent = card.AsPermanent(player);

        if (permanent.Card.Kind == CardKind.Creature)
        {
            // RX-302.6 — A creature’s activated ability with the tap symbol or the untap symbol in its activation cost
            // can’t be activated unless the creature has been under its controller’s control continuously since their
            // most recent turn began. A creature can’t attack unless it has been under its controller’s control
            // continuously since their most recent turn began. This rule is informally called the “summoning sickness”
            // rule.

            permanent
                .ToProxyCreature()
                .HasSummoningSickness = true;
        }

        return permanent;
    }
}