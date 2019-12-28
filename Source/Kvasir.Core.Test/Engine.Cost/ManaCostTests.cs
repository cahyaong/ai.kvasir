// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManaCostTests.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2018 Cahya Ong
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
// <creation_timestamp>Thursday, 24 January 2019 12:37:28 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using FluentAssertions.Execution;
    using JetBrains.Annotations;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Core.Testing;
    using Xunit;

    public class ManaCostTests
    {
        public class ParseMethod
        {
            [Theory]
            [MemberData(nameof(TestData.ValidValueTheories), MemberType = typeof(TestData))]
            public void WhenGettingValidValue_ShouldCreateInstance(ManaCostTheory theory)
            {
                // Arrange.

                // Act.

                var manaCost = ManaCost.Parse(theory.UnparsedValue);

                // Assert.

                manaCost
                    .Should().NotBeNull();

                using (new AssertionScope())
                {
                    theory
                        .ParsedAmountLookup
                        .ForEach(kvp => manaCost[kvp.Key]
                            .Should().Be(kvp.Value));

                    Enum
                        .GetValues(typeof(Mana))
                        .Cast<Mana>()
                        .Where(mana => mana != Mana.Unknown)
                        .Where(mana => !theory.ParsedAmountLookup.ContainsKey(mana))
                        .ForEach(mana => manaCost[mana]
                            .Should().Be(0));
                }
            }

            [Fact]
            public void WhenGettingInvalidValue_ShouldThrowArgumentException()
            {
                // Arrange.

                var unparsedValue = "{1}{W}{U}{B}{R}{G}{-}{A}{C}{E}";

                // Act.

                var action = new Action(() =>
                {
                    var _ = ManaCost.Parse(unparsedValue);
                });

                // Assert.

                action
                    .Should().Throw<ArgumentException>()
                    .WithMessage($"Value [{unparsedValue}] should contain colorless and/or color mana symbol.");
            }

            [UsedImplicitly(ImplicitUseTargetFlags.Members)]
            private static class TestData
            {
                public static IEnumerable<object[]> ValidValueTheories
                {
                    get
                    {
                        yield return ManaCostTheory
                            .Create("{0}")
                            .ExpectValid(new Dictionary<Mana, ushort>())
                            .WithLabel(1, "Zero amount.")
                            .ToXunitTheory();

                        yield return ManaCostTheory
                            .Create("{42}")
                            .ExpectValid(new Dictionary<Mana, ushort>
                            {
                                [Mana.Colorless] = 42
                            })
                            .WithLabel(2, "Colorless amount.")
                            .ToXunitTheory();

                        yield return ManaCostTheory
                            .Create("{G}")
                            .ExpectValid(new Dictionary<Mana, ushort>
                            {
                                [Mana.Green] = 1
                            })
                            .WithLabel(3, "Mono-color amount.")
                            .ToXunitTheory();

                        yield return ManaCostTheory
                            .Create("{1}{W}{W}{U}{U}{U}{B}{B}{B}{B}{R}{R}{R}{R}{R}{G}{G}{G}{G}{G}{G}")
                            .ExpectValid(new Dictionary<Mana, ushort>
                            {
                                [Mana.Colorless] = 1,
                                [Mana.White] = 2,
                                [Mana.Blue] = 3,
                                [Mana.Black] = 4,
                                [Mana.Red] = 5,
                                [Mana.Green] = 6
                            })
                            .WithLabel(4, "Colorless and all colors amount.")
                            .ToXunitTheory();
                    }
                }
            }
        }

        public class ManaCostTheory : CopTheory
        {
            public string UnparsedValue { get; private set; }

            public IReadOnlyDictionary<Mana, ushort> ParsedAmountLookup { get; private set; }

            public static ManaCostTheory Create(string unparsedValue)
            {
                return new ManaCostTheory
                {
                    UnparsedValue = unparsedValue,
                    ParsedAmountLookup = new Dictionary<Mana, ushort>()
                };
            }

            public ManaCostTheory ExpectValid(IReadOnlyDictionary<Mana, ushort> parsedAmountLookup)
            {
                this.ParsedAmountLookup = parsedAmountLookup;

                return this;
            }
        }
    }
}