// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Combat.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, November 11, 2021 4:47:23 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using nGratis.AI.Kvasir.Contract;

public class Combat : ICombat
{
    public Combat()
    {
        this.AttackingPermanent = Permanent.Unknown;
        this.BlockingPermanents = Array.Empty<IPermanent>();
    }

    public static ICombat Unknown => UnknownCombat.Instance;

    public IPermanent AttackingPermanent { get; init; }

    public IReadOnlyCollection<IPermanent> BlockingPermanents { get; init; }
}

internal sealed class UnknownCombat : ICombat
{
    private UnknownCombat()
    {
    }

    internal static UnknownCombat Instance { get; } = new();

    public IPermanent AttackingPermanent => Permanent.Unknown;

    public IReadOnlyCollection<IPermanent> BlockingPermanents =>
        throw new NotSupportedException("Getting blocking permanents is not allowed!");
}