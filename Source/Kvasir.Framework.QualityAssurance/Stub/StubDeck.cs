// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StubDeck.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Sunday, 3 February 2019 11:13:29 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Framework;

using System.Collections.Immutable;
using System.Linq;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Engine;

public class StubDeck : Deck
{
    public StubDeck(DefinedBlob.Deck definedDeck)
    {
        this.Cards = definedDeck
            .Entries
            .SelectMany(definedEntry => Enumerable
                .Range(0, definedDeck[definedEntry])
                .Select(_ => new Card
                {
                    Name = definedEntry.Name
                }))
            .ToImmutableArray();
    }
}