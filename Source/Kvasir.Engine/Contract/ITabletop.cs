// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITabletop.cs" company="nGratis">
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
// <creation_timestamp>Saturday, April 16, 2022 5:18:03 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;

public interface ITabletop
{
    IZone<IPermanent> Battlefield { get; }

    IZone<IAction> Stack { get; }

    IZone<ICard> Exile { get; }

    int TurnId { get; set; }

    Phase Phase { get; set; }

    IPlayer ActivePlayer { get; set; }

    IPlayer NonActivePlayer { get; set; }

    IPlayer PrioritizedPlayer { get; set; }

    IAttackingDecision AttackingDecision { get; set; }

    IBlockingDecision BlockingDecision { get; set; }

    bool IsFirstTurn { get; }

    int PlayedLandCount { get; set; }
}

internal sealed class UnknownTabletop : ITabletop
{
    private UnknownTabletop()
    {
    }

    internal static UnknownTabletop Instance { get; } = new();

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