// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Ability.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, May 19, 2023 5:33:56 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using nGratis.AI.Kvasir.Contract;

public class Ability : IAbility
{
    public Ability()
    {
        this.CanProduceMana = false;
        this.Costs = Array.Empty<ICost>();
        this.Effects = Array.Empty<IEffect>();
    }

    public static IAbility Unknown => UnknownAbility.Instance;

    public bool CanProduceMana { get; init; }

    public IReadOnlyCollection<ICost> Costs { get; init; }

    public IReadOnlyCollection<IEffect> Effects { get; init; }
}

internal sealed class UnknownAbility : IAbility
{
    private UnknownAbility()
    {
    }

    public static UnknownAbility Instance { get; } = new();

    public bool CanProduceMana =>
        throw new NotSupportedException("Getting can produce mana flag is not allowed!");

    public IReadOnlyCollection<ICost> Costs =>
        throw new NotSupportedException("Getting costs is not allowed!");

    public IReadOnlyCollection<IEffect> Effects =>
        throw new NotSupportedException("Getting effects is not allowed!");
}