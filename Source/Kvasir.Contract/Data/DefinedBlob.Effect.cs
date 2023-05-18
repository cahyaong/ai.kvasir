// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefinedBlob.Effect.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, December 27, 2019 7:31:42 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System.Collections.Generic;
using nGratis.Cop.Olympus.Contract;

public static partial class DefinedBlob
{
    public abstract record Effect
    {
        public static Effect Unknown => UnknownEffect.Instance;

        public abstract EffectKind Kind { get; }
    }

    internal sealed record UnknownEffect : Effect
    {
        private UnknownEffect()
        {
        }

        public static UnknownEffect Instance { get; } = new();

        public override EffectKind Kind => EffectKind.Unknown;
    }

    public sealed record ProducingManaEffect : Effect
    {
        private readonly IDictionary<Mana, ushort> _amountByManaLookup;

        private ProducingManaEffect()
        {
            this._amountByManaLookup = new Dictionary<Mana, ushort>();
        }

        public override EffectKind Kind => EffectKind.ProducingMana;

        public ushort this[Mana mana]
        {
            get
            {
                Guard
                    .Require(mana, nameof(mana))
                    .Is.Not.Default();

                return this._amountByManaLookup.TryGetValue(mana, out var amount)
                    ? amount
                    : (ushort)0;
            }
        }

        public class Builder
        {
            private readonly ProducingManaEffect _producingManaEffect;

            private Builder()
            {
                this._producingManaEffect = new ProducingManaEffect();
            }

            public static Builder Create()
            {
                return new Builder();
            }

            public Builder WithAmount(Mana mana, ushort amount)
            {
                // TODO: Consolidate logic with <PayingManaCost> implementation!

                Guard
                    .Require(mana, nameof(mana))
                    .Is.Not.Default();

                Guard
                    .Require(amount, nameof(amount))
                    .Is.Positive();

                if (!this._producingManaEffect._amountByManaLookup.ContainsKey(mana))
                {
                    this._producingManaEffect._amountByManaLookup[mana] = 0;
                }

                this._producingManaEffect._amountByManaLookup[mana] += amount;

                return this;
            }

            public ProducingManaEffect Build()
            {
                return this._producingManaEffect;
            }
        }
    }
}