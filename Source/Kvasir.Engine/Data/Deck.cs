// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Deck.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, May 1, 2021 6:33:23 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using nGratis.AI.Kvasir.Contract;

public class Deck : IDeck
{
    public Deck()
    {
        this.Cards = Array.Empty<ICard>();
    }

    public static IDeck Unknown => UnknownDeck.Instance;

    public IReadOnlyCollection<ICard> Cards { get; init; }
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