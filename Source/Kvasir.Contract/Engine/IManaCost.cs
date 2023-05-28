// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IManaCost.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Sunday, March 19, 2023 1:00:52 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System.Collections.Generic;

public interface IManaCost
{
    int TotalAmount { get; }

    IEnumerable<Mana> RequiredManas { get; }

    int FindAmount(Mana mana);
}