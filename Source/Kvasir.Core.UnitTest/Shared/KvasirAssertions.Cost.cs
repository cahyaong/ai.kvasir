// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KvasirAssertions.Cost.cs" company="nGratis">
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
// <creation_timestamp>Saturday, July 4, 2020 6:08:54 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.UnitTest
{
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
        {
            cost
                .Should().NotBeNull();

            this.Subject = cost;
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
}