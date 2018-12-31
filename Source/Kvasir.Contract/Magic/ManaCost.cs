// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManaCost.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2018 Cahya Ong
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
// <creation_timestamp>Saturday, 29 December 2018 10:38:28 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using nGratis.Cop.Core.Contract;

    public class ManaCost
    {
        private readonly IDictionary<Mana, ushort> _amountLookup;

        public ManaCost()
        {
            this._amountLookup = new Dictionary<Mana, ushort>();
        }

        public static ManaCost Empty { get; } = EmptyManaCost.Instance;

        public uint ConvertedAmount => (uint)this
            ._amountLookup.Values
            .Sum(amount => amount);

        public virtual ushort this[Mana mana]
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

            set
            {
                Guard
                    .Require(mana, nameof(mana))
                    .Is.Not.Default();

                if (this._amountLookup.ContainsKey(mana))
                {
                    this._amountLookup[mana] += value;
                }
                else
                {
                    this._amountLookup.Add(mana, value);
                }
            }
        }
    }

    internal sealed class EmptyManaCost : ManaCost
    {
        private EmptyManaCost()
        {
        }

        internal static EmptyManaCost Instance { get; } = new EmptyManaCost();

        public override ushort this[Mana mana]
        {
            get => base[mana];
            set => throw new NotSupportedException("Setting mana amount is not allowed.");
        }
    }
}