// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICard.Default.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Monday, July 8, 2024 12:29:58 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System;
using System.Collections.Generic;

public sealed class UnknownCard : ICard
{
    private UnknownCard()
    {
    }

    public static UnknownCard Instance { get; } = new();

    public int Id => -42;

    public string Name => DefinedText.Unknown;

    public CardKind Kind => CardKind.Unknown;

    public CardSuperKind SuperKind => CardSuperKind.Unknown;

    public IReadOnlyCollection<CardSubKind> SubKinds =>
        throw new NotSupportedException("Getting sub kinds is not allowed!");

    public ICost Cost => UnknownCost.Instance;

    public int Power =>
        throw new NotSupportedException("Getting power is not allowed!");

    public int Toughness =>
        throw new NotSupportedException("Getting toughness is not allowed!");

    public IReadOnlyCollection<IAbility> Abilities =>
        throw new NotSupportedException("Getting abilities is not allowed!");
}