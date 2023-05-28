// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActionKind.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Wednesday, June 1, 2022 12:39:49 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

public enum ActionKind
{
    Unknown = 0,

    ActivatingManaAbility,
    Discarding,
    Passing,
    PlayingNonLand,
    PlayingLand,

    PlayingStub
}