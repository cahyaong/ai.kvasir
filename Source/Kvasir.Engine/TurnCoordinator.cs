// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TurnCoordinator.cs" company="nGratis">
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
// <creation_timestamp>Sunday, June 13, 2021 7:25:01 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Olympus.Contract;

    internal class TurnCoordinator
    {
        private readonly Tabletop _tabletop;
        private readonly ILogger _logger;

        private readonly IDictionary<(Ticker.PhaseState, Ticker.StepState), Func<ExecutionResult>> _handlerLookup;

        private AttackingDecision _attackingDecision = AttackingDecision.None;
        private BlockingDecision _blockingDecision = BlockingDecision.None;

        public TurnCoordinator(Tabletop tabletop, ILogger logger)
        {
            Guard
                .Require(tabletop, nameof(tabletop))
                .Is.Not.Null();

            Guard
                .Require(logger, nameof(logger))
                .Is.Not.Null();

            this._tabletop = tabletop;
            this._logger = logger;

            this._handlerLookup = new Dictionary<(Ticker.PhaseState, Ticker.StepState), Func<ExecutionResult>>
            {
                [(Ticker.PhaseState.Combat, Ticker.StepState.DeclareAttackers)] = this.HandleDeclaringAttackerStep,
                [(Ticker.PhaseState.Combat, Ticker.StepState.AssignBlockers)] = this.HandleAssigningBlockersStep,
                [(Ticker.PhaseState.Combat, Ticker.StepState.CombatDamage)] = this.HandleResolvingCombatStep,
                [(Ticker.PhaseState.Ending, Ticker.StepState.Cleanup)] = this.HandleCleaningUpStep
            };
        }

        public ExecutionResult ExecuteStep(int turnId, Ticker.PhaseState phaseState, Ticker.StepState stepState)
        {
            Guard
                .Require(turnId, nameof(turnId))
                .Is.ZeroOrPositive();

            Guard
                .Require(phaseState, nameof(phaseState))
                .Is.Not.Default();

            Guard
                .Require(stepState, nameof(stepState))
                .Is.Not.Default();

            this._logger.LogInfoWithDetails(
                "Processing turn and step...",
                ("ID", $"{turnId:D4}-{phaseState}-{stepState}"),
                ("Active Player", this._tabletop.ActivePlayer.Name));

            return this._handlerLookup.TryGetValue((phaseState, stepState), out var handle)
                ? handle()
                : ExecutionResult.Successful;
        }

        private ExecutionResult HandleDeclaringAttackerStep()
        {
            var attackingDecision = this
                ._tabletop
                .ActivePlayer.Strategy.DeclareAttackers();

            var validationResult = Judge.Validate(attackingDecision);

            this._attackingDecision = !validationResult.HasError
                ? attackingDecision
                : AttackingDecision.None;

            return ExecutionResult.Successful;
        }

        private ExecutionResult HandleAssigningBlockersStep()
        {
            if (this._attackingDecision == AttackingDecision.None)
            {
                return ExecutionResult.Successful;
            }

            var blockingDecision = this
                ._tabletop
                .NonactivePlayer.Strategy
                .AssignBlockers(this._attackingDecision.Attackers);

            var validationResult = Judge.Validate(blockingDecision);

            this._blockingDecision = !validationResult.HasError
                ? blockingDecision
                : BlockingDecision.None;

            return ExecutionResult.Successful;
        }

        private ExecutionResult HandleResolvingCombatStep()
        {
            if (this._attackingDecision == AttackingDecision.None)
            {
                return ExecutionResult.Successful;
            }

            var combatLookup = this
                ._blockingDecision.Combats?
                .Where(combat => this._attackingDecision.Attackers.Contains(combat.Attacker))
                .ToDictionary(combat => combat.Attacker) ?? new Dictionary<Creature, Combat>();

            foreach (var attacker in this._attackingDecision.Attackers)
            {
                if (combatLookup.TryGetValue(attacker, out var matchedCombat))
                {
                    matchedCombat.Attacker.Damage = matchedCombat.Blockers.Sum(blocker => blocker.Power);

                    var attackerPower = matchedCombat.Attacker.Power;

                    matchedCombat
                        .Blockers
                        .ForEach(blocker =>
                        {
                            blocker.Damage = Math.Min(attackerPower, blocker.Toughness);
                            attackerPower -= blocker.Damage;
                        });
                }
                else
                {
                    this._tabletop.NonactivePlayer.Life -= attacker.Power;
                }
            }

            combatLookup
                .Values
                .Select(combat => combat.Attacker)
                .Where(attacker => attacker.Damage >= attacker.Toughness)
                .ForEach(attacker =>
                {
                    this._tabletop.Battlefield.MoveCardToZone(attacker, this._tabletop.ActivePlayer.Graveyard);
                    attacker.Damage = 0;
                });

            combatLookup
                .Values
                .SelectMany(combat => combat.Blockers)
                .Where(blocker => blocker.Damage >= blocker.Toughness)
                .ForEach(blocker =>
                {
                    this._tabletop.Battlefield.MoveCardToZone(blocker, this._tabletop.NonactivePlayer.Graveyard);
                    blocker.Damage = 0;
                });

            return ExecutionResult.Successful;
        }

        private ExecutionResult HandleCleaningUpStep()
        {
            this._tabletop.SwapActivePlayer();

            return ExecutionResult.Successful;
        }
    }
}