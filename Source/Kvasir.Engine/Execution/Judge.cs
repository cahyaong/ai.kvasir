// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Judge.cs" company="nGratis">
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
// <creation_timestamp>Friday, March 11, 2022 7:08:44 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;
using nGratis.Cop.Olympus.Framework;

public class Judge
{
    private readonly ILogger _logger;

    private readonly IReadOnlyDictionary<Phase, Func<ITabletop, ExecutionResult>> _phaseHandlerByPhaseLookup;

    public Judge(ILogger logger)
    {
        this._logger = logger;

        this._phaseHandlerByPhaseLookup = new Dictionary<Phase, Func<ITabletop, ExecutionResult>>
        {
            [Phase.Beginning] = this.ExecuteBeginningPhase,
            [Phase.PrecombatMain] = this.ExecuteMainPhase,
            [Phase.Combat] = this.ExecuteCombatPhase,
            [Phase.PostcombatMain] = this.ExecuteMainPhase,
            [Phase.Ending] = this.ExecuteEndingPhase
        };
    }

    public static Judge Unknown { get; } = new(VoidLogger.Instance);

    public ExecutionResult ExecuteNextTurn(ITabletop tabletop)
    {
        do
        {
            var executionResult = this.ExecuteNextPhase(tabletop);

            if (executionResult.HasError)
            {
                return executionResult;
            }
        }
        while (tabletop.Phase != Phase.Ending);

        return ExecutionResult.Successful;
    }

    public ExecutionResult ExecuteNextPhase(ITabletop tabletop)
    {
        tabletop.Phase = tabletop.Phase.Next();

        if (!this._phaseHandlerByPhaseLookup.TryGetValue(tabletop.Phase, out var handlePhase))
        {
            throw new KvasirException(
                "No handler is defined for given phase!",
                ("Phase", tabletop.Phase));
        }

        return handlePhase(tabletop);
    }

    private ExecutionResult ExecuteBeginningPhase(ITabletop tabletop)
    {
        tabletop.TurnId++;

        if (!tabletop.IsFirstTurn)
        {
            (tabletop.ActivePlayer, tabletop.NonactivePlayer) = (tabletop.NonactivePlayer, tabletop.ActivePlayer);
        }

        // RX-502.3 — Third, the active player determines which permanents they control will untap. Then they untap
        // them all simultaneously.This turn-based action doesn’t use the stack. Normally, all of a player’s
        // permanents untap, but effects can keep one or more of a player’s permanents from untapping.

        // TODO (SHOULD): Implement untap action for other permanent types besides creature!

        Rulebook
            .FindCreatures(tabletop, PlayerModifier.Active, CreatureModifier.None)
            .Where(creature => creature.Card.Controller == tabletop.ActivePlayer)
            .ForEach(creature => creature.IsTapped = false);

        this._logger.LogDiagnostic(tabletop);

        return ExecutionResult.Successful;
    }

    private ExecutionResult ExecuteMainPhase(ITabletop tabletop)
    {
        this._logger.LogDiagnostic(tabletop);

        return ExecutionResult.Successful;
    }

    private ExecutionResult ExecuteCombatPhase(ITabletop tabletop)
    {
        this._logger.LogDiagnostic(tabletop);

        var executionResult = ExecutionResult.Successful;

        tabletop.AttackingDecision = AttackingDecision.None;
        tabletop.BlockingDecision = BlockingDecision.None;

        executionResult = this.ExecuteDeclaringAttackerStep(tabletop);

        if (executionResult.HasError)
        {
            return executionResult;
        }

        // RX-508.1f — The active player taps the chosen creatures. Tapping a creature when it’s declared as an
        // attacker isn’t a cost; attacking simply causes creatures to become tapped.

        tabletop
            .AttackingDecision.AttackingCards
            .ForEach(attacker => attacker.ToProxyCreature().IsTapped = true);

        // RX-508.8 — If no creatures are declared as attackers or put onto the battlefield attacking, skip the declare
        // blockers and combat damage steps.

        if (tabletop.AttackingDecision != AttackingDecision.None)
        {
            executionResult = this.ExecuteDeclaringBlockersStep(tabletop);

            if (executionResult.HasError)
            {
                return executionResult;
            }

            executionResult = this.ExecuteResolvingCombatDamageStep(tabletop);

            if (executionResult.HasError)
            {
                return executionResult;
            }
        }

        return ExecutionResult.Successful;
    }

