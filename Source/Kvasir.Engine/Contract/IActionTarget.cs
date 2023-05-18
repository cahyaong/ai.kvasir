// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IActionTarget.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Wednesday, July 6, 2022 6:09:23 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;

public interface IActionTarget
{
    IPlayer Player { get; set; }

    IReadOnlyCollection<ICard> Cards { get; }

    // TODO (SHOULD): Implement ability as another kind of action source!
}

internal sealed class UnknownActionTarget : IActionTarget
{
    private UnknownActionTarget()
    {
    }

    internal static UnknownActionTarget Instance { get; } = new();

    public IPlayer Player
    {
        get => Engine.Player.Unknown;
        set => throw new NotSupportedException("Setting player is not allowed!");
    }

    public IReadOnlyCollection<ICard> Cards =>
        throw new NotSupportedException("Getting cards is not allowed!");
}

internal sealed class NoneActionTarget : IActionTarget
{
    private NoneActionTarget()
    {
    }

    internal static NoneActionTarget Instance { get; } = new();

    public IPlayer Player
    {
        get => Engine.Player.None;
        set => throw new NotSupportedException("Setting player is not allowed!");
    }

    public IReadOnlyCollection<ICard> Cards => Array.Empty<ICard>();
}