// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Arg.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, 23 November 2018 9:26:00 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace Moq.AI.Kvasir;

using Moq;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public partial class Arg : Moq.Arg
{
    public class UnparsedCardSet
    {
        public static UnparsedBlob.CardSet Is(string code)
        {
            Guard
                .Require(code, nameof(code))
                .Is.Not.Empty();

            return Match.Create<UnparsedBlob.CardSet>(cardSet => cardSet.Code == code);
        }
    }

    public class DefinedPlayer
    {
        public static DefinedBlob.Player Is(string name)
        {
            Guard
                .Require(name, nameof(name))
                .Is.Not.Empty();

            return Match.Create<DefinedBlob.Player>(agent => agent.Name == name);
        }
    }
}