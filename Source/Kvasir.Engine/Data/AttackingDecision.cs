// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AttackingDecision.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, July 6, 2021 7:07:41 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using nGratis.AI.Kvasir.Contract;

public class AttackingDecision : IAttackingDecision
{
    public AttackingDecision()
    {
        this.AttackingPermanents = Array.Empty<IPermanent>();
    }

    public static IAttackingDecision Unknown => UnknownAttackingDecision.Instance;

    public static IAttackingDecision None => NoneAttackingDecision.Instance;

    public IReadOnlyCollection<IPermanent> AttackingPermanents { get; init; }
}