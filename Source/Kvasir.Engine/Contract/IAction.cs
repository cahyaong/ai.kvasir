// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAction.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Wednesday, June 1, 2022 12:30:36 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using nGratis.Cop.Olympus.Contract;

public interface IAction : IDiagnostic
{
    ActionKind Kind { get; }

    IPlayer Owner { get; set; }

    ICost Cost { get; }

    IActionTarget Target { get; }

    IParameter Parameter { get; set; }
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

    public IActionTarget Target => ActionTarget.Unknown;

    public IParameter Parameter
    {
        get => Engine.Parameter.Unknown;
        set => throw new NotSupportedException("Setting parameter is not allowed!");
    }
}