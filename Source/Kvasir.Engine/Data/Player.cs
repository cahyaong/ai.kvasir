// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Player.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2021 Cahya Ong
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// </copyright>
// <author>Cahya Ong - cahya.ong@gmail.com</author>
// <creation_timestamp>Wednesday, 23 January 2019 11:14:15 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Diagnostics;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

// TODO: Use builder pattern to make most properties immutable after creation!

[DebuggerDisplay("<Player> {this.Name}")]
public class Player : IPlayer
{
    public Player()
    {
        this.Kind = PlayerKind.Unknown;
        this.Name = DefinedText.Unknown;
        this.Deck = Engine.Deck.Unknown;
        this.Strategy = Engine.Strategy.Unknown;

        this.Library = new Zone<ICard>
        {
            Kind = ZoneKind.Library,
            Visibility = Visibility.Hidden
        };

        this.Hand = new Zone<ICard>
        {
            Kind = ZoneKind.Hand,
            Visibility = Visibility.Hidden
        };

        this.Graveyard = new Zone<ICard>
        {
            Kind = ZoneKind.Graveyard,
            Visibility = Visibility.Public
        };

        this.ManaPool = ManaBlob.Builder
            .Create()
            .Build();

        this.Life = 0;
    }

    public static IPlayer Unknown => UnknownPlayer.Instance;

    public static IPlayer None => NonePlayer.Instance;

    public PlayerKind Kind { get; init; }

    public string Name { get; init; }

    public IDeck Deck { get; init; }

    public IStrategy Strategy { get; init; }

    public IZone<ICard> Library { get; }

    public IZone<ICard> Hand { get; }

    public IZone<ICard> Graveyard { get; }

    public IManaPool ManaPool { get; }

    public int Life { get; set; }
}