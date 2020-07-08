// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KvasirAssertions.Ability.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2020 Cahya Ong
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
// <creation_timestamp>Friday, June 26, 2020 6:40:16 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Test
{
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
        {
            ability
                .Should().NotBeNull();

            this.Subject = ability;
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
                        .UsingStrictCostComparison()
                        .UsingStrictEffectComparison());

            return new AndConstraint<AbilityAssertion>(this);
        }
    }

    internal static partial class AbilityAssertionExtensions
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
}