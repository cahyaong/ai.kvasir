// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RandomStrategy.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
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

    private readonly IJudicialAssistant _judicialAssistant;

    public RandomStrategy(IRandomGenerator randomGenerator, IJudicialAssistant judicialAssistant)
    {
        this._randomGenerator = randomGenerator;
        this._judicialAssistant = judicialAssistant;
    }

    public IAttackingDecision DeclareAttacker(ITabletop tabletop)
    {
        var attackingPermanents = this
            ._judicialAssistant
            .FindCreatures(tabletop, PlayerModifier.Active, CreatureModifier.CanAttack)
            .Where(_ => this._randomGenerator.RollDice(20) <= 10)
            .Select(creature => creature.Permanent)
            .ToImmutableArray();

        if (!attackingPermanents.Any())
        {
            return AttackingDecision.None;
        }

        return new AttackingDecision
        {
            AttackingPermanents = attackingPermanents
        };
    }

    public IBlockingDecision DeclareBlocker(ITabletop tabletop)
    {
        var blockingPermanents = this
            ._judicialAssistant
            .FindCreatures(tabletop, PlayerModifier.NonActive, CreatureModifier.CanBlock)
            .Select(creature => creature.Permanent)
            .ToList();

        if (blockingPermanents.Count <= 0)
        {
            return BlockingDecision.None;
        }

        var combats = new List<Combat>();

        foreach (var attackingPermanent in tabletop.AttackingDecision.AttackingPermanents)
        {
            var shouldBlock = this._randomGenerator.RollDice(20) <= 10;

            if (!shouldBlock)
            {
                continue;
            }

            if (blockingPermanents.Count > 0)
            {
                var blockingIndex = this._randomGenerator.RollDice(blockingPermanents.Count) - 1;

                combats.Add(new Combat
                {
                    AttackingPermanent = attackingPermanent,
                    BlockingPermanents = [blockingPermanents[blockingIndex]]
                });

                blockingPermanents.RemoveAt(blockingIndex);
            }
            else
            {
                combats.Add(new Combat
                {
                    AttackingPermanent = attackingPermanent
                });
            }
        }

        return new BlockingDecision
        {
            Combats = combats
        };
    }

    public IAction PerformPrioritizedAction(ITabletop tabletop)
    {
        var legalActions = this
            ._judicialAssistant
            .FindLegalActions(tabletop, PlayerModifier.Active)
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
            : legalActions[this._randomGenerator.RollDice(legalActions.Length) - 1];
    }

    public IAction PerformNonPrioritizedAction(ITabletop tabletop)
    {
        return Action.Pass();
    }

    public IAction PerformRequiredAction(ITabletop tabletop, ActionKind kind, IParameter requirement)
    {
        if (kind == ActionKind.Discarding)
        {
            return Action.Discard(tabletop
                .ActivePlayer.Hand
                .FindManyFromTop(requirement.FindValue<int>(ParameterKey.Amount))
                .ToArray());
        }

        return Action.Pass();
    }
}