// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StubBuilder.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, November 26, 2021 10:47:20 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Framework;

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

    public static ICard CreateStubCard(string name)
    {
        return new Card
        {
            Name = name,
            Kind = CardKind.Stub
        };
    }

    public static ICard CreateLandCard(string name)
    {
        return new Card
        {
            Name = name,
            Kind = CardKind.Land,
            Cost = new Cost
            {
                Kind = CostKind.PayingMana,
                Parameter = Parameter.Builder
                    .Create()
                    .WithValue(ParameterKey.Amount, ManaCost.Free)
                    .Build()
            }
        };
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