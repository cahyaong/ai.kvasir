// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Card.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, June 4, 2022 6:27:52 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using nGratis.AI.Kvasir.Contract;

[DebuggerDisplay("<Card> {this.Name} ({this.Id})")]
public class Card : ICard
{
    public Card()
    {
        this.Name = DefinedText.Unknown;
        this.Kind = CardKind.Unknown;
        this.SuperKind = CardSuperKind.Unknown;
        this.SubKinds = Array.Empty<CardSubKind>();
        this.Cost = Engine.Cost.Unknown;
        this.Power = -42;
        this.Toughness = -42;
        this.Abilities = Array.Empty<IAbility>();
    }

    public static ICard Unknown => UnknownCard.Instance;

    public int Id => this.GetHashCode();

    public string Name { get; init; }

    public CardKind Kind { get; init; }

    public CardSuperKind SuperKind { get; init; }

    public IReadOnlyCollection<CardSubKind> SubKinds { get; init; }

    public ICost Cost { get; init; }

    public int Power { get; init; }

    public int Toughness { get; init; }

    public IReadOnlyCollection<IAbility> Abilities { get; init; }
}