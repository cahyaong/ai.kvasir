// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefinedBlob.Player.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, December 27, 2019 7:42:46 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using nGratis.Cop.Olympus.Contract;

public static partial class DefinedBlob
{
    public record Player
    {
        public PlayerKind Kind { get; init; } = PlayerKind.Unknown;

        public string Name { get; init; } = DefinedText.Unknown;

        public string DeckCode { get; init; } = DefinedText.Unknown;
    }
}