// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IInfrastructureFactory.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Monday, January 13, 2025 4:09:11 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using nGratis.AI.Kvasir.Contract;

public interface IInfrastructureFactory
{
    ISimulator<GameConfig, GameResult> CreateGameSimulator(string id, int seed);
}