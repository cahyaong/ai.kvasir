// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefinedBlob.Card.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, December 27, 2019 7:22:02 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System;

public static partial class DefinedBlob
{
    public record Card
    {
        public ushort Number { get; init; }

        public string SetCode { get; init; } = DefinedText.Unknown;

        public uint MultiverseId { get; init; }

        public string Name { get; init; } = DefinedText.Unknown;

        public CardKind Kind { get; init; } = CardKind.Unknown;

        public CardSuperKind SuperKind { get; init; } = CardSuperKind.Unknown;

        public CardSubKind[] SubKinds { get; init; } = Array.Empty<CardSubKind>();

        public bool IsTribal { get; init; }

        public Cost Cost { get; init; } = Cost.Unknown;

        public ushort Power { get; init; }

        public ushort Toughness { get; init; }

        public Ability[] Abilities { get; init; } = Array.Empty<Ability>();
    }
}