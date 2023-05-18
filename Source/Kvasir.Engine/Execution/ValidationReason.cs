// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValidationReason.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, November 12, 2021 12:09:03 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public class ValidationReason
{
    private ValidationReason()
    {
        this.Id = -42;
        this.Cause = DefinedText.Unknown;
        this.References = Enumerable.Empty<string>();
        this.RuleIds = ImmutableList<string>.Empty;
        this.Message = DefinedText.Unknown;
    }

    public int Id { get; private init; }

    public string Cause { get; private init; }

    public IEnumerable<string> References { get; private init; }

    public IEnumerable<string> RuleIds { get; private init; }

    public string Message { get; private init; }

    public static ValidationReason Create(string message, IEnumerable<string> ruleIds, Creature creature)
    {
        return ValidationReason.Create(message, ruleIds, creature.Permanent);
    }

    public static ValidationReason Create(string message, IEnumerable<string> ruleIds, IPermanent permanent)
    {
        Guard
            .Require(message, nameof(message))
            .Is.Not.Empty();

        return new ValidationReason
        {
            Id = permanent.Id,
            Cause = $"Permanent_{permanent.Card.Kind}",
            References = new[] { permanent.Name },
            RuleIds = ruleIds,
            Message = message
        };
    }

    public static ValidationReason Create(string message, IEnumerable<string> ruleIds, ICost cost)
    {
        Guard
            .Require(message, nameof(message))
            .Is.Not.Empty();

        var references = new List<string>();

        if (cost.Kind == CostKind.PayingMana)
        {
            references.Add(cost
                .Parameter
                .FindValue<IManaCost>(ParameterKey.Amount)
                .PrintDiagnostic());
        }

        return new ValidationReason
        {
            Id = cost.Id,
            Cause = $"Cost_{cost.Kind}",
            References = references,
            RuleIds = ruleIds,
            Message = message
        };
    }

    public static ValidationReason Create(string message, IEnumerable<string> ruleIds, IAction action)
    {
        Guard
            .Require(message, nameof(message))
            .Is.Not.Empty();

        return new ValidationReason
        {
            Id = action.Id,
            Cause = $"Action_{action.Kind}",
            References = action
                .Target.Cards
                .Select(card => card.Name),
            RuleIds = ruleIds,
            Message = message
        };
    }

    public string CreateDetailedMessage()
    {
        return new StringBuilder(this.Message)
            .Append($" Cause: [{this.Cause}].")
            .Append($" References: ({this.References.ToPrettifiedText()}).")
            .ToString();
    }
}