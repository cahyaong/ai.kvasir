// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAction.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Wednesday, June 1, 2022 12:30:36 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

public interface IAction : IDiagnostic
{
    ActionKind Kind { get; }

    IPlayer OwningPlayer { get; set; }

    ICost Cost { get; }

    ITarget Target { get; }

    IParameter Parameter { get; set; }
}