// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StubBuilder.Zone.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, February 25, 2023 2:27:55 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Framework;

using System.Collections.Generic;
using System.Linq;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Engine;

public static partial class StubBuilder
{
    public static IZone<ICard> AddLandCard(this IZone<ICard> zone, string nameInfix, int startingIndex, int count)
    {
        Enumerable
            .Range(startingIndex, count)
            .Select(index => StubBuilder.CreateLandCard($"[_MOCK_LAND__{nameInfix}_{index:D2}_]"))
            .OrderByDescending(card => card.Name)
            .ForEach(zone.AddToTop);

        return zone;
    }

    public static IZone<ICard> AddStubCard(this IZone<ICard> zone, string nameInfix, int startingIndex, int count)
    {
        Enumerable
            .Range(startingIndex, count)
            .Select(index => new Card
            {
                Name = $"[_MOCK_STUB__{nameInfix}_{index:D2}_]",
                Kind = CardKind.Stub,
            })
            .OrderByDescending(card => card.Name)
            .ForEach(zone.AddToTop);

        return zone;
    }

    public static IZone<IPermanent> AddDefaultCreaturePermanent(
        this IZone<IPermanent> zone,
        string name,
        IPlayer owner,
        IPlayer controller,
        bool isTapped)
    {
        var permanent = StubBuilder.CreateCreaturePermanent(name, 1, 1);
        permanent.Owner = owner;
        permanent.Controller = controller;
        permanent.IsTapped = isTapped;

        zone.AddToTop(permanent);

        return zone;
    }
}