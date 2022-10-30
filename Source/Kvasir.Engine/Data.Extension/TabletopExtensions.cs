// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TabletopExtensions.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2021 Cahya Ong
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// </copyright>
// <author>Cahya Ong - cahya.ong@gmail.com</author>
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
            if (tabletop.Stack.IsEmpty)
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

    internal static bool ShouldResolveStack(this ITabletop tabletop)
    {
        if (tabletop.Stack.Quantity < 2)
        {
            return false;
        }

        // RX-117.4 — If all players pass in succession (that is, if all players pass without taking any actions in
        // between passing), the spell or ability on top of the stack resolves...

        // RX-405.5 — When all players pass in succession, the top (last-added) spell or ability on the stack
        // resolves...

        return tabletop
            .Stack
            .FindManyFromTop(2)
            .All(action => action.Kind == ActionKind.Passing);
    }

    internal static void ResolveStack(this ITabletop tabletop)
    {
        tabletop.Stack.RemoveManyFromTop(2);
        tabletop.IsActionPerformed = !tabletop.Stack.IsEmpty;
    }
}