// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParsingResult.cs" company="nGratis">
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
// <creation_timestamp>Tuesday, June 30, 2020 6:20:29 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Parser;

using System.Linq;
using Antlr4.Runtime;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public class ParsingResult : ExecutionResult
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