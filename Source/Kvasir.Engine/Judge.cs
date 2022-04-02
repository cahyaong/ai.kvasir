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

namespace nGratis.AI.Kvasir.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Olympus.Contract;

    public class Judge
    {
        private readonly ILogger _logger;

        private readonly IReadOnlyDictionary<Phase, Func<Tabletop, ExecutionResult>> _phaseHandlerLookup;

        public Judge(ILogger logger)
        {
            Guard
                .Require(logger, nameof(logger))
                .Is.Not.Null();

            this._logger = logger;

            this._phaseHandlerLookup = new Dictionary<Phase, Func<Tabletop, ExecutionResult>>
            {
                [Phase.Beginning] = this.ExecuteBeginningPhase,
                [Phase.PrecombatMain] = this.ExecuteMainPhase,
                [Phase.Combat] = this.ExecuteCombatPhase,
                [Phase.PostcombatMain] = this.ExecuteMainPhase,
                [Phase.Ending] = this.ExecuteEndingPhase
            };
        }

        public ExecutionResult ExecuteNextTurn(Tabletop tabletop)
        {
            Guard
                .Require(tabletop, nameof(tabletop))
                .Is.Not.Null();

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

        public ExecutionResult ExecuteNextPhase(Tabletop tabletop)
        {
            Guard
                .Require(tabletop, nameof(tabletop))
                .Is.Not.Null();

            tabletop.Phase = tabletop.Phase.Next();

            if (!this._phaseHandlerLookup.TryGetValue(tabletop.Phase, out var handlePhase))
            {
                throw new KvasirException(
                    "No handler is defined for given phase!",
                    ("Phase", tabletop.Phase));
            }

            return handlePhase(tabletop);
        }

        private ExecutionResult ExecuteBeginningPhase(Tabletop tabletop)
        {
            tabletop.TurnId++;

            if (!tabletop.IsFirstTurn)
            {
                (tabletop.ActivePlayer, tabletop.NonactivePlayer) = (tabletop.NonactivePlayer, tabletop.ActivePlayer);
            }

            this._logger.LogDiagnostic(tabletop);

            return ExecutionResult.Successful;
        }

        private ExecutionResult ExecuteMainPhase(Tabletop tabletop)
        {
            this._logger.LogDiagnostic(tabletop);

            return ExecutionResult.Successful;
        }

        private ExecutionResult ExecuteCombatPhase(Tabletop tabletop)
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

            return ExecutionResult.Successful;
        }

        private ExecutionResult ExecuteDeclaringAttackerStep(Tabletop tabletop)
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

        private ExecutionResult ExecuteDeclaringBlockersStep(Tabletop tabletop)
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

        private ExecutionResult ExecuteResolvingCombatDamageStep(Tabletop tabletop)
        {
            if (tabletop.AttackingDecision == AttackingDecision.None)
            {
                return ExecutionResult.Successful;
            }

            var combatLookup = tabletop
                .BlockingDecision.Combats?
                .Where(combat => tabletop.AttackingDecision.Attackers.Contains(combat.Attacker))
                .ToDictionary(combat => combat.Attacker) ?? new Dictionary<Creature, Combat>();

            foreach (var attacker in tabletop.AttackingDecision.Attackers)
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
                    tabletop.NonactivePlayer.Life -= attacker.Power;
                }
            }

            combatLookup
                .Values
                .Select(combat => combat.Attacker)
                .Where(attacker => attacker.Damage >= attacker.Toughness)
                .ForEach(attacker =>
                {
                    tabletop.Battlefield.MoveCardToZone(attacker, tabletop.ActivePlayer.Graveyard);
                    attacker.Damage = 0;
                });

            combatLookup
                .Values
                .SelectMany(combat => combat.Blockers)
                .Where(blocker => blocker.Damage >= blocker.Toughness)
                .ForEach(blocker =>
                {
                    tabletop.Battlefield.MoveCardToZone(blocker, tabletop.NonactivePlayer.Graveyard);
                    blocker.Damage = 0;
                });

            return ExecutionResult.Successful;
        }

        private ExecutionResult ExecuteEndingPhase(Tabletop tabletop)
        {
            this._logger.LogDiagnostic(tabletop);

            return ExecutionResult.Successful;
        }
    }
}