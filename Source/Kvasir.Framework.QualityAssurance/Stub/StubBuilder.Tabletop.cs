// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StubBuilder.Tabletop.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Sunday, September 18, 2022 5:52:20 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Framework;

using System.Collections.Generic;
using System.Linq;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Engine;

public static partial class StubBuilder
{
    public static ITabletop CreateDefaultTabletop()
    {
        return StubBuilder.CreateDefaultTabletop(Strategy.Noop, Strategy.Noop);
    }

    public static ITabletop CreateDefaultTabletop(IStrategy attackingStrategy, IStrategy blockingStrategy)
    {
        var activePlayer = new Player
        {
            Name = "[_MOCK_PLAYER__ACTIVE_]",
            Kind = PlayerKind.Testing,
            Strategy = attackingStrategy,
            Life = 20
        };

        var nonActivePlayer = new Player
        {
            Name = "[_MOCK_PLAYER__NONACTIVE_]",
            Kind = PlayerKind.Testing,
            Strategy = blockingStrategy,
            Life = 20
        };

        return new Tabletop
        {
            TurnId = 0,
            Phase = Phase.Beginning,
            ActivePlayer = activePlayer,
            NonActivePlayer = nonActivePlayer
        };
    }

    public static ITabletop WithDefaultLibrary(this ITabletop tabletop)
    {
        Enumerable
            .Range(0, 10)
            .Select(index => new Card
            {
                Name = $"[_MOCK_STUB__ACTIVE_{index:D2}_]",
                Kind = CardKind.Stub
            })
            .ForEach(card => tabletop.ActivePlayer.Library.AddToTop(card));

        Enumerable
            .Range(0, 10)
            .Select(index => new Card
            {
                Name = $"[_MOCK_STUB__NONACTIVE_{index:D2}_]",
                Kind = CardKind.Stub
            })
            .ForEach(card => tabletop.NonActivePlayer.Library.AddToTop(card));

        return tabletop;
    }

    public static IPermanent CreateActiveCreaturePermanent(
        this ITabletop tabletop,
        string name,
        int power,
        int toughness)
    {
        var permanent = StubBuilder.CreateCreaturePermanent(name, power, toughness);
        permanent.OwningPlayer = tabletop.ActivePlayer;
        permanent.ControllingPlayer = tabletop.ActivePlayer;

        return permanent;
    }

    public static IPermanent CreateNonActiveCreaturePermanent(
        this ITabletop tabletop,
        string name,
        int power,
        int toughness)
    {
        var permanent = StubBuilder.CreateCreaturePermanent(name, power, toughness);
        permanent.OwningPlayer = tabletop.NonActivePlayer;
        permanent.ControllingPlayer = tabletop.NonActivePlayer;

        return permanent;
    }

    public static IPermanent WithoutSummoningSickness(this IPermanent permanent)
    {
        if (permanent.Card.Kind != CardKind.Creature)
        {
            return permanent;
        }

        permanent
            .FindPart<CreaturePart>()
            .HasSummoningSickness = false;

        return permanent;
    }
}