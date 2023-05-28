// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TappingHandler.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Sunday, May 28, 2023 10:50:10 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using nGratis.AI.Kvasir.Contract;

public class TappingHandler : BaseCostHandler
{
    public override CostKind CostKind => CostKind.Tapping;

    protected override void ResolveCore(ITabletop tabletop, ICost cost, ITarget target)
    {
        throw new NotImplementedException("WIP: Implement tapping cost for activating land's mana ability!");
    }
}