// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValidationResult.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, November 11, 2021 11:51:24 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public class ValidationResult
{
    private ValidationResult()
    {
        this.Reasons = Enumerable.Empty<ValidationReason>();
    }

    public static ValidationResult Successful { get; } = new();

    public bool HasError => this.Reasons.Any();

    public IEnumerable<ValidationReason> Reasons { get; protected init; }

    public IEnumerable<string> Messages => this
        .Reasons
        .Select(reason => reason.CreateDetailedMessage());

    public static ValidationResult Create(IReadOnlyCollection<ValidationReason> reasons)
    {
        return reasons.Any()
            ? new ValidationResult { Reasons = reasons }
            : ValidationResult.Successful;
    }

    public static ValidationResult Create(params ValidationResult[] results)
    {
        var reasons = results
            .Where(result => result.HasError)
            .SelectMany(result => result.Reasons)
            .ToImmutableArray();

        return ValidationResult.Create(reasons);
    }
}