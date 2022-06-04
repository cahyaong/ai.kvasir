// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPlayer.cs" company="nGratis">
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
// <creation_timestamp>Friday, April 15, 2022 2:47:31 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public interface IPlayer
{
    PlayerKind Kind { get; }

    string Name { get; }

    IDeck Deck { get; }

    IStrategy Strategy { get; }

    IZone<ICard> Library { get; }

    IZone<ICard> Hand { get; }

    IZone<ICard> Graveyard { get; }

    int Life { get; set; }
}

public sealed class UnknownPlayer : IPlayer
{
    private UnknownPlayer()
    {
    }

    internal static IPlayer Instance { get; } = new UnknownPlayer();

    public PlayerKind Kind => PlayerKind.Unknown;

    public string Name => DefinedText.Unknown;

    public IDeck Deck => Engine.Deck.Unknown;

    public IStrategy Strategy => Engine.Strategy.Unknown;

    public IZone<ICard> Library => Zone<ICard>.Unknown;

    public IZone<ICard> Hand => Zone<ICard>.Unknown;

    public IZone<ICard> Graveyard => Zone<ICard>.Unknown;

    public int Life
    {
        get => -42;
        set => throw new NotSupportedException("Setting life is not allowed!");
    }
}

internal sealed class NonePlayer : IPlayer
{
    private NonePlayer()
    {
    }

    internal static IPlayer Instance { get; } = new NonePlayer();

    public PlayerKind Kind => PlayerKind.None;

    public string Name => DefinedText.None;

    public IDeck Deck => Engine.Deck.Unknown;

    public IStrategy Strategy => Engine.Strategy.Unknown;

    public IZone<ICard> Library => Zone<ICard>.Unknown;

    public IZone<ICard> Hand => Zone<ICard>.Unknown;

    public IZone<ICard> Graveyard => Zone<ICard>.Unknown;

    public int Life
    {
        get => -42;
        set => throw new NotSupportedException("Setting life is not allowed!");
    }
}