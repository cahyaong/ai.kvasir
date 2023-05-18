// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICostTarget.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, April 15, 2023 4:46:13 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace nGratis.AI.Kvasir.Engine;

public interface ICostTarget
{
    IPlayer Player { get; set; }
}

internal sealed class UnknownCostTarget : ICostTarget
{
    private UnknownCostTarget()
    {
    }

    public static UnknownCostTarget Instance { get; } = new();

    public IPlayer Player
    {
        get => Engine.Player.Unknown;
        set => throw new NotSupportedException("Setting player is not allowed!");
    }
}

internal sealed class NoneCostTarget : ICostTarget
{
    private NoneCostTarget()
    {
    }

    public static NoneCostTarget Instance { get; } = new();

    public IPlayer Player
    {
        get => Engine.Player.None;
        set => throw new NotSupportedException("Setting player is not allowed!");
    }
}