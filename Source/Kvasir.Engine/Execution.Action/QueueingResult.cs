// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueueingResult.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, April 8, 2023 2:05:14 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Linq;
using nGratis.AI.Kvasir.Contract;

public class QueueingResult : ExecutionResult
{
    private QueueingResult()
    {
    }

    public bool IsNormalActionPerformed { get; private init; }

    public bool IsSpecialActionPerformed { get; private init; }

    public bool IsStackResolved { get; private init; }

    public static QueueingResult CreateWhenStackUnresolved(bool isActionPerformed, ValidationResult validationResult)
    {
        return new QueueingResult
        {
            IsNormalActionPerformed = isActionPerformed,
            IsSpecialActionPerformed = false,
            IsStackResolved = false,
            Messages = validationResult != ValidationResult.Successful
                ? validationResult.Messages
                : Enumerable.Empty<string>()
        };
    }

    public static QueueingResult CreateWhenStackResolved(bool isActionPerformed, ValidationResult validationResult)
    {
        return new QueueingResult
        {
            IsNormalActionPerformed = isActionPerformed,
            IsSpecialActionPerformed = false,
            IsStackResolved = true,
            Messages = validationResult != ValidationResult.Successful
                ? validationResult.Messages
                : Enumerable.Empty<string>()
        };
    }

    public static QueueingResult CreateWhenSpecialActionPerformed(ValidationResult validationResult)
    {
        return new QueueingResult
        {
            IsSpecialActionPerformed = true,
            IsStackResolved = false,
            Messages = validationResult != ValidationResult.Successful
                ? validationResult.Messages
                : Enumerable.Empty<string>()
        };
    }
}