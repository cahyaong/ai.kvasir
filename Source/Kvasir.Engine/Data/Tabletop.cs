// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Tabletop.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Wednesday, 23 January 2019 10:45:26 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using nGratis.AI.Kvasir.Contract;

public class Tabletop : ITabletop
{
    public Tabletop()
    {
        this.Players = ImmutableArray<IPlayer>.Empty;

        this.Battlefield = new Zone<IPermanent>
        {
            Kind = ZoneKind.Battlefield,
            Visibility = Visibility.Public
        };

        this.Stack = new Zone<IAction>
        {
            Kind = ZoneKind.Stack,
            Visibility = Visibility.Public
        };

        this.Exile = new Zone<ICard>
        {
            Kind = ZoneKind.Exile,
            Visibility = Visibility.Public
        };

        this.TurnId = -1;
        this.Phase = Phase.Unknown;

        this.ActivePlayer = Player.Unknown;
        this.NonActivePlayer = Player.Unknown;
        this.PrioritizedPlayer = Player.Unknown;

        this.AttackingDecision = Engine.AttackingDecision.Unknown;
        this.BlockingDecision = Engine.BlockingDecision.Unknown;
    }

    public static ITabletop Unknown => UnknownTabletop.Instance;

    public IReadOnlyCollection<IPlayer> Players { get; init; }

    public IZone<IPermanent> Battlefield { get; }

    public IZone<IAction> Stack { get; }

    public IZone<ICard> Exile { get; }

    public int TurnId { get; set; }

    public Phase Phase { get; set; }

    public IPlayer ActivePlayer { get; set; }

    public IPlayer NonActivePlayer { get; set; }

    public IPlayer PrioritizedPlayer { get; set; }

    public IAttackingDecision AttackingDecision { get; set; }

    public IBlockingDecision BlockingDecision { get; set; }

    public bool IsFirstTurn => this.TurnId <= 0;

    public bool IsActionPerformed { get; set; }

    public int PlayedLandCount { get; set; }
}

internal sealed class UnknownTabletop : ITabletop
{
    private UnknownTabletop()
    {
    }

    internal static UnknownTabletop Instance { get; } = new();

    public IReadOnlyCollection<IPlayer> Players => throw new NotSupportedException("Getting players is not allowed!");

    public IZone<IPermanent> Battlefield => Zone<IPermanent>.Unknown;

    public IZone<IAction> Stack => Zone<IAction>.Unknown;

    public IZone<ICard> Exile => Zone<ICard>.Unknown;

    public int TurnId
    {
        get => throw new NotSupportedException("Getting turn ID is not allowed!");
        set => throw new NotSupportedException("Setting turn ID is not allowed!");
    }

    public Phase Phase
    {
        get => Phase.Unknown;
        set => throw new NotSupportedException("Setting phase is not allowed!");
    }

    public IPlayer ActivePlayer
    {
        get => Player.Unknown;
        set => throw new NotSupportedException("Setting active player is not allowed!");
    }

    public IPlayer NonActivePlayer
    {
        get => Player.Unknown;
        set => throw new NotSupportedException("Setting nonactive player is not allowed!");
    }

    public IPlayer PrioritizedPlayer
    {
        get => Player.Unknown;
        set => throw new NotSupportedException("Setting prioritized player is not allowed!");
    }

    public IAttackingDecision AttackingDecision
    {
        get => Engine.AttackingDecision.Unknown;
        set => throw new NotSupportedException("Setting attacking decision is not allowed!");
    }

    public IBlockingDecision BlockingDecision
    {
        get => Engine.BlockingDecision.Unknown;
        set => throw new NotSupportedException("Setting blocking decision is not allowed!");
    }

    public bool IsFirstTurn => throw new NotSupportedException("Getting is first turn flag is not allowed");

    public int PlayedLandCount
    {
        get => throw new NotSupportedException("Getting played land count is not allowed!");
        set => throw new NotSupportedException("Setting played land count is not allowed!");
    }
}