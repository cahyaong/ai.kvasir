// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiscardingHandler.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, February 23, 2023 6:57:35 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public class DiscardingHandler : BaseActionHandler
{
    // TODO (SHOULD): Check if target cards are coming from target player!
    // TODO (SHOULD): Check if target cards containing duplicating instances!

    public override ActionKind ActionKind => ActionKind.Discarding;

    protected override ValidationResult ValidateCore(ITabletop tabletop, IAction action)
    {
        var reasons = new List<ValidationReason>();

        var actualQuantity = action.Target.Cards.Count;
        var expectedQuantity = action.Parameter.FindValue<int>(ParameterKey.Amount);

        if (actualQuantity < expectedQuantity)
        {
            reasons.Add(ValidationReason.Create(
                @"Discarding action expect an exact amount, but found less cards to discard! " +
                $"Actual Amount: [{actualQuantity}]. Expected Amount: [{expectedQuantity}]",
                new[] { "kvr-101" },
                action));
        }
        else if (actualQuantity > expectedQuantity)
        {
            reasons.Add(ValidationReason.Create(
                @"Discarding action expect an exact amount, but found more cards to discard! " +
                $"Actual Amount: [{actualQuantity}]. Expected Amount: [{expectedQuantity}]",
                new[] { "kvr-102" },
                action));
        }

        return ValidationResult.Create(reasons);
    }

    protected override void ResolveCore(ITabletop _, IAction action)
    {
        var actualQuantity = action.Target.Cards.Count;
        var expectedQuantity = action.Parameter.FindValue<int>(ParameterKey.Amount);

        var discardedCards = Enumerable.Empty<ICard>();

        if (actualQuantity >= expectedQuantity)
        {
            discardedCards = action
                .Target.Cards
                .Take(Math.Min(actualQuantity, expectedQuantity));
        }
        else
        {
            var additionalCards = action
                .Target.Player.Hand
                .FindAll()
                .Except(action.Target.Cards)
                .Take(Math.Max(0, expectedQuantity - actualQuantity));

            discardedCards = discardedCards
                .Append(action.Target.Cards)
                .Append(additionalCards);
        }

        // RX-404.1. A player’s graveyard is their discard pile. Any object that’s countered, discarded, destroyed, or
        // sacrificed is put on top of its owner’s graveyard, ...

        discardedCards
            .ToImmutableArray()
            .ForEach(card => action.Target.Player.Hand.MoveToZone(card, action.Target.Player.Graveyard));
    }
}