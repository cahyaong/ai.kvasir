﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IStrategy.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, July 6, 2021 6:47:03 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;

public interface IStrategy
{
    // TODO (MUST): Pass in different tabletop view based on related player!

    IAttackingDecision DeclareAttacker(ITabletop tabletop);

    IBlockingDecision DeclareBlocker(ITabletop tabletop);

    IAction PerformPrioritizedAction(ITabletop tabletop);

    IAction PerformNonPrioritizedAction(ITabletop tabletop);

    IAction PerformRequiredAction(ITabletop tabletop, ActionKind kind, IParameter parameter);
}

public static class Strategy
{
    public static IStrategy Unknown => UnknownStrategy.Instance;

    public static IStrategy Noop => NoopStrategy.Instance;
}

internal sealed class UnknownStrategy : IStrategy
{
    private UnknownStrategy()
    {
    }

    internal static UnknownStrategy Instance { get; } = new();

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

internal sealed class NoopStrategy : IStrategy
{
    private NoopStrategy()
    {
    }

    internal static NoopStrategy Instance { get; } = new();

    public IAttackingDecision DeclareAttacker(ITabletop _) => AttackingDecision.None;

    public IBlockingDecision DeclareBlocker(ITabletop _) => BlockingDecision.None;

    public IAction PerformPrioritizedAction(ITabletop _) => Action.Pass();

    public IAction PerformNonPrioritizedAction(ITabletop _) => Action.Pass();

    public IAction PerformRequiredAction(ITabletop _, ActionKind __, IParameter ___) => Action.Pass();
}