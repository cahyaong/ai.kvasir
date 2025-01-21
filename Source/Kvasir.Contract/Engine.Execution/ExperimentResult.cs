// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExperimentResult.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Sunday, December 29, 2024 1:39:59 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System.Collections.Generic;

public class ExperimentResult : KvasirResult
{
    public required IEnumerable<GameSummary> GameSummaries { get; init; }
}