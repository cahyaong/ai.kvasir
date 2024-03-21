// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompositeCost.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Monday, March 18, 2024 6:19:23 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Collections.Generic;
using System.Collections.Immutable;
using nGratis.AI.Kvasir.Contract;

public class CompositeCost : ICost
{
    public CompositeCost()
    {
        this.ChildCosts = ImmutableArray<ICost>.Empty;
    }

    public int Id => this.GetHashCode();

    public string Name => DefinedText.Composite;

    public CostKind Kind => CostKind.MarkingComposite;

    public IParameter Parameter => Engine.Parameter.None;

    public IReadOnlyCollection<ICost> ChildCosts { get; init; }
}