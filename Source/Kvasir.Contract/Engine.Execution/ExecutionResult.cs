// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExecutionResult.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, July 23, 2020 5:44:12 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public class ExecutionResult
{
    protected ExecutionResult()
    {
        this.Messages = Enumerable.Empty<string>();
    }

    protected ExecutionResult(IEnumerable<string> messages)
    {
        this.Messages = messages;
    }

    public static ExecutionResult Successful { get; } = new();

    public bool HasError =>
        this.Messages.Any() ||
        this.HasErrorCore();

    public IEnumerable<string> Messages { get; protected init; }

    public static ExecutionResult Create(IEnumerable<string> messages)
    {
        var validMessages = messages
            .Where(message => !string.IsNullOrEmpty(message))
            .ToImmutableArray();

        return validMessages.Any()
            ? new ExecutionResult(validMessages)
            : ExecutionResult.Successful;
    }

    public static ExecutionResult Create(IEnumerable<ExecutionResult> executionResults)
    {
        var validMessages = executionResults
            .Where(result => result.HasError)
            .SelectMany(result => result.Messages)
            .ToImmutableArray();

        return validMessages.Any()
            ? new ExecutionResult(validMessages)
            : ExecutionResult.Successful;
    }

    protected virtual bool HasErrorCore() => false;
}