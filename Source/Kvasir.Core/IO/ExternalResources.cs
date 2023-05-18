// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExternalResources.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Sunday, 16 December 2018 11:26:02 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core;

using System;

[Flags]
public enum ExternalResources
{
    None = 0,
    CardSet = 1 << 1,
    Card = 1 << 2,
    CardImage = 1 << 3,
    Rule = 1 << 4,

    All = CardSet | Card | CardImage | Rule
}