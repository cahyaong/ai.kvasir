// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPermanent.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, April 28, 2022 4:03:54 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

public interface IPermanent : IDiagnostic
{
    ICard Card { get; }

    IPlayer OwningPlayer { get; set; }

    IPlayer ControllingPlayer { get; set; }

    public bool IsTapped { get; set; }

    void AddPart(params IPart[] parts);

    void RemoveParts();

    bool HasPart<TPart>() where TPart : IPart;

    TPart FindPart<TPart>() where TPart : IPart;
}