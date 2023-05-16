// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManaBlob.cs" company="nGratis">
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
// <creation_timestamp>Sunday, March 19, 2023 1:05:51 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using System.Linq;
using nGratis.AI.Kvasir.Contract;

public class ManaBlob : IManaCost, IManaPool
{
    private readonly IDictionary<Mana, int> _amountByManaLookup;

    private ManaBlob()
    {
        this._amountByManaLookup = new Dictionary<Mana, int>();
    }

    public int TotalAmount => this
        ._amountByManaLookup
        .Values
        .Sum();

    public IEnumerable<Mana> RequiredManas => this._amountByManaLookup.Keys;

    public IEnumerable<Mana> AvailableManas => this._amountByManaLookup.Keys;

    public int FindAmount(Mana mana)
    {
        return this._amountByManaLookup.TryGetValue(mana, out var amount)
            ? amount
            : 0;
    }

    public void AddAmount(Mana mana, int amount)
    {
        if (this._amountByManaLookup.TryGetValue(mana, out var existingAmount))
        {
            this._amountByManaLookup[mana] = existingAmount + amount;
        }
        else
        {
            this._amountByManaLookup.Add(mana, amount);
        }
    }

    public void RemoveAmount(Mana mana, int amount)
    {
        if (!this._amountByManaLookup.TryGetValue(mana, out var existingAmount))
        {
            throw new KvasirException("Mana must exist in blob!", ("Mana", mana));
        }

        if (existingAmount < amount)
        {
            throw new KvasirException("Amount should not exceed existing amount!",
                ("Mana", mana),
                ("Amount", amount),
                ("Existing Amount", existingAmount));
        }

        this._amountByManaLookup[mana] = existingAmount - amount;
    }

    public class Builder

    {
        private readonly ManaBlob _manaBlob;

        private Builder()
        {
            this._manaBlob = new ManaBlob();
        }

        public static Builder Create()
        {
            return new Builder();
        }

        public Builder WithAmount(Mana mana, int amount)
        {
            if (!this._manaBlob._amountByManaLookup.TryGetValue(mana, out var existingAmount))
            {
                this._manaBlob._amountByManaLookup[mana] = existingAmount + amount;
            }
            else
            {
                this._manaBlob._amountByManaLookup.Add(mana, amount);
            }

            return this;
        }

        public Builder WithDefinedCost(DefinedBlob.PayingManaCost definedCost)
        {
            Enum.GetValues<Mana>()
                .Where(mana => mana != Mana.Unknown)
                .Where(mana => definedCost[mana] > 0)
                .ForEach(mana => this.WithAmount(mana, definedCost[mana]));

            return this;
        }

        public ManaBlob Build()
        {
            return this._manaBlob;
        }
    }
}

public static class ManaCost
{
    public static IManaCost Unknown => UnknownManaCost.Instance;

    public static IManaCost Free => FreeManaCost.Instance;
}

public static class ManaPool
{
    public static IManaPool Unknown => UnknownManaPool.Instance;
}