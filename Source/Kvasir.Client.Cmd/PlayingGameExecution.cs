// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayingGameExecution.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, May 29, 2021 6:14:57 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client.Cmd;

using System.Threading.Tasks;
using nGratis.AI.Kvasir.Contract;

public class PlayingGameExecution : IExecution
{
    private readonly IRoundSimulator _roundSimulator;

    public PlayingGameExecution(IRoundSimulator roundSimulator)
    {
        this._roundSimulator = roundSimulator;
    }

    public async Task<ExecutionResult> ExecuteAsync(ExecutionParameter parameter)
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

        var simulationConfig = new SimulationConfig
        {
            MaxTurnCount = 10,
            ShouldTerminateOnIllegalAction = true,
            DefinedPlayers = definedPlayers
        };

        var simulationResult = this._roundSimulator.Simulate(simulationConfig);

        return await Task.FromResult(simulationResult);
    }
}