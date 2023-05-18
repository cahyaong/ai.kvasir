// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnparsedBlob.Card.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, December 28, 2019 6:09:39 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System.Diagnostics;
using nGratis.Cop.Olympus.Contract;

public static partial class UnparsedBlob
{
    [DebuggerDisplay("[{this.MultiverseId}]: {this.Name ?? \"<undefined>\"}")]
    public record Card
    {
        public int MultiverseId { get; init; }

        public string ScryfallId { get; init; } = DefinedText.Unknown;

        public string ScryfallImageUrl { get; init; } = DefinedText.Unknown;

        public string SetCode { get; init; } = DefinedText.Unknown;

        public string Name { get; init; } = DefinedText.Unknown;

        public string ManaCost { get; init; } = string.Empty;

        public string Type { get; init; } = DefinedText.Unknown;

        public string Rarity { get; init; } = DefinedText.Unknown;

        public string Text { get; init; } = DefinedText.Unknown;

        public string FlavorText { get; init; } = DefinedText.Unknown;

        public string Power { get; init; } = string.Empty;

        public string Toughness { get; init; } = string.Empty;

        public string Number { get; init; } = DefinedText.Unknown;

        public string Artist { get; init; } = DefinedText.Unknown;
    }
}