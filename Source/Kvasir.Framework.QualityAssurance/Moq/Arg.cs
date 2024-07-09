// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Arg.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, 23 November 2018 9:26:00 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Framework;

using Moq;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public class Arg : Cop.Olympus.Framework.Arg
{
    public static class UnparsedCardSet
    {
        public static UnparsedBlob.CardSet Is(string code)
        {
            Guard
                .Require(code, nameof(code))
                .Is.Not.Empty();

            return Match.Create<UnparsedBlob.CardSet>(cardSet => cardSet.Code == code);
        }
    }

    public static class DefinedPlayer
    {
        public static DefinedBlob.Player Is(string name)
        {
            Guard
                .Require(name, nameof(name))
                .Is.Not.Empty();

            return Match.Create<DefinedBlob.Player>(agent => agent.Name == name);
        }
    }

    public static class Tabletop
    {
        public static ITabletop HasTurnWithId(int id)
        {
            Guard
                .Require(id, nameof(id))
                .Is.Positive();

            return Match.Create<ITabletop>(tabletop => tabletop.TurnId == id);
        }
    }
}