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
using System.Reflection;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;
using nGratis.Cop.Olympus.Framework;

public class Judge
{
    private readonly bool _shouldTerminateOnIllegalAction;

    private readonly ILogger _logger;

    private readonly IReadOnlyDictionary<Phase, Func<ITabletop, ExecutionResult>> _phaseHandlerByPhaseLookup;

    private readonly IReadOnlyDictionary<ActionKind, IActionHandler> _actionHandlerByActionKindLookup;

    public Judge(ILogger logger)
        : this(true, logger)
    {
    }

    public Judge(bool shouldTerminateOnIllegalAction, ILogger logger)
    {
        this._shouldTerminateOnIllegalAction = shouldTerminateOnIllegalAction;
        this._logger = logger;

        this._phaseHandlerByPhaseLookup = new Dictionary<Phase, Func<ITabletop, ExecutionResult>>
        {
            [Phase.Beginning] = this.ExecuteBeginningPhase,
            [Phase.PrecombatMain] = this.ExecuteMainPhase,
            [Phase.Combat] = this.ExecuteCombatPhase,
            [Phase.PostcombatMain] = this.ExecuteMainPhase,
            [Phase.Ending] = this.ExecuteEndingPhase
        };

        this._actionHandlerByActionKindLookup = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(type => !type.IsInterface)
            .Where(type => type.IsAssignableTo(typeof(IActionHandler)))
            .Select(Activator.CreateInstance)
            .Cast<IActionHandler>()
            .ToImmutableDictionary(handler => handler.Kind);
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

        // RX-117.3a — The active player receives priority at the beginning of most steps and phases,...

        tabletop.PrioritizedPlayer = tabletop.ActivePlayer;

        return handlePhase(tabletop);
    }

    private ExecutionResult ExecuteBeginningPhase(ITabletop tabletop)
    {
        tabletop.TurnId++;

        if (!tabletop.IsFirstTurn)
        {
            (tabletop.ActivePlayer, tabletop.NonActivePlayer) = (tabletop.NonActivePlayer, tabletop.ActivePlayer);
        }

        // RX-117.3a — ...No player receives priority during the untap step...

        tabletop.PrioritizedPlayer = Player.None;

        // RX-502.3 — Third, the active player determines which permanents they control will untap. Then they untap
        // them all simultaneously. This turn-based action doesn’t use the stack. Normally, all of a player’s
        // permanents untap, but effects can keep one or more of a player’s permanents from untapping.

        // TODO (SHOULD): Implement untap action for other permanent types besides creature!

        tabletop
            .FindCreatures(PlayerModifier.Active, CreatureModifier.None)
            .Where(creature => creature.Permanent.Controller == tabletop.ActivePlayer)
            .ForEach(creature => creature.Permanent.IsTapped = false);

        this._logger.LogDiagnostic(tabletop);

        return ExecutionResult.Successful;
    }

    private ExecutionResult ExecuteMainPhase(ITabletop tabletop)
    {
        this._logger.LogDiagnostic(tabletop);

        tabletop.PrioritizedPlayer = tabletop.ActivePlayer;
        var executionResult = this.ExecutePerformingActionStep(tabletop);

        if (executionResult.HasError)
        {
            return executionResult;
        }

        // RX-117.3d — If a player has priority and chooses not to take any actions, that player passes. If any mana is
        // in that player’s mana pool, they announce what mana is there. Then the next player in turn order
        // receives priority.

        tabletop.PrioritizedPlayer = tabletop.NonActivePlayer;
        executionResult = this.ExecutePerformingActionStep(tabletop);

        if (executionResult.HasError)
        {
            return executionResult;
        }

        // RX-117.4 — If all players pass in succession (that is, if all players pass without taking any actions in
        // between passing), ..., if the stack is empty, the phase or step ends.

        return ExecutionResult.Successful;
    }

