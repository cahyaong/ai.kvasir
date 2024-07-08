// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParsingResult.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, June 30, 2020 6:20:29 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Parser;

using System.Linq;
using Antlr4.Runtime;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public class ParsingResult : KvasirResult
{
    protected ParsingResult(params string[] messages)
        : base(messages)
    {
    }
}

public sealed class ParsingResult<TValue> : ParsingResult
    where TValue : class
{
    private ParsingResult(params string[] messages)
        : base(messages)
    {
    }

    public TValue? Value { get; private init; }

    internal static ParsingResult<TValue> CreateSuccessful(TValue value)
    {
        return new ParsingResult<TValue>
        {
            Value = value
        };
    }

    internal static ParsingResult<TValue> CreateFailure(string message)
    {
        Guard
            .Require(message, nameof(message))
            .Is.Not.Empty();

        return new ParsingResult<TValue>($"<Root> {message}")
        {
            Value = default
        };
    }

    internal static ParsingResult<TValue> CreateFailure<TContext>(params string[] messages)
        where TContext : ParserRuleContext
    {
        Guard
            .Require(messages, nameof(messages))
            .Is.Not.Empty();

        var contextName = typeof(TContext)
            .Name
            .Replace("Context", string.Empty);

        messages = messages
            .Where(message => !string.IsNullOrEmpty(message))
            .Select(message => $"<{contextName}> {message}")
            .ToArray();

        if (!messages.Any())
        {
            throw new KvasirException("Invalid parsing result must contain at least 1 message!");
        }

        return new ParsingResult<TValue>(messages)
        {
            Value = default
        };
    }

    internal static ParsingResult<TValue> CreateFailure(params ParsingResult[] parsingResults)
    {
        var messages = parsingResults
            .SelectMany(result => result.Messages)
            .Distinct()
            .ToArray();

        if (!messages.Any())
        {
            throw new KvasirException("Invalid parsing result must contain at least 1 message!");
        }

        return new ParsingResult<TValue>(messages)
        {
            Value = default
        };
    }
}