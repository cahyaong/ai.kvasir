// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IManaCost.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Sunday, March 19, 2023 1:00:52 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using System.Linq;
using nGratis.AI.Kvasir.Contract;

public interface IManaCost
{
    int TotalAmount { get; }

    IEnumerable<Mana> RequiredManas { get; }

    int FindAmount(Mana mana);
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