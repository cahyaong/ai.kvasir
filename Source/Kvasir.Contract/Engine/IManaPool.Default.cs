// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IManaPool.Default.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, July 5, 2024 12:32:00 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System;
using System.Collections.Generic;

public sealed class UnknownManaPool : IManaPool
{
    private UnknownManaPool()
    {
    }

    public static UnknownManaPool Instance { get; } = new();

    public int TotalAmount =>
        throw new NotSupportedException("Getting total amount is not allowed!");

    public IReadOnlyCollection<Mana> AvailableManas =>
        throw new NotSupportedException("Getting available manas is not allowed!");

    public int FindAmount(Mana _) =>
        throw new NotSupportedException("Finding amount is not allowed!");

    public void AddAmount(Mana _, int __) =>
        throw new NotSupportedException("Adding amount is not allowed!");

    public void AddManaPool(IManaPool _) =>
        throw new NotSupportedException("Adding mana pool is not allowed!");

    public void RemoveAmount(Mana _, int __) =>
        throw new NotSupportedException("Removing amount is not allowed!");

    public void RemoveMana(Mana _) =>
        throw new NotSupportedException("Removing mana is not allowed!");

    public void UpdateAmount(Mana _, int __) =>
        throw new NotSupportedException("Updating amount is not allowed!");
}