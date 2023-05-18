// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPlayer.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
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

    IManaPool ManaPool { get; }

    int Life { get; set; }
}

internal sealed class UnknownPlayer : IPlayer
{
    private UnknownPlayer()
    {
    }

    internal static UnknownPlayer Instance { get; } = new();

    public PlayerKind Kind => PlayerKind.Unknown;

    public string Name => DefinedText.Unknown;

    public IDeck Deck => Engine.Deck.Unknown;

    public IStrategy Strategy => Engine.Strategy.Unknown;

    public IZone<ICard> Library => Zone<ICard>.Unknown;

    public IZone<ICard> Hand => Zone<ICard>.Unknown;

    public IZone<ICard> Graveyard => Zone<ICard>.Unknown;

    public IManaPool ManaPool => Engine.ManaPool.Unknown;

    public int Life
    {
        get => throw new NotSupportedException("Getting life is not allowed!");
        set => throw new NotSupportedException("Setting life is not allowed!");
    }
}

internal sealed class NonePlayer : IPlayer
{
    private NonePlayer()
    {
    }

    internal static NonePlayer Instance { get; } = new();

    public PlayerKind Kind => PlayerKind.None;

    public string Name => DefinedText.None;

    public IDeck Deck => Engine.Deck.Unknown;

    public IStrategy Strategy => Engine.Strategy.Unknown;

    public IZone<ICard> Library => Zone<ICard>.Unknown;

    public IZone<ICard> Hand => Zone<ICard>.Unknown;

    public IZone<ICard> Graveyard => Zone<ICard>.Unknown;

    public IManaPool ManaPool => Engine.ManaPool.Unknown;

    public int Life
    {
        get => -42;
        set => throw new NotSupportedException("Setting life is not allowed!");
    }
}