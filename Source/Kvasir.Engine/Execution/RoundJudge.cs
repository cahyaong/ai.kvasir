// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RoundJudge.cs" company="nGratis">
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

public class RoundJudge
{
    // TODO (MUST): Wire up the validation logic for executing combat phase!

    private readonly IActionJudge _actionJudge;

    private readonly ILogger _logger;

    private readonly IReadOnlyDictionary<Phase, Func<ITabletop, ExecutionResult>> _phaseHandlerByPhaseLookup;

    public RoundJudge(ILogger logger)
        : this(new ActionJudge(), logger)
    {
    }

    public RoundJudge(IActionJudge actionJudge, ILogger logger)
    {
        this._actionJudge = actionJudge;
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

    public static RoundJudge Unknown { get; } = new(VoidLogger.Instance);

    public ExecutionResult ExecuteNextTurn(ITabletop tabletop)
    {
        var executionResults = new List<ExecutionResult>();

        do
        {
            executionResults.Add(this.ExecuteNextPhase(tabletop));
        }
        while (tabletop.Phase != Phase.Ending);

        return ExecutionResult.Create(executionResults);
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

            // RX-504.1 — First, the active player draws a card. This turn-based action doesn’t use the stack.

            tabletop.ActivePlayer.Library.MoveToZone(
                tabletop.ActivePlayer.Library.FindFromTop(),
                tabletop.ActivePlayer.Hand);
        }

        // RX-504.2 — Second, the active player gets priority. (See RX-117, “Timing and Priority”).

        tabletop.PrioritizedPlayer = tabletop.ActivePlayer;

        return ExecutionResult.Successful;
    }

    private ExecutionResult ExecuteMainPhase(ITabletop tabletop)
    {
        this._logger.LogDiagnostic(tabletop);

        var executionResults = new List<ExecutionResult>();

        tabletop.PrioritizedPlayer = tabletop.ActivePlayer;
        executionResults.Add(this.ExecutePerformingActionStep(tabletop));

        // RX-117.3d — If a player has priority and chooses not to take any actions, that player passes. If any mana is
        // in that player’s mana pool, they announce what mana is there. Then the next player in turn order
        // receives priority.

        tabletop.PrioritizedPlayer = tabletop.NonActivePlayer;
        executionResults.Add(this.ExecutePerformingActionStep(tabletop));

        // RX-117.4 — If all players pass in succession (that is, if all players pass without taking any actions in
        // between passing), ..., if the stack is empty, the phase or step ends.

        return ExecutionResult.Create(executionResults);
    }

    private ExecutionResult ExecutePerformingActionStep(ITabletop tabletop)
    {
        var shouldEndStep = false;

        var nonPrioritizedPlayer = tabletop.PrioritizedPlayer == tabletop.ActivePlayer
            ? tabletop.NonActivePlayer
            : tabletop.ActivePlayer;

        var messages = new List<string>();

        while (!shouldEndStep)
        {
            var shouldQueueAction = true;
            var actionIndex = 0;

            while (shouldQueueAction)
            {
                var isPrioritizedAction = actionIndex % 2 == 0;

                var selectedPlayer = isPrioritizedAction
                    ? tabletop.PrioritizedPlayer
                    : nonPrioritizedPlayer;

                var performedAction = isPrioritizedAction
                    ? selectedPlayer.Strategy.PerformPrioritizedAction(tabletop)
                    : selectedPlayer.Strategy.PerformNonPrioritizedAction(tabletop);

                performedAction.Owner = selectedPlayer;

                if (performedAction.Target != ActionTarget.None)
                {
                    performedAction.Target.Player = selectedPlayer;
                }

                var queueingResult = this._actionJudge.QueueAction(tabletop, performedAction);
                messages.AddRange(queueingResult.Messages);

                if (!queueingResult.IsSpecialActionPerformed)
                {
                    actionIndex++;
                }

                shouldQueueAction = !queueingResult.IsStackResolved;
                shouldEndStep = !queueingResult.IsNormalActionPerformed;
            }
        }

        // RX-117.3d — If a player has priority and chooses not to take any actions, that player passes...

        // RX-405.5 — ...If the stack is empty when all players pass, the current step or phase ends and the next
        // begins.

        return ExecutionResult.Create(messages);
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

        // TODO (SHOULD): Remove all invalid attacking creatures, and continue with the valid ones!

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

        // TODO (SHOULD): Remove all invalid blocking creatures, and continue with the valid ones!

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

        var executionResult = ExecutionResult.Successful;

        // RX-117.3a — ...Players usually don’t get priority during the cleanup step (see rule 514.3).
        // RX-513.1 — ...Once it begins, the active player gets priority...

        tabletop.PrioritizedPlayer = tabletop.ActivePlayer;

        // RX-402.2. Each player has a maximum hand size, which is normally seven cards. A player may have any
        // number of cards in their hand, but as part of their cleanup step, the player must discard excess cards
        //
        // down to the maximum hand size.

        // RX-514.1. First, if the active player’s hand contains more cards than their maximum hand size (normally
        // seven), they discard enough cards to reduce their hand size to that number. This turn-based action
        // doesn’t use the stack.

        if (tabletop.ActivePlayer.Hand.Quantity > MagicConstant.Hand.MaxCardCount)
        {
            var parameter = Parameter.Builder
                .Create()
                .WithValue(ParameterKey.Amount, tabletop.ActivePlayer.Hand.Quantity - MagicConstant.Hand.MaxCardCount)
                .Build();

            var action = tabletop
                .ActivePlayer.Strategy
                .PerformRequiredAction(tabletop, ActionKind.Discarding, parameter);

            action.Owner = Player.None;
            action.Target.Player = tabletop.ActivePlayer;
            action.Parameter = parameter;

            executionResult = this._actionJudge.ExecuteAction(tabletop, action);
        }

        tabletop.PlayedLandCount = 0;

        return executionResult;
    }
}