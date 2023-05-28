// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayingNonLandHandler.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, May 27, 2023 4:32:35 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Linq;
using nGratis.AI.Kvasir.Contract;

public class PlayingNonLandHandler : BaseActionHandler
{
    public override ActionKind ActionKind => ActionKind.PlayingNonLand;

    protected override void ResolveCore(ITabletop tabletop, IAction action)
    {
        action.Target.Player.Hand.MoveToZone(
            action.Target.Cards.Single(),
            tabletop.Battlefield,
            card => card.AsPermanent(action.Target.Player));
    }
}