// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IStrategy.Default.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Monday, July 1, 2024 12:30:44 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System;

public sealed class UnknownStrategy : IStrategy
{
    private UnknownStrategy()
    {
    }

    public static UnknownStrategy Instance { get; } = new();

    public IAttackingDecision DeclareAttacker(ITabletop _) =>
        throw new NotSupportedException("Declaring attacker is not allowed!");

    public IBlockingDecision DeclareBlocker(ITabletop _) =>
        throw new NotSupportedException("Declaring blocker is not allowed!");

    public IAction PerformPrioritizedAction(ITabletop _) =>
        throw new NotSupportedException("Performing prioritized action is not allowed!");

    public IAction PerformNonPrioritizedAction(ITabletop _) =>
        throw new NotSupportedException("Performing non-prioritized action is not allowed!");

    public IAction PerformRequiredAction(ITabletop _, ActionKind __, IParameter ___) =>
        throw new NotSupportedException("Performing required action is not allowed!");
}