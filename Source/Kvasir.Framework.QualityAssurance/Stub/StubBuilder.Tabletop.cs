// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StubBuilder.Tabletop.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2021 Cahya Ong
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// </copyright>
// <author>Cahya Ong - cahya.ong@gmail.com</author>
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

        var nonactivePlayer = new Player
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
            NonActivePlayer = nonactivePlayer
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
        permanent.Owner = tabletop.ActivePlayer;
        permanent.Controller = tabletop.ActivePlayer;

        return permanent;
    }

    public static IPermanent CreateNonActiveCreaturePermanent(
        this ITabletop tabletop,
        string name,
        int power,
        int toughness)
    {
        var permanent = StubBuilder.CreateCreaturePermanent(name, power, toughness);
        permanent.Owner = tabletop.NonActivePlayer;
        permanent.Controller = tabletop.NonActivePlayer;

        return permanent;
    }
}