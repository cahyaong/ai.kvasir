﻿// --------------------------------------------------------------------------------------------------------------------
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
        var attackingCards = Rulebook
            .FindCreatures(tabletop, PlayerModifier.Active, CreatureModifier.CanAttack)
            .Where(_ => this._randomGenerator.RollDice(20) <= 10)
            .Select(creature => creature.Card)
            .ToImmutableList();

        return new AttackingDecision
        {
            AttackingCards = attackingCards
        };
    }

    public IBlockingDecision DeclareBlocker(ITabletop tabletop)
    {
        var blockingCards = Rulebook
            .FindCreatures(tabletop, PlayerModifier.Nonactive, CreatureModifier.CanBlock)
            .Select(creature => creature.Card)
            .ToList();

        var combats = new List<Combat>();

        foreach (var attackingCard in tabletop.AttackingDecision.AttackingCards)
        {
            if (!blockingCards.Any())
            {
                break;
            }

            var shouldBlock = this._randomGenerator.RollDice(20) <= 10;

            if (!shouldBlock)
            {
                continue;
            }

            var blockingIndex = this._randomGenerator.RollDice(blockingCards.Count) - 1;

            combats.Add(new Combat
            {
                AttackingCard = attackingCard,
                BlockingCards = new[] { blockingCards[blockingIndex] }
            });

            blockingCards.RemoveAt(blockingIndex);
        }

        return new BlockingDecision
        {
            Combats = combats
        };
    }
}