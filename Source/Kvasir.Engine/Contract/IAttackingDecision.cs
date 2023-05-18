// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAttackingDecision.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, April 16, 2022 5:34:49 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;

public interface IAttackingDecision
{
    IReadOnlyCollection<IPermanent> AttackingPermanents { get; }
}

internal sealed class UnknownAttackingDecision : IAttackingDecision
{
    private UnknownAttackingDecision()
    {
    }

    internal static UnknownAttackingDecision Instance { get; } = new();

    public IReadOnlyCollection<IPermanent> AttackingPermanents =>
        throw new NotSupportedException("Getting attacking permanents is not allowed!");
}