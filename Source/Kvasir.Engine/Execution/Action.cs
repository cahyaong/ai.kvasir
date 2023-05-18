// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Action.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Wednesday, June 1, 2022 12:37:45 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using nGratis.Cop.Olympus.Contract;

public class Action : IAction
{
    private Action()
    {
        this.Kind = ActionKind.Unknown;
        this.Owner = Player.Unknown;
        this.Cost = Engine.Cost.Unknown;
        this.Target = ActionTarget.Unknown;
        this.Parameter = Engine.Parameter.Unknown;
    }

    public static IAction Unknown => UnknownAction.Instance;

    public int Id => this.GetHashCode();

    public string Name => DefinedText.Unsupported;

    public ActionKind Kind { get; private init; }

    public IPlayer Owner { get; set; }

    public ICost Cost { get; private init; }

    public IActionTarget Target { get; private init; }

    public IParameter Parameter { get; set; }

    public static IAction Pass()
    {
        return new Action
        {
            Kind = ActionKind.Passing,
            Cost = Engine.Cost.None,
            Target = ActionTarget.None,
            Parameter = Engine.Parameter.None
        };
    }

    public static IAction PlayLand(ICard card)
    {
        return new Action
        {
            Kind = ActionKind.PlayingLand,
            Cost = card.Cost,
            Target = new ActionTarget
            {
                Cards = new[] { card }
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
            Target = new ActionTarget
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
            Target = new ActionTarget
            {
                Cards = new[] { card }
            },
            Parameter = Engine.Parameter.None
        };
    }
}