// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Strategy.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Sunday, May 21, 2023 7:16:26 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using nGratis.AI.Kvasir.Contract;

public static class Strategy
{
    public static IStrategy Unknown => UnknownStrategy.Instance;

    public static IStrategy Noop => NoopStrategy.Instance;
}

public sealed class NoopStrategy : IStrategy
{
    private NoopStrategy()
    {
    }

    public static NoopStrategy Instance { get; } = new();

    public IAttackingDecision DeclareAttacker(ITabletop _) => AttackingDecision.None;

    public IBlockingDecision DeclareBlocker(ITabletop _) => BlockingDecision.None;

    public IAction PerformPrioritizedAction(ITabletop _) => Action.Pass();

    public IAction PerformNonPrioritizedAction(ITabletop _) => Action.Pass();

    public IAction PerformRequiredAction(ITabletop _, ActionKind __, IParameter ___) => Action.Pass();
}