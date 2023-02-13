// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RandomStrategy.cs" company="nGratis">
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
// <creation_timestamp>Tuesday, July 6, 2021 6:47:06 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using nGratis.AI.Kvasir.Contract;

public class RandomStrategy : IStrategy
{
    private readonly IRandomGenerator _randomGenerator;

    public RandomStrategy(IRandomGenerator randomGenerator)
    {
        this._randomGenerator = randomGenerator;
    }

    public IAttackingDecision DeclareAttacker(ITabletop tabletop)
    {
        var attackingPermanents = tabletop
            .FindCreatures(PlayerModifier.Active, CreatureModifier.CanAttack)
            .Where(_ => this._randomGenerator.RollDice(20) <= 10)
            .Select(creature => creature.Permanent)
            .ToImmutableList();

        return new AttackingDecision
        {
            AttackingPermanents = attackingPermanents
        };
    }

    public IBlockingDecision DeclareBlocker(ITabletop tabletop)
    {
        var blockingPermanents = tabletop
            .FindCreatures(PlayerModifier.NonActive, CreatureModifier.CanBlock)
            .Select(creature => creature.Permanent)
            .ToList();

        var combats = new List<Combat>();

        foreach (var attackingPermanent in tabletop.AttackingDecision.AttackingPermanents)
        {
            if (!blockingPermanents.Any())
            {
                break;
            }

            var shouldBlock = this._randomGenerator.RollDice(20) <= 10;

            if (!shouldBlock)
            {
                continue;
            }

            var blockingIndex = this._randomGenerator.RollDice(blockingPermanents.Count) - 1;

            combats.Add(new Combat
            {
                AttackingPermanent = attackingPermanent,
                BlockingPermanents = new[] { blockingPermanents[blockingIndex] }
            });

            blockingPermanents.RemoveAt(blockingIndex);
        }

        return new BlockingDecision
        {
            Combats = combats
        };
    }

    public IAction PerformPrioritizedAction(ITabletop tabletop)
    {
        var legalActions = tabletop
            .FindLegalActions(PlayerModifier.Active)
            .ToImmutableArray();

        if (!legalActions.Any())
        {
            return Action.Pass();
        }

        var playingLandActions = legalActions
            .Where(action => action.Kind == ActionKind.PlayingLand)
            .ToImmutableArray();

        return playingLandActions.Any()
            ? playingLandActions.First()
            : legalActions[this._randomGenerator.RollDice(legalActions.Length)];
    }

    public IAction PerformNonPrioritizedAction(ITabletop tabletop)
    {
        return Action.Pass();
    }
}