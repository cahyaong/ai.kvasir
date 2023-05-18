// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPermanent.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, April 28, 2022 4:03:54 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using nGratis.Cop.Olympus.Contract;

public interface IPermanent : IDiagnostic
{
    ICard Card { get; }

    IPlayer Owner { get; set; }

    IPlayer Controller { get; set; }

    public bool IsTapped { get; set; }

    void AddPart(params IPart[] parts);

    void RemoveParts();

    TPart FindPart<TPart>() where TPart : IPart;
}

internal sealed class UnknownPermanent : IPermanent
{
    private UnknownPermanent()
    {
    }

    internal static UnknownPermanent Instance { get; } = new();

    public int Id => -42;

    public string Name => DefinedText.Unknown;

    public ICard Card => Engine.Card.Unknown;

    public IPlayer Owner
    {
        get => Player.Unknown;
        set => throw new NotSupportedException("Setting owner is not allowed!");
    }

    public IPlayer Controller
    {
        get => Player.Unknown;
        set => throw new NotSupportedException("Setting controller is not allowed!");
    }

    public bool IsTapped
    {
        get => throw new NotSupportedException("Getting is tapped flag is not allowed!");
        set => throw new NotSupportedException("Setting is tapped flag is not allowed!");
    }

    public void AddPart(params IPart[] _) =>
        throw new NotSupportedException("Adding part is not allowed!");

    public void RemoveParts() =>
        throw new NotSupportedException("Removing parts is not allowed!");

    public TPart FindPart<TPart>() where TPart : IPart =>
        throw new NotSupportedException("Finding part is not allowed!");
}