// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseCostHandler.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, April 15, 2023 4:23:11 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Collections.Immutable;
using nGratis.AI.Kvasir.Contract;

public abstract class BaseCostHandler : ICostHandler
{
    public abstract CostKind CostKind { get; }

    public ValidationResult Validate(ITabletop tabletop, ICost cost)
    {
        if (cost.Kind != this.CostKind)
        {
            throw new KvasirException(
                "Handler is expecting correct cost kind!",
                ("Actual Kind", cost.Kind),
                ("Expected Kind", this.CostKind));
        }

        var reasons = this
            .ValidateCore(tabletop, cost)
            .Reasons
            .ToImmutableList();

        return ValidationResult.Create(reasons);
    }

    public void Resolve(ITabletop tabletop, ICost cost)
    {
        if (cost.Kind != this.CostKind)
        {
            throw new KvasirException(
                "Handler is expecting correct cost kind!",
                ("Actual Kind", cost.Kind),
                ("Expected Kind", this.CostKind));
        }

        this.PayCore(tabletop, cost);
    }

    protected virtual ValidationResult ValidateCore(ITabletop tabletop, ICost cost)
    {
        return ValidationResult.Successful;
    }

    protected abstract void PayCore(ITabletop tabletop, ICost cost);
}