﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DecisionExtensions.cs" company="nGratis">
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
// <creation_timestamp>Wednesday, August 17, 2022 5:17:56 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public static class DecisionExtensions
{
    public static ValidationResult Validate(this IAttackingDecision attackingDecision)
    {
        if (attackingDecision == AttackingDecision.None)
        {
            return ValidationResult.Successful;
        }

        var reasons = new List<ValidationReason>();

        var attackingCreatures = attackingDecision
            .AttackingPermanents
            .Select(card => card.ToProxyCreature())
            .ToImmutableArray();

        // RX-302.6 — ...A creature can’t attack unless it has been under its controller’s control
        // continuously since their most recent turn began.This rule is informally called the “summoning
        // sickness” rule.

        // RX-508.1a — ...The chosen creatures must be untapped, and each one must ... have been controlled by the
        // active player continuously since the turn began.

        foreach (var attackingCreature in attackingCreatures)
        {
            if (attackingCreature.HasSummoningSickness)
            {
                reasons.Add(ValidationReason.Create(
                    "Attacking creature has summoning sickness!",
                    new[] { "302.6", "508.1a" },
                    attackingCreature));
            }

            if (attackingCreature.Permanent.IsTapped)
            {
                reasons.Add(ValidationReason.Create(
                    "Attacking creature is tapped!",
                    new[] { "508.1a" },
                    attackingCreature));
            }
        }

        return ValidationResult.Create(reasons);
    }

    public static ValidationResult Validate(this IBlockingDecision blockingDecision)
    {
        // TODO (MUST): Validate that blocker is not tapped!

        var reasons = new List<ValidationReason>();

        // 509.1a — ...For each of the chosen creatures, the defending player chooses one
        // creature for it to block that’s attacking that player or a planeswalker they control.

        blockingDecision
            .Combats
            .SelectMany(combat => combat.BlockingPermanents)
            .GroupBy(card => card.Id)
            .Where(grouping => grouping.Count() > 1)
            .ForEach(grouping => reasons.Add(ValidationReason.Create(
                "Blocking creature is assigned to multiple attacking creatures!",
                new[] { "509.1a" },
                grouping.First())));

        return ValidationResult.Create(reasons);
    }
}