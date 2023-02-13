﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StubBuilder.cs" company="nGratis">
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
// <creation_timestamp>Friday, November 26, 2021 10:47:20 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Framework;

using System.Collections.Generic;
using System.Linq;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Engine;

public static partial class StubBuilder
{
    public static IPlayer CreateDefaultPlayer(string name)
    {
        return StubBuilder.CreateDefaultPlayer(name, Strategy.Noop);
    }

    public static IPlayer CreateDefaultPlayer(string name, IStrategy strategy)
    {
        return new Player
        {
            Name = name,
            Kind = PlayerKind.Testing,
            Strategy = strategy,
            Life = 20
        };
    }

    public static ICard CreateLandCard(string name)
    {
        return new Card
        {
            Name = name,
            Kind = CardKind.Land
        };
    }

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

    public static IAction CreateStubAction(string name, IPlayer owner)
    {
        var card = new Card
        {
            Name = name,
            Kind = CardKind.Stub
        };

        var action = Action.PlayStub(card);
        action.Owner = owner;

        return action;
    }

    private static IPermanent CreateCreaturePermanent(string name, int power, int toughness)
    {
        var card = new Card
        {
            Name = name,
            Kind = CardKind.Creature,
            Power = power,
            Toughness = toughness
        };

        return card.AsPermanent();
    }
}