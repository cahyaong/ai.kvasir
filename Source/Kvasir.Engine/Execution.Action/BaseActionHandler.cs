// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseActionHandler.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, February 23, 2023 7:01:39 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Collections.Immutable;
using nGratis.AI.Kvasir.Contract;

public abstract class BaseActionHandler : IActionHandler
{
    public abstract ActionKind ActionKind { get; }

    public virtual bool IsSpecialAction => false;

    public virtual ICost FindCost(IAction action)
    {
        return Cost.None;
    }

    public ValidationResult Validate(ITabletop tabletop, IAction action)
    {
        if (action.Kind != this.ActionKind)
        {
            throw new KvasirException(
                "Handler is expecting correct action kind!",
                ("Actual Kind", action.Kind),
                ("Expected Kind", this.ActionKind));
        }

        var reasons = this
            .ValidateCore(tabletop, action)
            .Reasons
            .ToImmutableList();

        return ValidationResult.Create(reasons);
    }

    public void Resolve(ITabletop tabletop, IAction action)
    {
        if (action.Kind != this.ActionKind)
        {
            throw new KvasirException(
                "Handler is expecting correct action kind!",
                ("Actual Kind", action.Kind),
                ("Expected Kind", this.ActionKind));
        }

        this.ResolveCore(tabletop, action);
    }

    protected virtual ValidationResult ValidateCore(ITabletop tabletop, IAction action)
    {
        return ValidationResult.Successful;
    }

    protected abstract void ResolveCore(ITabletop tabletop, IAction action);
}