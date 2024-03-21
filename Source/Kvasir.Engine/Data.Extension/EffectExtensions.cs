// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EffectExtensions.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Wednesday, March 20, 2024 5:46:33 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Collections.Generic;
using System.Linq;
using nGratis.AI.Kvasir.Contract;

public static class EffectExtensions
{
    public static IEffect Roll(this IReadOnlyCollection<IEffect> effects)
    {
        return effects.Count == 1
            ? effects.Single()
            : new CompositeEffect { ChildEffects = effects };
    }

    public static IReadOnlyCollection<IEffect> Unroll(this IEffect effect)
    {
        return effect is CompositeEffect compositeEffect
            ? compositeEffect.ChildEffects
            : new[] { effect };
    }
}