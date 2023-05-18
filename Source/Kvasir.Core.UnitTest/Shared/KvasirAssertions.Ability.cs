// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KvasirAssertions.Ability.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, June 26, 2020 6:40:16 AM UTC</creation_timestamp>
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
using EquivalencyOption = FluentAssertions.Equivalency.EquivalencyAssertionOptions<Contract.DefinedBlob.Ability>;

// NOTE: FluentAssertions could not compare two instances correctly if their type has data exposed via indexer,
// so in this case we need to handle it manually, e.g. for <PayingManaCost> and <ProducingManaEffect>.

internal class AbilityAssertion : ReferenceTypeAssertions<DefinedBlob.Ability, AbilityAssertion>
{
    public AbilityAssertion(DefinedBlob.Ability ability)
        : base(ability)
    {
        ability
            .Should().NotBeNull();
    }

    protected override string Identifier { get; } = "ability";

    public AndConstraint<AbilityAssertion> BeStrictEquivalentTo(DefinedBlob.Ability ability)
    {
        Guard
            .Require(ability, nameof(ability))
            .Is.Not.Null();

        this
            .Subject
            .Should().BeEquivalentTo(
                ability,
                option => option
                    .ComparingByMembers<DefinedBlob.Ability>()
                    .UsingStrictCostComparison()
                    .UsingStrictEffectComparison());

        return new AndConstraint<AbilityAssertion>(this);
    }
}

internal static class AbilityAssertionExtensions
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

    internal static EquivalencyOption UsingStrictEffectComparison(this EquivalencyOption equivalencyOption)
    {
        Guard
            .Require(equivalencyOption, nameof(equivalencyOption))
            .Is.Not.Null();

        return equivalencyOption
            .Using<DefinedBlob.ProducingManaEffect>(context =>
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
                                $"Expected ability to have [{mana}] mana producing effect, " +
                                $"with amount [{context.Expectation[mana]}], " +
                                $"but found [{context.Subject[mana]}]."));
                }
            })
            .When(info => info.RuntimeType == typeof(DefinedBlob.ProducingManaEffect));
    }
}