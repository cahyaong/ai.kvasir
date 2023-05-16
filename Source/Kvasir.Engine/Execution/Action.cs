// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Action.cs" company="nGratis">
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