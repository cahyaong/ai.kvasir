// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StubStrategy.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Monday, July 8, 2024 6:33:35 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Framework;

using System.Collections.Immutable;
using System.Linq;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Engine;

public sealed class AllAttackingStrategy : IStrategy
{
    private readonly IJudicialAssistant _judicialAssistant;

    public AllAttackingStrategy(IJudicialAssistant judicialAssistant)
    {
        this._judicialAssistant = judicialAssistant;
    }

    public IAttackingDecision DeclareAttacker(ITabletop tabletop)
    {
        var attackingPermanents = this
            ._judicialAssistant
            .FindCreatures(tabletop, PlayerModifier.Active, CreatureModifier.CanAttack)
            .Select(creature => creature.Permanent)
            .ToImmutableArray();

        return new AttackingDecision
        {
            AttackingPermanents = attackingPermanents
        };
    }

    public IBlockingDecision DeclareBlocker(ITabletop _) => BlockingDecision.None;

    public IAction PerformPrioritizedAction(ITabletop _) => Action.Pass();

    public IAction PerformNonPrioritizedAction(ITabletop _) => Action.Pass();

    public IAction PerformRequiredAction(ITabletop _, ActionKind __, IParameter ___) => Action.Pass();
}