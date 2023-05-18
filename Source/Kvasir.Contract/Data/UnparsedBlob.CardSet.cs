// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UnparsedBlob.CardSet.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, December 28, 2019 6:11:22 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System;
using System.Diagnostics;
using nGratis.Cop.Olympus.Contract;

public static partial class UnparsedBlob
{
    [DebuggerDisplay("[{this.Code ?? \"XXX\"}] {this.Name ?? \"<undefined>\"}")]
    public record CardSet
    {
        public string Code { get; init; } = DefinedText.Unknown;

        public string Name { get; init; } = DefinedText.Unknown;

        public DateTime ReleasedTimestamp { get; init; } = DateTime.MaxValue;
    }
}