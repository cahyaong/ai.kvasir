// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefinedBlob.Cost.cs" company="nGratis">
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
// <creation_timestamp>Friday, December 27, 2019 7:27:11 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System.Collections.Generic;
using nGratis.Cop.Olympus.Contract;

public static partial class DefinedBlob
{
    public abstract record Cost
    {
        public static Cost Unknown => UnknownCost.Instance;

        public abstract CostKind Kind { get; }
    }

    internal sealed record UnknownCost : Cost
    {
        private UnknownCost()
        {
        }

        public static UnknownCost Instance { get; } = new();

        public override CostKind Kind => CostKind.Unknown;
    }

    public sealed record TappingCost : Cost
    {
        private TappingCost()
        {
        }

        public static TappingCost Instance { get; } = new();

        public override CostKind Kind => CostKind.Tapping;
    }

    public sealed record PayingManaCost : Cost
    {
        private readonly IDictionary<Mana, ushort> _amountByManaLookup;

        private PayingManaCost()
        {
            this._amountByManaLookup = new Dictionary<Mana, ushort>();
        }

        public static PayingManaCost Free { get; } = PayingManaCost.Builder
            .Create()
            .Build();

        public override CostKind Kind => CostKind.PayingMana;

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
            private readonly PayingManaCost _payingManaCost;

            private Builder()
            {
                this._payingManaCost = new PayingManaCost();
            }

            public static Builder Create()
            {
                return new Builder();
            }

            public Builder WithAmount(Mana mana, ushort amount)
            {
                Guard
                    .Require(mana, nameof(mana))
                    .Is.Not.Default();

                if (amount <= 0)
                {
                    return this;
                }

                if (!this._payingManaCost._amountByManaLookup.ContainsKey(mana))
                {
                    this._payingManaCost._amountByManaLookup[mana] = 0;
                }

                this._payingManaCost._amountByManaLookup[mana] += amount;

                return this;
            }

            public PayingManaCost Build()
            {
                return this._payingManaCost;
            }
        }
    }
}