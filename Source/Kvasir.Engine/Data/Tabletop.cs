// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Tabletop.cs" company="nGratis">
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
// <creation_timestamp>Wednesday, 23 January 2019 10:45:26 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using nGratis.AI.Kvasir.Contract;

public class Tabletop : ITabletop
{
    public Tabletop()
    {
        this.Battlefield = new Zone<IPermanent>
        {
            Kind = ZoneKind.Battlefield,
            Visibility = Visibility.Public
        };

        this.Stack = new Zone<IAction>
        {
            Kind = ZoneKind.Stack,
            Visibility = Visibility.Public
        };

        this.Exile = new Zone<ICard>
        {
            Kind = ZoneKind.Exile,
            Visibility = Visibility.Public
        };

        this.TurnId = -1;
        this.Phase = Phase.Unknown;

        this.ActivePlayer = Player.Unknown;
        this.NonActivePlayer = Player.Unknown;
        this.PrioritizedPlayer = Player.Unknown;

        this.AttackingDecision = Engine.AttackingDecision.Unknown;
        this.BlockingDecision = Engine.BlockingDecision.Unknown;
    }

    public static ITabletop Unknown => UnknownTabletop.Instance;

    public IZone<IPermanent> Battlefield { get; }

    public IZone<IAction> Stack { get; }

    public IZone<ICard> Exile { get; }

    public int TurnId { get; set; }

    public Phase Phase { get; set; }

    public IPlayer ActivePlayer { get; set; }

    public IPlayer NonActivePlayer { get; set; }

    public IPlayer PrioritizedPlayer { get; set; }

    public IAttackingDecision AttackingDecision { get; set; }

    public IBlockingDecision BlockingDecision { get; set; }

    public bool IsFirstTurn => this.TurnId <= 0;

    public bool IsActionPerformed { get; set; }

    public int PlayedLandCount { get; set; }
}