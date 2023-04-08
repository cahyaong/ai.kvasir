// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IStrategy.cs" company="nGratis">
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

public class UnknownStrategy : IStrategy
{
    private UnknownStrategy()
    {
    }

    internal static UnknownStrategy Instance { get; } = new();

    public IAttackingDecision DeclareAttacker(ITabletop _)
    {
        throw new NotSupportedException("Declaring attacker is not supported!");
    }

    public IBlockingDecision DeclareBlocker(ITabletop _)
    {
        throw new NotSupportedException("Declaring blocker is not supported!");
    }

    public IAction PerformPrioritizedAction(ITabletop _)
    {
        throw new NotSupportedException("Performing prioritized action is not supported!");
    }

    public IAction PerformNonPrioritizedAction(ITabletop _)
    {
        throw new NotSupportedException("Performing non-prioritized action is not supported!");
    }

    public IAction PerformRequiredAction(ITabletop _, ActionKind __, IParameter ___)
    {
        throw new NotSupportedException("Performing required action is not supported!");
    }
}

public class NoopStrategy : IStrategy
{
    private NoopStrategy()
    {
    }

    internal static NoopStrategy Instance { get; } = new();

    public IAttackingDecision DeclareAttacker(ITabletop _)
    {
        return AttackingDecision.None;
    }

    public IBlockingDecision DeclareBlocker(ITabletop _)
    {
        return BlockingDecision.None;
    }

    public IAction PerformPrioritizedAction(ITabletop _)
    {
        return Action.Pass();
    }

    public IAction PerformNonPrioritizedAction(ITabletop _)
    {
        return Action.Pass();
    }

    public IAction PerformRequiredAction(ITabletop _, ActionKind __, IParameter ___)
    {
        return Action.Pass();
    }
}