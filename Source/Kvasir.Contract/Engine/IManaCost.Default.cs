// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IManaCost.Default.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, July 5, 2024 12:30:01 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System;
using System.Collections.Generic;

public sealed class UnknownManaCost : IManaCost
{
    private UnknownManaCost()
    {
    }

    public static UnknownManaCost Instance { get; } = new();

    public int TotalAmount =>
        throw new NotSupportedException("Getting total amount is not allowed!");

    public IReadOnlyCollection<Mana> RequiredManas =>
        throw new NotSupportedException("Getting required manas is not allowed!");

    public int FindAmount(Mana _) =>
        throw new NotSupportedException("Finding amount is not allowed!");
}

public sealed class FreeManaCost : IManaCost
{
    private FreeManaCost()
    {
    }

    public static FreeManaCost Instance { get; } = new();

    public int TotalAmount => 0;

    public IReadOnlyCollection<Mana> RequiredManas { get; } = [];

    public int FindAmount(Mana _) => 0;
}