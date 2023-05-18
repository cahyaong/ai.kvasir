// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManaBlob.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
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