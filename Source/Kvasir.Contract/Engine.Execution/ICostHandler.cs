// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICostHandler.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, April 15, 2023 4:22:55 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

public interface ICostHandler
{
    CostKind CostKind { get; }

    ValidationResult Validate(ITabletop tabletop, ICost cost, ITarget target);

    void Resolve(ITabletop tabletop, ICost cost, ITarget target);
}