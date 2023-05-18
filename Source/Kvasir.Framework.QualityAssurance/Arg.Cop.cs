// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Arg.Cop.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, 23 November 2018 9:31:18 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace Moq.AI.Kvasir;

using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public partial class Arg
{
    public new class DataSpec : Moq.Arg.DataSpec
    {
        public static nGratis.Cop.Olympus.Contract.DataSpec IsKvasirCaching(string name)
        {
            Guard
                .Require(name, nameof(name))
                .Is.Not.Empty();

            return Match.Create<nGratis.Cop.Olympus.Contract.DataSpec>(spec =>
                spec.Name == name &&
                spec.Mime == KvasirMime.Cache);
        }
    }
}