    private ExecutionResult ExecuteDeclaringAttackerStep(ITabletop tabletop)
    {
        // TODO (COULD): Create a copy of tabletop with appropriate visibility when passing to to strategy!

        var attackingDecision = tabletop
            .ActivePlayer.Strategy
            .DeclareAttacker(tabletop);

        var validationResult = Rulebook.Validate(attackingDecision);

        tabletop.AttackingDecision = !validationResult.HasError
            ? attackingDecision
            : AttackingDecision.None;

        return ExecutionResult.Successful;
    }

    private ExecutionResult ExecuteDeclaringBlockersStep(ITabletop tabletop)
    {
        if (tabletop.AttackingDecision == AttackingDecision.None)
        {
            return ExecutionResult.Successful;
        }

        // TODO (COULD): Create a copy of tabletop with appropriate visibility when passing to to strategy!

        var blockingDecision = tabletop
            .NonactivePlayer.Strategy
            .DeclareBlocker(tabletop);

        var validationResult = Rulebook.Validate(blockingDecision);

        tabletop.BlockingDecision = !validationResult.HasError
            ? blockingDecision
            : BlockingDecision.None;

        return ExecutionResult.Successful;
    }

    private ExecutionResult ExecuteResolvingCombatDamageStep(ITabletop tabletop)
    {
        if (tabletop.AttackingDecision == AttackingDecision.None)
        {
            return ExecutionResult.Successful;
        }

        var combatByAttackerLookup = tabletop
            .BlockingDecision.Combats
            .Where(combat => tabletop.AttackingDecision.AttackingCards.Contains(combat.AttackingCard))
            .ToDictionary(combat => combat.AttackingCard);

        foreach (var attacker in tabletop.AttackingDecision.AttackingCards)
        {
            var attackingCreature = attacker.ToProxyCreature();

            if (combatByAttackerLookup.TryGetValue(attackingCreature.Card, out var matchedCombat))
            {
                var blockingCreatures = matchedCombat
                    .BlockingCards
                    .Select(blocker => blocker.ToProxyCreature())
                    .ToImmutableArray();

                attackingCreature.Damage = blockingCreatures.Sum(blockingCreature => blockingCreature.Power);

                var attackingPower = attackingCreature.Power;

                blockingCreatures.ForEach(blockingCreature =>
                {
                    blockingCreature.Damage = Math.Min(attackingPower, blockingCreature.Toughness);
                    attackingPower -= blockingCreature.Damage;
                });
            }
            else
            {
                tabletop.NonactivePlayer.Life -= attackingCreature.Power;
            }
        }

        combatByAttackerLookup
            .Values
            .Select(combat => combat.AttackingCard.ToProxyCreature())
            .Where(attackingCreature => attackingCreature.Damage >= attackingCreature.Toughness)
            .ForEach(attackingCreature =>
            {
                tabletop.Battlefield.MoveCardToZone(attackingCreature.Card, tabletop.ActivePlayer.Graveyard);
                attackingCreature.Damage = 0;
            });

        combatByAttackerLookup
            .Values
            .SelectMany(combat => combat.BlockingCards)
            .Select(blocker => blocker.ToProxyCreature())
            .Where(blockingCreature => blockingCreature.Damage >= blockingCreature.Toughness)
            .ForEach(blockingCreature =>
            {
                tabletop.Battlefield.MoveCardToZone(blockingCreature.Card, tabletop.NonactivePlayer.Graveyard);
                blockingCreature.Damage = 0;
            });

        return ExecutionResult.Successful;
    }

    private ExecutionResult ExecuteEndingPhase(ITabletop tabletop)
    {
        this._logger.LogDiagnostic(tabletop);

        return ExecutionResult.Successful;
    }
}