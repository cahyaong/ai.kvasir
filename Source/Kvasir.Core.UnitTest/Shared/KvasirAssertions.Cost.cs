// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KvasirAssertions.Cost.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, July 4, 2020 6:08:54 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.UnitTest;

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;
using EquivalencyOption = FluentAssertions.Equivalency.EquivalencyAssertionOptions<Contract.DefinedBlob.Cost>;

internal class CostAssertion : ReferenceTypeAssertions<DefinedBlob.Cost, CostAssertion>
{
    public CostAssertion(DefinedBlob.Cost cost)
        : base(cost)
    {
        cost
            .Should().NotBeNull();
    }

    protected override string Identifier { get; } = "cost";

    public AndConstraint<CostAssertion> BeStrictEquivalentTo(DefinedBlob.Cost cost)
    {
        Guard
            .Require(cost, nameof(cost))
            .Is.Not.Null();

        this
            .Subject
            .Should().BeEquivalentTo(
                cost,
                option => option
                    .UsingStrictCostComparison());

        return new AndConstraint<CostAssertion>(this);
    }
}

internal static class CostAssertionExtensions
{
    internal static EquivalencyOption UsingStrictCostComparison(this EquivalencyOption equivalencyOption)
    {
        Guard
            .Require(equivalencyOption, nameof(equivalencyOption))
            .Is.Not.Null();

        return equivalencyOption
            .Using<DefinedBlob.PayingManaCost>(context =>
            {
                using (new AssertionScope())
                {
                    Enum
                        .GetValues(typeof(Mana))
                        .Cast<Mana>()
                        .Where(mana => mana != Mana.Unknown)
                        .Where(mana => context.Subject[mana] != context.Expectation[mana])
                        .ForEach(mana => Execute
                            .Assertion
                            .FailWith(
                                $"Expected ability to have paying [{mana}] mana cost, " +
                                $"with amount [{context.Expectation[mana]}], " +
                                $"but found [{context.Subject[mana]}]."));
                }
            })
            .When(info => info.RuntimeType == typeof(DefinedBlob.PayingManaCost));
    }
}