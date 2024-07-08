// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBlockingDecision.Default.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Monday, July 1, 2024 12:35:41 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System;
using System.Collections.Generic;

public sealed class UnknownBlockingDecision : IBlockingDecision
{
    private UnknownBlockingDecision()
    {
    }

    public static UnknownBlockingDecision Instance { get; } = new();

    public IReadOnlyCollection<ICombat> Combats =>
        throw new NotSupportedException("Getting combats is not allowed!");
}

public sealed class NoneBlockingDecision : IBlockingDecision
{
    private NoneBlockingDecision()
    {
    }

    public static NoneBlockingDecision Instance { get; } = new();

    public IReadOnlyCollection<ICombat> Combats { get; } = [];
}