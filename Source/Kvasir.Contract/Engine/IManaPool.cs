// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IManaPool.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Sunday, March 19, 2023 3:03:22 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System.Collections.Generic;

public interface IManaPool
{
    int TotalAmount { get; }

    IReadOnlyCollection<Mana> AvailableManas { get; }

    int FindAmount(Mana mana);

    void AddAmount(Mana mana, int amount);

    void AddManaPool(IManaPool manaPool);

    void RemoveAmount(Mana mana, int amount);

    void RemoveMana(Mana mana);

    void UpdateAmount(Mana mana, int amount);
}