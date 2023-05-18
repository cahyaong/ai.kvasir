// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TabletopExtensions.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, July 6, 2021 11:06:28 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public static class TabletopExtensions
{
    public static IEnumerable<Creature> FindCreatures(
        this ITabletop tabletop,
        PlayerModifier playerModifier,
        CreatureModifier creatureModifier)
    {
        Guard
            .Require(playerModifier, nameof(playerModifier))
            .Is.Not.Default();

        Guard
            .Require(creatureModifier, nameof(creatureModifier))
            .Is.Not.Default();

        var player = playerModifier == PlayerModifier.Active
            ? tabletop.ActivePlayer
            : tabletop.NonActivePlayer;

        var filteredCreatures = tabletop
            .Battlefield
            .FindAll()
            .Where(permanent => permanent.Card.Kind == CardKind.Creature)
            .Select(permanent => permanent.ToProxyCreature());

        filteredCreatures = creatureModifier switch
        {
            CreatureModifier.None => filteredCreatures,

            CreatureModifier.CanAttack => filteredCreatures
                .Where(creature => creature.Permanent.Controller == player)
                .Where(creature => !creature.Permanent.IsTapped)
                .Where(creature => !creature.HasSummoningSickness),

            CreatureModifier.CanBlock => filteredCreatures
                .Where(creature => creature.Permanent.Controller == player)
                .Where(creature => !creature.Permanent.IsTapped),

            _ => Enumerable.Empty<Creature>()
        };

        return filteredCreatures.ToImmutableList();
    }

    public static IEnumerable<IAction> FindLegalActions(this ITabletop tabletop, PlayerModifier playerModifier)
    {
        var legalActions = Enumerable.Empty<IAction>();

        if (playerModifier == PlayerModifier.Active)
        {
            var canPlayLand =
                tabletop.Stack.IsEmpty &&
                tabletop.PlayedLandCount <= 0;

            if (canPlayLand)
            {
                legalActions = legalActions.Concat(tabletop
                    .ActivePlayer.Hand
                    .FindAll()
                    .Where(card => card.Kind == CardKind.Land)
                    .Select(Action.PlayLand));
            }
        }

        return legalActions;
    }
}