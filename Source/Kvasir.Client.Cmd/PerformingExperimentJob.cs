// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PerformingExperimentJob.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, May 29, 2021 6:14:57 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client.Cmd;

using System.Threading.Tasks;
using nGratis.AI.Kvasir.Contract;

public class PerformingExperimentJob : IJob
{
    private readonly ISimulator<ExperimentConfig, ExperimentResult> _experimentSimulator;

    public PerformingExperimentJob(ISimulator<ExperimentConfig, ExperimentResult> experimentSimulator)
    {
        this._experimentSimulator = experimentSimulator;
    }

    public async Task<JobResult> PerformAsync(JobParameter parameter)
    {
        var definedPlayers = new[]
        {
            new DefinedBlob.Player
            {
                Name = "John Doe",
                Kind = PlayerKind.AI,
                DeckCode = "RED_01"
            },
            new DefinedBlob.Player
            {
                Name = "Jane Doe",
                Kind = PlayerKind.AI,
                DeckCode = "WHITE_01"
            }
        };

        var gameConfig = new GameConfig
        {
            MaxTurnCount = 50,
            ShouldTerminateOnIllegalAction = true,
            DefinedPlayers = definedPlayers
        };

        var experimentConfig = new ExperimentConfig
        {
            GameCount = 100,
            GameConfig = gameConfig
        };

        var experimentResult = this._experimentSimulator.Simulate(experimentConfig);

        return await Task.FromResult(JobResult.Create(experimentResult));
    }
}