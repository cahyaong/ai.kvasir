// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActivatingManaAbilityHandler.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Sunday, May 21, 2023 6:06:47 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Collections.Generic;
using System.Linq;
using nGratis.AI.Kvasir.Contract;

public class ActivatingManaAbilityHandler : BaseActionHandler
{
    public override ActionKind ActionKind => ActionKind.ActivatingManaAbility;

    public override bool IsSpecialAction => true;

    protected override void ResolveCore(ITabletop tabletop, IAction action)
    {
        action
            .Target.Permanents
            .SelectMany(permanent => permanent
                .FindPart<CharacteristicPart>()
                .ActivatedAbilities
                .Single(ability => ability.CanProduceMana)
                .Effect
                .Unroll()
                .Where(effect => effect.Kind == EffectKind.ProducingMana))
            .ForEach(effect => action
                .Target.Player.ManaPool
                .AddManaPool(effect.Parameter.FindValue<IManaPool>(ParameterKey.Amount)));
    }
}