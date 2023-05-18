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