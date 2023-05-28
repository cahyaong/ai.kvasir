// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Target.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Wednesday, July 6, 2022 6:08:28 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using nGratis.AI.Kvasir.Contract;

public class Target : ITarget
{
    public Target()
    {
        this.Player = Engine.Player.Unknown;
        this.Cards = Array.Empty<ICard>();
    }

    public static ITarget Unknown => UnknownTarget.Instance;

    public static ITarget None => NoneTarget.Instance;

    public IPlayer Player { get; set; }

    public IReadOnlyCollection<ICard> Cards { get; init; }
}

internal sealed class UnknownTarget : ITarget
{
    private UnknownTarget()
    {
    }

    internal static UnknownTarget Instance { get; } = new();

    public IPlayer Player
    {
        get => Engine.Player.Unknown;
        set => throw new NotSupportedException("Setting player is not allowed!");
    }

    public IReadOnlyCollection<ICard> Cards =>
        throw new NotSupportedException("Getting cards is not allowed!");
}

internal sealed class NoneTarget : ITarget
{
    private NoneTarget()
    {
    }

    internal static NoneTarget Instance { get; } = new();

    public IPlayer Player
    {
        get => Engine.Player.None;
        set => throw new NotSupportedException("Setting player is not allowed!");
    }

    public IReadOnlyCollection<ICard> Cards => Array.Empty<ICard>();
}