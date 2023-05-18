// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ZoneKind.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, 24 January 2019 9:45:48 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

public enum ZoneKind
{
    Unknown = 0,

    Library,
    Hand,
    Battlefield,
    Graveyard,
    Stack,
    Exile,

    Stub
}