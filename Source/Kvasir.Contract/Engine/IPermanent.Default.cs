// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPermanent.Default.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Monday, July 8, 2024 12:28:44 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System;

public sealed class UnknownPermanent : IPermanent
{
    private UnknownPermanent()
    {
    }

    public static UnknownPermanent Instance { get; } = new();

    public int Id => -42;

    public string Name => DefinedText.Unknown;

    public ICard Card => UnknownCard.Instance;

    public IPlayer OwningPlayer
    {
        get => UnknownPlayer.Instance;
        set => throw new NotSupportedException("Setting owning player is not allowed!");
    }

    public IPlayer ControllingPlayer
    {
        get => UnknownPlayer.Instance;
        set => throw new NotSupportedException("Setting controlling player is not allowed!");
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

    public bool HasPart<TPart>() where TPart : IPart =>
        throw new NotSupportedException("Checking has part flag is not allowed!");

    public TPart FindPart<TPart>() where TPart : IPart =>
        throw new NotSupportedException("Finding part is not allowed!");
}