// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITarget.Default.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, July 5, 2024 12:27:12 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System;
using System.Collections.Generic;

public sealed class UnknownTarget : ITarget
{
    private UnknownTarget()
    {
    }

    public static UnknownTarget Instance { get; } = new();

    public IPlayer Player
    {
        get => UnknownPlayer.Instance;
        set => throw new NotSupportedException("Setting player is not allowed!");
    }

    public IReadOnlyCollection<ICard> Cards =>
        throw new NotSupportedException("Getting cards is not allowed!");

    public IReadOnlyCollection<IPermanent> Permanents =>
        throw new NotSupportedException("Getting permanents is not allowed!");
}

public sealed class NoneTarget : ITarget
{
    private NoneTarget()
    {
    }

    public static NoneTarget Instance { get; } = new();

    public IPlayer Player
    {
        get => NonePlayer.Instance;
        set => throw new NotSupportedException("Setting player is not allowed!");
    }

    public IReadOnlyCollection<ICard> Cards { get; } = [];

    public IReadOnlyCollection<IPermanent> Permanents { get; } = [];
}