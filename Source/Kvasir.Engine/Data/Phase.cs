// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Phase.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, March 11, 2022 7:19:42 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

public enum Phase
{
    Unknown = 0,

    Setup,

    Beginning,
    PrecombatMain,
    Combat,
    PostcombatMain,
    Ending
}