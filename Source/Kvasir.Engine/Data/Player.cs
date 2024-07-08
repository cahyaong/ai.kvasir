// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Player.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Wednesday, 23 January 2019 11:14:15 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Diagnostics;
using nGratis.AI.Kvasir.Contract;

// TODO: Use builder pattern to make most properties immutable after creation!

[DebuggerDisplay("<Player> {this.Name}")]
public class Player : IPlayer
{
    public Player()
    {
        this.Kind = PlayerKind.Unknown;
        this.Name = DefinedText.Unknown;
        this.Deck = Engine.Deck.Unknown;
        this.Strategy = Engine.Strategy.Unknown;

        this.Library = new Zone<ICard>
        {
            Kind = ZoneKind.Library,
            Visibility = Visibility.Hidden
        };

        this.Hand = new Zone<ICard>
        {
            Kind = ZoneKind.Hand,
            Visibility = Visibility.Hidden
        };

        this.Graveyard = new Zone<ICard>
        {
            Kind = ZoneKind.Graveyard,
            Visibility = Visibility.Public
        };

        this.ManaPool = ManaBlob.Builder
            .Create()
            .Build();

        this.Life = 0;
        this.AttackingDecision = Engine.AttackingDecision.Unknown;
        this.BlockingDecision = Engine.BlockingDecision.Unknown;
    }

    public static IPlayer Unknown => UnknownPlayer.Instance;

    public static IPlayer None => NonePlayer.Instance;

    public PlayerKind Kind { get; init; }

    public string Name { get; init; }

    public IDeck Deck { get; init; }

    public IStrategy Strategy { get; init; }

    public IZone<ICard> Library { get; }

    public IZone<ICard> Hand { get; }

    public IZone<ICard> Graveyard { get; }

    public IManaPool ManaPool { get; }

    public int Life { get; set; }

    public IAttackingDecision AttackingDecision { get; set; }

    public IBlockingDecision BlockingDecision { get; set; }
}