    private ExecutionResult ExecutePerformingActionStep(ITabletop tabletop)
    {
        var shouldEndStep = false;

        var nonPrioritizedPlayer = tabletop.PrioritizedPlayer == tabletop.ActivePlayer
            ? tabletop.NonActivePlayer
            : tabletop.ActivePlayer;

        while (!shouldEndStep)
        {
            var shouldResolveStack = false;
            var actionIndex = 0;

            while (!shouldResolveStack)
            {
                var selectedPlayer = actionIndex % 2 == 0
                    ? tabletop.PrioritizedPlayer
                    : nonPrioritizedPlayer;

                var performedAction = selectedPlayer == tabletop.ActivePlayer
                    ? selectedPlayer.Strategy.PerformActiveAction(tabletop)
                    : selectedPlayer.Strategy.PerformNonActiveAction(tabletop);

                performedAction.Owner = selectedPlayer;

                if (!this._actionHandlerByActionKindLookup.TryGetValue(performedAction.Kind, out var actionHandler))
                {
                    throw new KvasirException(
                        "Action has no handler associated to it!",
                        ("Action Kind", performedAction.Kind));
                }

                if (actionHandler.IsSpecialAction)
                {
                    actionHandler.Resolve(tabletop, performedAction);
                }
                else
                {
                    tabletop.Stack.AddToTop(performedAction);
                    shouldResolveStack = tabletop.ShouldResolveStack();

                    if (shouldResolveStack)
                    {
                        tabletop.ResolveStack();
                        shouldEndStep = !tabletop.IsActionPerformed;
                    }
                    else
                    {
                        actionIndex++;
                    }
                }
            }
        }

        // RX-117.3d — If a player has priority and chooses not to take any actions, that player passes...

        // RX-405.5 — ...If the stack is empty when all players pass, the current step or phase ends and the next
        // begins.

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
            .AttackingDecision.AttackingPermanents
            .ForEach(attacker => attacker.IsTapped = true);

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
        // TODO (SHOULD): Create a copy of tabletop with appropriate visibility when passing to to strategy!

        var attackingDecision = tabletop
            .ActivePlayer.Strategy
            .DeclareAttacker(tabletop);

        var validationResult = attackingDecision.Validate();

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

        // TODO (SHOULD): Create a copy of tabletop with appropriate visibility when passing to to strategy!

        var blockingDecision = tabletop
            .NonActivePlayer.Strategy
            .DeclareBlocker(tabletop);

        var validationResult = blockingDecision.Validate();

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

        var combatByAttackingPermanentLookup = tabletop
            .BlockingDecision.Combats
            .Where(combat => tabletop.AttackingDecision.AttackingPermanents.Contains(combat.AttackingPermanent))
            .ToDictionary(combat => combat.AttackingPermanent);

        foreach (var attackingPermanent in tabletop.AttackingDecision.AttackingPermanents)
        {
            var attackingCreature = attackingPermanent.ToProxyCreature();

            if (combatByAttackingPermanentLookup.TryGetValue(attackingPermanent, out var matchedCombat))
            {
                var blockingCreatures = matchedCombat
                    .BlockingPermanents
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
                tabletop.NonActivePlayer.Life -= attackingCreature.Power;
            }
        }

        combatByAttackingPermanentLookup
            .Values
            .Select(combat => combat.AttackingPermanent.ToProxyCreature())
            .Where(attackingCreature => attackingCreature.Damage >= attackingCreature.Toughness)
            .ForEach(attackingCreature =>
            {
                tabletop.Battlefield.MoveToZone(
                    attackingCreature.Permanent,
                    tabletop.ActivePlayer.Graveyard,
                    permanent => permanent.Card);

                attackingCreature.Damage = 0;
            });

        combatByAttackingPermanentLookup
            .Values
            .SelectMany(combat => combat.BlockingPermanents)
            .Select(blocker => blocker.ToProxyCreature())
            .Where(blockingCreature => blockingCreature.Damage >= blockingCreature.Toughness)
            .ForEach(blockingCreature =>
            {
                tabletop.Battlefield.MoveToZone(
                    blockingCreature.Permanent,
                    tabletop.NonActivePlayer.Graveyard,
                    permanent => permanent.Card);

                blockingCreature.Damage = 0;
            });

        return ExecutionResult.Successful;
    }

    private ExecutionResult ExecuteEndingPhase(ITabletop tabletop)
    {
        this._logger.LogDiagnostic(tabletop);

        // RX-117.3a — ...Players usually don’t get priority during the cleanup step (see rule 514.3).

        tabletop.PrioritizedPlayer = Player.None;
        tabletop.PlayedLandCount = 0;

        return ExecutionResult.Successful;
    }
}