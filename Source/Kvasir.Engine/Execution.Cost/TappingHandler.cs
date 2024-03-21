// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TappingHandler.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Sunday, May 28, 2023 10:50:10 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using nGratis.AI.Kvasir.Contract;

public class TappingHandler : BaseCostHandler
{
    public override CostKind CostKind => CostKind.Tapping;

    protected override ValidationResult ValidateCore(ITabletop _, ICost __, ITarget target)
    {
        var invalidPermanents = target
            .Permanents
            .Where(permanent => permanent.IsTapped)
            .ToImmutableArray();

        if (!invalidPermanents.Any())
        {
            return ValidationResult.Successful;
        }

        // RX-701.21a — ...Only untapped permanents can be tapped.

        var reasons = invalidPermanents
            .Select(permanent => ValidationReason.Create(
                "Target permanent is tapped!",
                new[] { "mtg-701.21a" },
                permanent))
            .ToImmutableArray();

        return ValidationResult.Create(reasons);
    }

    protected override void ResolveCore(ITabletop tabletop, ICost cost, ITarget target)
    {
        // RX-701.21a — To tap a permanent, turn it sideways from an upright position...

        target
            .Permanents
            .ForEach(permanent => permanent.IsTapped = true);
    }
}