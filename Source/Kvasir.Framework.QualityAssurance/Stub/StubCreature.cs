// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StubCreature.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, November 5, 2021 5:54:33 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Framework;

using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Engine;

// NOTE: This <StubCreature> class will be used to represent the mocking data in `*.ngkcard` file!

public class StubCreature
{
    public StubCreature()
    {
        this.Name = DefinedText.Unknown;
        this.HasSummoningSickness = false;
        this.IsTapped = false;
    }

    public string Name { get; init; }

    public bool HasSummoningSickness { get; init; }

    public bool IsTapped { get; init; }

    public IPermanent AsPermanent()
    {
        var card = new Card
        {
            Name = this.Name,
            Kind = CardKind.Creature,
            Power = 1,
            Toughness = 1
        };

        var permanent = card.AsPermanent();
        permanent.IsTapped = IsTapped;

        var creature = permanent.ToProxyCreature();
        creature.HasSummoningSickness = this.HasSummoningSickness;

        return permanent;
    }
}