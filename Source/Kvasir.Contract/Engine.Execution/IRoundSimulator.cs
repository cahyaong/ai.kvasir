// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRoundSimulator.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, May 27, 2023 6:26:06 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

public interface IRoundSimulator
{
    SimulationResult Simulate(SimulationConfig simulationConfig);
}