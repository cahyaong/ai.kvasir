// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Class1.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, April 28, 2022 5:40:58 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using nGratis.AI.Kvasir.Contract;

public class CreaturePart : IPart
{
    internal CreaturePart()
    {
        this.Power = -42;
        this.Toughness = -42;
        this.HasSummoningSickness = false;
        this.Damage = -42;
    }

    public int Power { get; init; }

    public int Toughness { get; init; }

    public bool HasSummoningSickness { get; internal set; }

    public int Damage { get; internal set; }
}