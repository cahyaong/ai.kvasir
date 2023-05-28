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

        public Builder WithAmount(DefinedBlob.PayingManaCost definedCost)
        {
            Enum.GetValues<Mana>()
                .Where(mana => mana != Mana.Unknown)
                .Where(mana => definedCost[mana] > 0)
                .ForEach(mana => this.WithAmount(mana, definedCost[mana]));

            return this;
        }

        public Builder WithAmount(DefinedBlob.ProducingManaEffect definedEffect)
        {
            Enum.GetValues<Mana>()
                .Where(mana => mana != Mana.Unknown)
                .Where(mana => definedEffect[mana] > 0)
                .ForEach(mana => this.WithAmount(mana, definedEffect[mana]));

            return this;
        }

        public Builder WithAmount(IManaPool manaPool)
        {
            manaPool
                .AvailableManas
                .ForEach(mana => manaPool.FindAmount(mana));

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

internal sealed class UnknownManaCost : IManaCost
{
    private UnknownManaCost()
    {
    }

    public static UnknownManaCost Instance { get; } = new();

    public int TotalAmount =>
        throw new NotSupportedException("Getting total amount is not allowed!");

    public IEnumerable<Mana> RequiredManas =>
        throw new NotSupportedException("Getting required manas is not allowed!");

    public int FindAmount(Mana _) =>
        throw new NotSupportedException("Finding amount is not allowed!");
}

internal sealed class FreeManaCost : IManaCost
{
    private FreeManaCost()
    {
    }

    public static FreeManaCost Instance { get; } = new();

    public int TotalAmount => 0;

    public IEnumerable<Mana> RequiredManas => Enumerable.Empty<Mana>();

    public int FindAmount(Mana _) => 0;
}

internal sealed class UnknownManaPool : IManaPool
{
    private UnknownManaPool()
    {
    }

    public static UnknownManaPool Instance { get; } = new();

    public int TotalAmount =>
        throw new NotSupportedException("Getting total amount is not allowed!");

    public IEnumerable<Mana> AvailableManas =>
        throw new NotSupportedException("Getting available manas is not allowed!");

    public int FindAmount(Mana _) =>
        throw new NotSupportedException("Finding amount is not allowed!");

    public void AddAmount(Mana _, int __) =>
        throw new NotSupportedException("Adding amount is not allowed!");

    public void RemoveAmount(Mana _, int __) =>
        throw new NotSupportedException("Removing amount is not allowed!");
}