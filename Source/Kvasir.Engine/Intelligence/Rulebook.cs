// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Rulebook.cs" company="nGratis">
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

namespace nGratis.AI.Kvasir.Engine
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Olympus.Contract;

    public static class Rulebook
    {
        public static IEnumerable<Creature> FindCreatures(
            Tabletop tabletop,
            PlayerModifier playerModifier,
            CreatureModifier creatureModifier)
        {
            Guard
                .Require(tabletop, nameof(tabletop))
                .Is.Not.Null();

            Guard
                .Require(playerModifier, nameof(playerModifier))
                .Is.Not.Default();

            Guard
                .Require(creatureModifier, nameof(creatureModifier))
                .Is.Not.Default();

            var player = playerModifier == PlayerModifier.Active
                ? tabletop.ActivePlayer
                : tabletop.NonactivePlayer;

            var filteredCreatures = tabletop
                .Battlefield.Cards
                .Where(card => card.Kind == CardKind.Creature)
                .OfType<Creature>();

            filteredCreatures = creatureModifier switch
            {
                CreatureModifier.None => filteredCreatures,

                CreatureModifier.CanAttack => filteredCreatures
                    .Where(creature => creature.Controller == player)
                    .Where(creature => !creature.HasSummoningSickness)
                    .Where(creature => !creature.IsTapped),

                CreatureModifier.CanBlock => filteredCreatures
                    .Where(creature => creature.Controller == player)
                    .Where(creature => !creature.IsTapped),

                _ => Enumerable.Empty<Creature>()
            };

            return filteredCreatures.ToImmutableList();
        }

        public static ValidationResult Validate(AttackingDecision attackingDecision)
        {
            Guard
                .Require(attackingDecision, nameof(attackingDecision))
                .Is.Not.Null();

            if (attackingDecision == AttackingDecision.None)
            {
                return ValidationResult.Successful;
            }

            var reasons = new List<ValidationReason>();

            foreach (var attacker in attackingDecision.Attackers)
            {
                if (attacker.HasSummoningSickness)
                {
                    reasons.Add(ValidationReason.Create("Attacking creature has summoning sickness!", attacker));
                }

                if (attacker.IsTapped)
                {
                    reasons.Add(ValidationReason.Create("Attacking creature is tapped!", attacker));
                }
            }

            return ValidationResult.Create(reasons);
        }

        public static ValidationResult Validate(BlockingDecision blockingDecision)
        {
            Guard
                .Require(blockingDecision, nameof(blockingDecision))
                .Is.Not.Null();

            var reasons = new List<ValidationReason>();

            blockingDecision
                .Combats
                .SelectMany(combat => combat.Blockers)
                .GroupBy(blocker => blocker.Id)
                .Where(grouping => grouping.Count() > 1)
                .ForEach(grouping => reasons.Add(ValidationReason.Create(
                    "Blocking creature is assigned to multiple attackers!",
                    grouping.First())));

            return ValidationResult.Create(reasons);
        }
    }
}