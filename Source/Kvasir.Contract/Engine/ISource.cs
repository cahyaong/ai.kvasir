// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISource.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, December 29, 2023 7:19:01 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

public interface ISource
{
    IPermanent Permanent { get; set; }

    IAbility Ability { get; set; }
}