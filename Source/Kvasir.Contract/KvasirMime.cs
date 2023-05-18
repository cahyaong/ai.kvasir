// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KvasirMime.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, 22 November 2018 9:10:01 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using nGratis.Cop.Olympus.Contract;

public static class KvasirMime
{
    public static readonly Mime Blob = new("application/x-ng-kvasir-blob", "ngkblob");

    public static readonly Mime Cache = new("application/x-ng-kvasir-cache", "ngkcache");

    public static readonly Mime Deck = new("application/x-ng-kvasir-deck", "ngkdeck");
}