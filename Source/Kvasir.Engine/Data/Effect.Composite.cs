﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Effect.Composite.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, March 19, 2024 3:31:10 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Collections.Generic;
using System.Collections.Immutable;
using nGratis.AI.Kvasir.Contract;

public class CompositeEffect : IEffect
{
    public CompositeEffect()
    {
        this.ChildEffects = ImmutableArray<IEffect>.Empty;
    }

    public int Id => this.GetHashCode();

    public string Name => DefinedText.Composite;

    public EffectKind Kind => EffectKind.MarkingComposite;

    public IParameter Parameter => Engine.Parameter.None;

    public IReadOnlyCollection<IEffect> ChildEffects { get; init; }
}