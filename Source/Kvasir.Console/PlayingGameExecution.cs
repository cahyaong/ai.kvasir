// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayingGameExecution.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2021 Cahya Ong
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// </copyright>
// <author>Cahya Ong - cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, May 29, 2021 6:14:57 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Console;

using System.Threading.Tasks;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Engine;
using nGratis.Cop.Olympus.Contract;

public class PlayingGameExecution : IExecution
{
    private readonly IMagicEntityFactory _entityFactory;
    private readonly IRandomGenerator _randomGenerator;
    private readonly ILogger _logger;

    public PlayingGameExecution(IMagicEntityFactory entityFactory, IRandomGenerator randomGenerator, ILogger logger)
    {
        this._entityFactory = entityFactory;
        this._randomGenerator = randomGenerator;
        this._logger = logger;
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

        var simulator = new RoundSimulator(this._entityFactory, this._randomGenerator, this._logger);

        simulator.Simulate(simulationConfig);

        return await Task.FromResult(ExecutionResult.Successful);
    }
}