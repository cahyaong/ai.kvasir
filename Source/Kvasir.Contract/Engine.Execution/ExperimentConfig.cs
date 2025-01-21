// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExperimentConfig.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Sunday, December 29, 2024 1:39:47 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

public class ExperimentConfig
{
    public required int GameCount { get; init; }

    public required GameConfig GameConfig { get; init; }
}