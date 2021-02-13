// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefinedBlob.Effect.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2021 Cahya Ong
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// </copyright>
// <author>Cahya Ong - cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, December 27, 2019 7:31:42 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract
{
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
            private readonly IDictionary<Mana, ushort> _amountLookup;

            private ProducingManaEffect()
            {
                this._amountLookup = new Dictionary<Mana, ushort>();
            }

            public override EffectKind Kind => EffectKind.ProducingMana;

            public ushort this[Mana mana]
            {
                get
                {
                    Guard
                        .Require(mana, nameof(mana))
                        .Is.Not.Default();

                    return this._amountLookup.TryGetValue(mana, out var amount)
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
                    return new();
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

                    if (!this._producingManaEffect._amountLookup.ContainsKey(mana))
                    {
                        this._producingManaEffect._amountLookup[mana] = 0;
                    }

                    this._producingManaEffect._amountLookup[mana] += amount;

                    return this;
                }

                public ProducingManaEffect Build()
                {
                    return this._producingManaEffect;
                }
            }
        }
    }
}