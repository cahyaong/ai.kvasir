// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDeck.Default.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Monday, July 1, 2024 12:29:09 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System;
using System.Collections.Generic;

public sealed class UnknownDeck : IDeck
{
    private UnknownDeck()
    {
    }

    public static UnknownDeck Instance { get; } = new();

    public IReadOnlyCollection<ICard> Cards =>
        throw new NotSupportedException("Getting cards is not allowed!");
}