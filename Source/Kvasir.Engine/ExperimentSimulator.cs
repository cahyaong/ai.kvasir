// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExperimentSimulator.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Sunday, December 29, 2024 1:38:37 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Immutable;
using System.Linq;
using nGratis.AI.Kvasir.Contract;

public class ExperimentSimulator : ISimulator<ExperimentConfig, ExperimentResult>
{
    private readonly IInfrastructureFactory _infrastructureFactory;

    public ExperimentSimulator(IInfrastructureFactory infrastructureFactory)
    {
        this._infrastructureFactory = infrastructureFactory;
    }

    public ExperimentResult Simulate(ExperimentConfig experimentConfig)
    {
        var gameSummaries = Enumerable
            .Range(0, experimentConfig.GameCount)
            .Select(index => this.SimulateGame(index, experimentConfig.GameConfig))
            .ToImmutableArray();

        return new ExperimentResult
        {
            GameSummaries = gameSummaries
        };
    }

    private GameSummary SimulateGame(int index, GameConfig gameConfig)
    {
        var id = $"GAME-{index:D4}";
        var seed = (int)DateTime.UtcNow.Ticks;

        var gameSimulator = this._infrastructureFactory.CreateGameSimulator(id, seed);
        var gameResult = gameSimulator.Simulate(gameConfig);

        return new GameSummary
        {
            Id = id,
            Seed = seed,
            Tabletop = gameResult.Tabletop,
            WinningPlayer = gameResult.WinningPlayer
        };
    }
}