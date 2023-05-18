﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBlockingDecision.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, April 16, 2022 5:39:19 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;

public interface IBlockingDecision
{
    IEnumerable<ICombat> Combats { get; }
}

internal sealed class UnknownBlockingDecision : IBlockingDecision
{
    private UnknownBlockingDecision()
    {
    }

    internal static UnknownBlockingDecision Instance { get; } = new();

    public IEnumerable<ICombat> Combats =>
        throw new NotSupportedException("Getting combats is not allowed!");
}