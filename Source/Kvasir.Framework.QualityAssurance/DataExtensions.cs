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

using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public static class DataExtensions
{
    public static ICard CreateActiveCreature(this ITabletop tabletop, string name, int power, int toughness)
    {
        var creature = tabletop.CreateCreature(name, power, toughness);

        creature.Owner = tabletop.ActivePlayer;
        creature.Controller = tabletop.ActivePlayer;

        return creature;
    }

    public static ICard CreateNonactiveCreature(this ITabletop tabletop, string name, int power, int toughness)
    {
        var creature = tabletop.CreateCreature(name, power, toughness);

        creature.Owner = tabletop.NonactivePlayer;
        creature.Controller = tabletop.NonactivePlayer;

        return creature;
    }

    private static ICard CreateCreature(this ITabletop tabletop, string name, int power, int toughness)
    {
        Guard
            .Require(power, nameof(power))
            .Is.GreaterThanOrEqualTo(0);

        Guard
            .Require(toughness, nameof(toughness))
            .Is.GreaterThanOrEqualTo(1);

        var creature = new Card
        {
            Kind = CardKind.Creature,
            Name = name
        };

        creature.AddParts(new CreaturePart
        {
            Power = power,
            Toughness = toughness,
            HasSummoningSickness = false,
            Damage = 0
        });

        creature.AddParts(new PermanentPart
        {
            IsTapped = false
        });

        tabletop.Battlefield.AddCardToTop(creature);

        return creature;
    }
}