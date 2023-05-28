// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BlockingDecision.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, November 11, 2021 4:39:57 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using nGratis.AI.Kvasir.Contract;

public class BlockingDecision : IBlockingDecision
{
    public BlockingDecision()
    {
        this.Combats = Array.Empty<ICombat>();
    }

    public static IBlockingDecision Unknown => UnknownBlockingDecision.Instance;

    public static IBlockingDecision None { get; } = new BlockingDecision();

    public IReadOnlyCollection<ICombat> Combats { get; init; }
}

internal sealed class UnknownBlockingDecision : IBlockingDecision
{
    private UnknownBlockingDecision()
    {
    }

    internal static UnknownBlockingDecision Instance { get; } = new();

    public IReadOnlyCollection<ICombat> Combats =>
        throw new NotSupportedException("Getting combats is not allowed!");
}