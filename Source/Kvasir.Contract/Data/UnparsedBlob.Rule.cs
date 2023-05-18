// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnparsedBlob.Rule.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, January 10, 2020 7:54:28 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using nGratis.Cop.Olympus.Contract;

public static partial class UnparsedBlob
{
    public record Rule
    {
        public string Id { get; init; } = DefinedText.Unknown;

        public string Text { get; init; } = DefinedText.Unknown;
    }
}