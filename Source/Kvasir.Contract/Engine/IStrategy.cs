// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IStrategy.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, July 6, 2021 6:47:03 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

public interface IStrategy
{
    // TODO (MUST): Pass in different tabletop view based on related player!

    IAttackingDecision DeclareAttacker(ITabletop tabletop);

    IBlockingDecision DeclareBlocker(ITabletop tabletop);

    IAction PerformPrioritizedAction(ITabletop tabletop);

    IAction PerformNonPrioritizedAction(ITabletop tabletop);

    IAction PerformRequiredAction(ITabletop tabletop, ActionKind kind, IParameter parameter);
}