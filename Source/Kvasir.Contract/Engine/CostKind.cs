// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CostKind.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Monday, 14 January 2019 11:46:29 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

public enum CostKind
{
    Unknown = 0,

    DoingNothing,

    PayingMana,
    Tapping
}