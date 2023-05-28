// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IJudicialAssistant.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, May 26, 2023 6:04:20 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Collections.Generic;
using nGratis.AI.Kvasir.Contract;

public interface IJudicialAssistant
{
    IEnumerable<Creature> FindCreatures(
        ITabletop tabletop,
        PlayerModifier playerModifier,
        CreatureModifier creatureModifier);

    IEnumerable<IAction> FindLegalActions(ITabletop tabletop, PlayerModifier playerModifier);

    IManaPool CalculatePotentialManaPool(ITabletop tabletop, PlayerModifier playerModifier);
}