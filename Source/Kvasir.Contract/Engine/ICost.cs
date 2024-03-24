﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICost.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, June 4, 2022 6:30:05 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

public interface ICost : IDiagnostic
{
    CostKind Kind { get; }

    IParameter Parameter { get; }
}