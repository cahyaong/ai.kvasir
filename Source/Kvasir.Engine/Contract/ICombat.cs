// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICombat.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, April 16, 2022 5:42:03 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;

public interface ICombat
{
    IPermanent AttackingPermanent { get; }

    IReadOnlyCollection<IPermanent> BlockingPermanents { get; }
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