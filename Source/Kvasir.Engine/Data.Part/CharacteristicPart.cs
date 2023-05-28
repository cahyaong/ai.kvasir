// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CharacteristicPart.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, May 19, 2023 5:10:45 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using nGratis.AI.Kvasir.Contract;

public class CharacteristicPart : IPart
{
    public CharacteristicPart()
    {
        this.ActivatedAbilities = Array.Empty<IAbility>();
    }

    public IReadOnlyCollection<IAbility> ActivatedAbilities { get; init; }
}