// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KvasirAssertions.Effect.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, November 19, 2020 6:11:15 AM UTC</creation_timestamp>
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
using EquivalencyOption = FluentAssertions.Equivalency.EquivalencyAssertionOptions<Contract.DefinedBlob.Effect>;

internal class EffectAssertion : ReferenceTypeAssertions<DefinedBlob.Effect, EffectAssertion>
{
    public EffectAssertion(DefinedBlob.Effect effect)
        : base(effect)
    {
        effect
            .Should().NotBeNull();
    }

    protected override string Identifier { get; } = "effect";

    public AndConstraint<EffectAssertion> BeStrictEquivalentTo(DefinedBlob.Effect effect)
    {
        Guard
            .Require(effect, nameof(effect))
            .Is.Not.Null();

        this
            .Subject
            .Should().BeEquivalentTo(
                effect,
                option => option
                    .UsingStrictEffectComparison());

        return new AndConstraint<EffectAssertion>(this);
    }
}

internal static class EffectAssertionExtensions
{
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