// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPlayer.Default.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Monday, July 1, 2024 12:24:43 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System;

public sealed class UnknownPlayer : IPlayer
{
    private UnknownPlayer()
    {
    }

    public static UnknownPlayer Instance { get; } = new();

    public PlayerKind Kind => PlayerKind.Unknown;

    public string Name => DefinedText.Unknown;

    public IDeck Deck => UnknownDeck.Instance;

    public IStrategy Strategy => UnknownStrategy.Instance;

    public IZone<ICard> Library => UnknownZone<ICard>.Instance;

    public IZone<ICard> Hand => UnknownZone<ICard>.Instance;

    public IZone<ICard> Graveyard => UnknownZone<ICard>.Instance;

    public IManaPool ManaPool => UnknownManaPool.Instance;

    public int Life
    {
        get => throw new NotSupportedException("Getting life is not allowed!");
        set => throw new NotSupportedException("Setting life is not allowed!");
    }

    public IAttackingDecision AttackingDecision
    {
        get => throw new NotSupportedException("Getting attacking decision is not allowed!");
        set => throw new NotSupportedException("Setting attacking decision is not allowed!");
    }

    public IBlockingDecision BlockingDecision
    {
        get => throw new NotSupportedException("Getting blocking decision is not allowed!");
        set => throw new NotSupportedException("Setting blocking decision is not allowed!");
    }
}

public sealed class NonePlayer : IPlayer
{
    private NonePlayer()
    {
    }

    public static NonePlayer Instance { get; } = new();

    public PlayerKind Kind => PlayerKind.None;

    public string Name => DefinedText.None;

    public IDeck Deck => UnknownDeck.Instance;

    public IStrategy Strategy => UnknownStrategy.Instance;

    public IZone<ICard> Library => UnknownZone<ICard>.Instance;

    public IZone<ICard> Hand => UnknownZone<ICard>.Instance;

    public IZone<ICard> Graveyard => UnknownZone<ICard>.Instance;

    public IManaPool ManaPool => UnknownManaPool.Instance;

    public int Life
    {
        get => -42;
        set => throw new NotSupportedException("Setting life is not allowed!");
    }

    public IAttackingDecision AttackingDecision
    {
        get => NoneAttackingDecision.Instance;
        set => throw new NotSupportedException("Setting attacking decision is not allowed!");
    }

    public IBlockingDecision BlockingDecision
    {
        get => NoneBlockingDecision.Instance;
        set => throw new NotSupportedException("Setting blocking decision is not allowed!");
    }
}