// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataExtensions.cs" company="nGratis">
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
// <creation_timestamp>Friday, November 26, 2021 10:25:14 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace nGratis.AI.Kvasir.Engine;

using System.Linq;
using Moq;
using nGratis.Cop.Olympus.Contract;

public static class DataExtensions
{
    public static Creature CreateAttacker(this Tabletop tabletop, string name, int power, int toughness)
    {
        Guard
            .Require(power, nameof(power))
            .Is.GreaterThanOrEqualTo(1);

        Guard
            .Require(toughness, nameof(toughness))
            .Is.GreaterThanOrEqualTo(1);

        var attacker = new Creature(name)
        {
            Power = power,
            Toughness = toughness,
            Owner = tabletop.ActivePlayer,
            Controller = tabletop.ActivePlayer
        };

        tabletop.Battlefield.AddCardToTop(attacker);

        return attacker;
    }

    public static Creature CreateBlocker(this Tabletop tabletop, string name, int power, int toughness)
    {
        Guard
            .Require(power, nameof(power))
            .Is.GreaterThanOrEqualTo(1);

        Guard
            .Require(toughness, nameof(toughness))
            .Is.GreaterThanOrEqualTo(1);

        var blocker = new Creature(name)
        {
            Power = power,
            Toughness = toughness,
            Owner = tabletop.NonactivePlayer,
            Controller = tabletop.NonactivePlayer
        };

        tabletop.Battlefield.AddCardToTop(blocker);

        return blocker;
    }

    public static Player WithAttackingDecision(this Player player, params Creature[] attackers)
    {
        Guard
            .Require(player, nameof(player))
            .Is.Not.Null();

        Guard
            .Require(attackers, nameof(attackers))
            .Is.Not.Empty();

        var attackDecision = new AttackingDecision
        {
            Attackers = attackers
        };

        var mockStrategy = MockBuilder.CreateMock<IStrategy>();

        mockStrategy
            .Setup(mock => mock.DeclareAttacker(Arg.IsAny<Tabletop>()))
            .Returns(attackDecision);

        player.Strategy = mockStrategy.Object;

        return player;
    }

    public static Player WithBlockingDecision(this Player player, params Combat[] combats)
    {
        Guard
            .Require(player, nameof(player))
            .Is.Not.Null();

        var blockingDecision = combats.Any()
            ? new BlockingDecision { Combats = combats }
            : BlockingDecision.None;

        var mockStrategy = MockBuilder.CreateMock<IStrategy>();

        mockStrategy
            .Setup(mock => mock.DeclareBlocker(Arg.IsAny<Tabletop>()))
            .Returns(blockingDecision);

        player.Strategy = mockStrategy.Object;

        return player;
    }
}