// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAction.Default.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Monday, July 1, 2024 12:40:40 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System;

public sealed class UnknownAction : IAction
{
    private UnknownAction()
    {
    }

    public static UnknownAction Instance { get; } = new();

    public int Id => -42;

    public string Name => DefinedText.Unknown;

    public ActionKind Kind => ActionKind.Unknown;

    public IPlayer OwningPlayer
    {
        get => UnknownPlayer.Instance;
        set => throw new NotSupportedException("Setting owning player is not allowed!");
    }

    public ICost Cost => UnknownCost.Instance;

    public ITarget Target => UnknownTarget.Instance;

    public IParameter Parameter
    {
        get => UnknownParameter.Instance;
        set => throw new NotSupportedException("Setting parameter is not allowed!");
    }
}