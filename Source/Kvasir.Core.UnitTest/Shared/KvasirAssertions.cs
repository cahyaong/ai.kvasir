// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KvasirAssertions.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Wednesday, 30 January 2019 11:56:11 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.UnitTest;

using nGratis.AI.Kvasir.Contract;

internal static class KvasirAssertions
{
    public static CardAssertion Must(this UnparsedBlob.Card card)
    {
        return new CardAssertion(card);
    }

    public static CostAssertion Must(this DefinedBlob.Cost cost)
    {
        return new CostAssertion(cost);
    }

    public static EffectAssertion Must(this DefinedBlob.Effect effect)
    {
        return new EffectAssertion(effect);
    }

    public static AbilityAssertion Must(this DefinedBlob.Ability ability)
    {
        return new AbilityAssertion(ability);
    }
}