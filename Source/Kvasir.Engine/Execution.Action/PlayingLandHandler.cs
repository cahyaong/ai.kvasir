﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayingLandHandler.cs" company="nGratis">
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
// <creation_timestamp>Sunday, September 18, 2022 12:32:48 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Collections.Generic;
using System.Linq;

public class PlayingLandHandler : BaseActionHandler
{
    public override ActionKind ActionKind => ActionKind.PlayingLand;

    public override bool IsSpecialAction => true;

    public override ICost FindCost(IAction action)
    {
        return action
            .Target.Cards
            .Single()
            .Cost;
    }

    protected override ValidationResult ValidateCore(ITabletop tabletop, IAction action)
    {
        var reasons = new List<ValidationReason>();

        // RX-116.2a — Playing a land is a special action. To play a land, a player puts that land onto the battlefield
        // from the zone it was in (usually that player’s hand). By default, a player can take this action
        // only once during each of their turns. A player can take this action any time they have priority
        // and the stack is empty during a main phase of their turn. See rule 305, “Lands.”.

        // RX-505.6b — During either main phase, the active player may play one land card from their hand if the
        // stack is empty, if the player has priority, and if they haven’t played a land this turn (unless an
        // effect states the player may play additional lands)...

        if (!tabletop.Stack.IsEmpty)
        {
            reasons.Add(ValidationReason.Create(
                "Stack is not empty when playing a land!",
                new[] { "mtg-116.2a", "mtg-505.6b" },
                action));
        }

        if (action.Target.Player != tabletop.ActivePlayer)
        {
            reasons.Add(ValidationReason.Create(
                "Non-active player is playing a land!",
                new[] { "mtg-505.6b" },
                action));
        }
        else if (tabletop.PlayedLandCount >= 1)
        {
            reasons.Add(ValidationReason.Create(
                "Active player had played a land this turn!",
                new[] { "mtg-116.2a", "mtg-505.6b" },
                action));
        }

        return ValidationResult.Create(reasons);
    }

    protected override void ResolveCore(ITabletop tabletop, IAction action)
    {
        // RX-116.2a — Playing a land is a special action. To play a land, a player puts that land onto the battlefield
        // from the zone it was in (usually that player’s hand)...

        action.Target.Player.Hand.MoveToZone(
            action.Target.Cards.Single(),
            tabletop.Battlefield,
            card => card.AsPermanent(action.Target.Player));

        tabletop.PlayedLandCount++;
    }
}