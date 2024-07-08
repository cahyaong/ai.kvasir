// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExecutionResult.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, June 29, 2024 6:16:57 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public class ExecutionResult : KvasirResult
{
    private ExecutionResult()
    {
        this.WinningPlayer = UnknownPlayer.Instance;
    }

    public static ExecutionResult SuccessfulWithoutWinner { get; } = new()
    {
        WinningPlayer = NonePlayer.Instance
    };

    public bool IsTerminal =>
        this.HasError ||
        this.WinningPlayer.Kind != PlayerKind.None;

    public IPlayer WinningPlayer { get; init; }

    public static ExecutionResult Create(IPlayer winningPlayer)
    {
        return new ExecutionResult
        {
            WinningPlayer = winningPlayer,
            Messages = []
        };
    }

    public new static ExecutionResult Create(IEnumerable<string> messages)
    {
        return new ExecutionResult
        {
            WinningPlayer = NonePlayer.Instance,
            Messages = messages
        };
    }

    public static ExecutionResult Create(IReadOnlyCollection<ExecutionResult> executionResults)
    {
        var winningPlayers = executionResults
            .Select(executionResult => executionResult.WinningPlayer)
            .Where(winningPlayer => winningPlayer != NonePlayer.Instance)
            .Distinct()
            .ToImmutableArray();

        if (winningPlayers.Length > 1)
        {
            throw new KvasirException("Execution results must have at most single distinct winning player!");
        }

        return new ExecutionResult
        {
            WinningPlayer = winningPlayers
                .SingleOrDefault() ?? NonePlayer.Instance,
            Messages = executionResults
                .SelectMany(executionResult => executionResult.Messages)
        };
    }
}