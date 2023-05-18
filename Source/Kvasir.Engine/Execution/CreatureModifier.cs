// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreatureModifier.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Wednesday, July 7, 2021 5:24:28 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

public enum CreatureModifier
{
    Unknown = 0,

    None = 1,

    CanAttack = 2,
    CanBlock = 3
}