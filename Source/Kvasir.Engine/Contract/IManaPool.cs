// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IManaPool.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Sunday, March 19, 2023 3:03:22 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using nGratis.AI.Kvasir.Contract;

public interface IManaPool
{
    int TotalAmount { get; }

    IEnumerable<Mana> AvailableManas { get; }

    int FindAmount(Mana mana);

    void AddAmount(Mana mana, int amount);

    void RemoveAmount(Mana mana, int amount);
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