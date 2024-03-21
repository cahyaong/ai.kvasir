// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProducingManaHandler.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, March 19, 2024 3:26:13 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using nGratis.AI.Kvasir.Contract;

public class ProducingManaHandler : BaseEffectHandler
{
    public override EffectKind EffectKind => EffectKind.ProducingMana;

    protected override void ResolveCore(ITabletop tabletop, IEffect effect, ITarget target)
    {
        var amount = effect.Parameter.FindValue<IManaPool>(ParameterKey.Amount);

        target.Player.ManaPool.AddManaPool(amount);
    }
}