// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICard.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, June 4, 2022 6:27:26 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public interface ICard : IDiagnostic
{
    // TODO (SHOULD): Implement the concept of owner and controller!

    CardKind Kind { get; }

    CardSuperKind SuperKind { get; }

    IReadOnlyCollection<CardSubKind> SubKinds { get; }

    ICost Cost { get; }

    int Power { get; }

    int Toughness { get; }

    IReadOnlyCollection<IAbility> Abilities { get; }
}

internal sealed class UnknownCard : ICard
{
    private UnknownCard()
    {
    }

    internal static UnknownCard Instance { get; } = new();

    public int Id => -42;

    public string Name => DefinedText.Unknown;

    public CardKind Kind => CardKind.Unknown;

    public CardSuperKind SuperKind => CardSuperKind.Unknown;

    public IReadOnlyCollection<CardSubKind> SubKinds =>
        throw new NotSupportedException("Getting sub kinds is not allowed!");

    public ICost Cost => Engine.Cost.Unknown;

    public int Power =>
        throw new NotSupportedException("Getting power is not allowed!");

    public int Toughness =>
        throw new NotSupportedException("Getting toughness is not allowed!");

    public IReadOnlyCollection<IAbility> Abilities =>
        throw new NotSupportedException("Getting abilities is not allowed!");
}