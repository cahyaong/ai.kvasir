// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CostExtensions.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Wednesday, March 20, 2024 5:43:48 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Collections.Generic;
using System.Linq;
using nGratis.AI.Kvasir.Contract;

public static class CostExtensions
{
    public static ICost Roll(this IReadOnlyCollection<ICost> costs)
    {
        return costs.Count == 1
            ? costs.Single()
            : new CompositeCost { ChildCosts = costs };
    }

    public static IReadOnlyCollection<ICost> Unroll(this ICost cost)
    {
        return cost is CompositeCost compositeCost
            ? compositeCost.ChildCosts
            : new[] { cost };
    }
}