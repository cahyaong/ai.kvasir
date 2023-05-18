// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Tabletop.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
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