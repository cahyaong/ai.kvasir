// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICard.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, June 4, 2022 6:27:26 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System.Collections.Generic;

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