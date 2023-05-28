﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Action.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Wednesday, June 1, 2022 12:37:45 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Diagnostics;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

[DebuggerDisplay("<Action> {this.Kind}")]
public class Action : IAction
{
    private Action()
    {
        this.Kind = ActionKind.Unknown;
        this.Owner = Player.Unknown;
        this.Cost = Engine.Cost.Unknown;
        this.Target = Engine.Target.Unknown;
        this.Parameter = Engine.Parameter.Unknown;
    }

    public static IAction Unknown => UnknownAction.Instance;

    public int Id => this.GetHashCode();

    public string Name => DefinedText.Unsupported;

    public ActionKind Kind { get; private init; }

    public IPlayer Owner { get; set; }

    public ICost Cost { get; private init; }

    public ITarget Target { get; private init; }

    public IParameter Parameter { get; set; }

    public static IAction Pass()
    {
        return new Action
        {
            Kind = ActionKind.Passing,
            Cost = Engine.Cost.None,
            Target = Engine.Target.None,
            Parameter = Engine.Parameter.None
        };
    }

    public static IAction PlayCard(ICard card)
    {
        return new Action
        {
            Kind = card.Kind == CardKind.Land
                ? ActionKind.PlayingLand
                : ActionKind.PlayingNonLand,
            Cost = card.Cost,
            Target = new Target
            {
                Cards = new[] { card }
            },
            Parameter = Engine.Parameter.None
        };
    }

    public static IAction Discard(params ICard[] cards)
    {
        return new Action
        {
            Kind = ActionKind.Discarding,
            Cost = Engine.Cost.None,
            Target = new Target
            {
                Cards = cards
            }
        };
    }

    internal static IAction PlayStub(ICard card)
    {
        return new Action
        {
            Kind = ActionKind.PlayingStub,
            Cost = card.Cost,
            Target = new Target
            {
                Cards = new[] { card }
            },
            Parameter = Engine.Parameter.None
        };
    }
}

internal sealed class UnknownAction : IAction
{
    private UnknownAction()
    {
    }

    internal static UnknownAction Instance { get; } = new();

    public int Id => -42;

    public string Name => DefinedText.Unknown;

    public ActionKind Kind => ActionKind.Unknown;

    public IPlayer Owner
    {
        get => Player.Unknown;
        set => throw new NotSupportedException("Setting owner is not allowed!");
    }

    public ICost Cost => Engine.Cost.Unknown;

    public ITarget Target => Engine.Target.Unknown;

    public IParameter Parameter
    {
        get => Engine.Parameter.Unknown;
        set => throw new NotSupportedException("Setting parameter is not allowed!");
    }
}