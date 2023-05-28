// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Land.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, May 19, 2023 5:09:02 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using System.Linq;
using nGratis.AI.Kvasir.Contract;

public class Land
{
    private readonly Lazy<CharacteristicPart> _deferredCharacteristicPart;

    public Land(IPermanent permanent)
    {
        this._deferredCharacteristicPart = new Lazy<CharacteristicPart>(
            permanent.FindPart<CharacteristicPart>,
            false);
    }

    public bool HasActivatedAbility => this
        ._deferredCharacteristicPart.Value.ActivatedAbilities
        .Any();

    public IEnumerable<IAbility> FindManaAbilities() => this
        ._deferredCharacteristicPart.Value.ActivatedAbilities
        .Where(ability => ability.CanProduceMana);
}