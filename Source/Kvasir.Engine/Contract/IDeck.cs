// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDeck.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, April 16, 2022 5:05:41 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;

public interface IDeck
{
    IReadOnlyCollection<ICard> Cards { get; }
}

internal sealed class UnknownDeck : IDeck
{
    private UnknownDeck()
    {
    }

    internal static UnknownDeck Instance { get; } = new();

    public IReadOnlyCollection<ICard> Cards =>
        throw new NotSupportedException("Getting cards is not allowed!");
}