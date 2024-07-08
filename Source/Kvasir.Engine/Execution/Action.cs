// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Action.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Wednesday, June 1, 2022 12:37:45 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Diagnostics;
using nGratis.AI.Kvasir.Contract;

[DebuggerDisplay("<Action> {this.Kind}")]
public class Action : IAction
{
    private Action()
    {
        this.Kind = ActionKind.Unknown;
        this.OwningPlayer = Player.Unknown;
        this.Cost = Engine.Cost.Unknown;
        this.Target = Engine.Target.Unknown;
        this.Parameter = Engine.Parameter.Unknown;
    }

    public static IAction Unknown => UnknownAction.Instance;

    public int Id => this.GetHashCode();

    public string Name => DefinedText.Unsupported;

    public ActionKind Kind { get; private init; }

    public IPlayer OwningPlayer { get; set; }

    public ICost Cost { get; private init; }

    public ITarget Target { get; private init; }

    public IParameter Parameter { get; set; }

    public static IAction Pass()
    {
        return new Action
        {
            Kind = ActionKind.Passing,
            Cost = Engine.Cost.None,
            Target = Engine.Target.None,
            Parameter = Engine.Parameter.None
        };
    }

    public static IAction PlayCard(ICard card)
    {
        return new Action
        {
            Kind = card.Kind == CardKind.Land
                ? ActionKind.PlayingLand
                : ActionKind.PlayingNonLand,
            Cost = card.Cost,
            Target = new Target
            {
                Cards = [card]
            },
            Parameter = Engine.Parameter.None
        };
    }

    public static IAction ActivateManaAbility(IPermanent permanent, IAbility ability)
    {
        return new Action
        {
            Kind = ActionKind.ActivatingManaAbility,
            Cost = ability.Cost,
            Target = new Target
            {
                Player = permanent.ControllingPlayer,
                Permanents = [permanent]
            },
            Parameter = Engine.Parameter.None
        };
    }

    public static IAction Discard(params ICard[] cards)
    {
        return new Action
        {
            Kind = ActionKind.Discarding,
            Cost = Engine.Cost.None,
            Target = new Target
            {
                Cards = cards
            }
        };
    }

    internal static IAction PlayStub(ICard card)
    {
        return new Action
        {
            Kind = ActionKind.PlayingStub,
            Cost = card.Cost,
            Target = new Target
            {
                Cards = [card]
            },
            Parameter = Engine.Parameter.None
        };
    }
}