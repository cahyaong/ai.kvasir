﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicCardParserTests.cs" company="nGratis">
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
// <creation_timestamp>Thursday, 17 January 2019 7:49:51 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Test
{
    using System.Collections.Generic;
    using FluentAssertions;
    using JetBrains.Annotations;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Core.Parser;
    using nGratis.Cop.Olympus.Contract;
    using nGratis.Cop.Olympus.Framework;
    using Xunit;

    public class MagicCardParserTests
    {
        public class ParseAbilityMethod
        {
            [Theory]
            [MemberData(nameof(TestData.ParsingManaAbilityTheories), MemberType = typeof(TestData))]
            public void WhenGettingAbilityToProduceMana_ShouldParseValue(ParsingAbilityTheory theory)
            {
                // Arrange.

                // Act.

                var parsingResult = MagicCardParser.ParseAbility(theory.UnparsedAbility);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .HasError
                    .Should().BeFalse();

                parsingResult
                    .Value
                    .Must().BeStrictEquivalentTo(theory.ParsedAbility);
            }

            [Fact]
            public void WhenGettingInvalidAbility_ShouldIncludeMessage()
            {
                // Arrange.

                var unparsedAbility = "[_MOCK_UNPARSED_ABILITY_]";

                // Act.

                var parsingResult = MagicCardParser.ParseAbility(unparsedAbility);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .HasError
                    .Should().BeTrue();

                parsingResult
                    .Messages
                    .Should().HaveCount(1)
                    .And.Contain("<Root> Ability [[_MOCK_UNPARSED_ABILITY_]] parsing could not continue after processing '[' at [0:0]!");
            }

            [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
            private static class TestData
            {
                public static IEnumerable<object[]> ParsingManaAbilityTheories
                {
                    get
                    {
                        yield return ParsingAbilityTheory
                            .Create("({T}: Add {W}.)")
                            .ExpectProducingMana(Mana.White)
                            .WithLabel(1, "Parsing mana ability from Plains")
                            .ToXunitTheory();

                        yield return ParsingAbilityTheory
                            .Create("({T}: Add {U}.)")
                            .ExpectProducingMana(Mana.Blue)
                            .WithLabel(2, "Parsing mana ability from Island")
                            .ToXunitTheory();

                        yield return ParsingAbilityTheory
                            .Create("({T}: Add {B}.)")
                            .ExpectProducingMana(Mana.Black)
                            .WithLabel(3, "Parsing mana ability from Swamp")
                            .ToXunitTheory();

                        yield return ParsingAbilityTheory
                            .Create("({T}: Add {R}.)")
                            .ExpectProducingMana(Mana.Red)
                            .WithLabel(4, "Parsing mana ability from Mountain")
                            .ToXunitTheory();

                        yield return ParsingAbilityTheory
                            .Create("({T}: Add {G}.)")
                            .ExpectProducingMana(Mana.Green)
                            .WithLabel(5, "Parsing mana ability from Forest")
                            .ToXunitTheory();
                    }
                }
            }

            public sealed class ParsingAbilityTheory : CopTheory
            {
                public string UnparsedAbility { get; private init; }

                public DefinedBlob.Ability ParsedAbility { get; private set; }

                public static ParsingAbilityTheory Create(string unparsedAbility)
                {
                    Guard
                        .Require(unparsedAbility, nameof(unparsedAbility))
                        .Is.Not.Empty();

                    return new ParsingAbilityTheory
                    {
                        UnparsedAbility = unparsedAbility,
                        ParsedAbility = DefinedBlob.Ability.NotSupported
                    };
                }

                public ParsingAbilityTheory ExpectProducingMana(Mana mana)
                {
                    this.ParsedAbility = new DefinedBlob.Ability
                    {
                        Kind = AbilityKind.Activated,
                        Costs = new DefinedBlob.Cost[]
                        {
                            DefinedBlob.TappingCost.Instance
                        },
                        Effects = new DefinedBlob.Effect[]
                        {
                            DefinedBlob.ProducingManaEffect.Builder
                                .Create()
                                .WithAmount(mana, 1)
                                .Build()
                        }
                    };

                    return this;
                }
            }
        }

        public class ParseCostMethod
        {
            [Theory]
            [MemberData(nameof(TestData.ParsingManaCostTheories), MemberType = typeof(TestData))]
            public void WhenGettingCostToPayMana_ShouldParseValue(ParsingCostTheory theory)
            {
                // Arrange.

                // Act.

                var parsingResult = MagicCardParser.ParseCost(theory.UnparsedCost);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .HasError
                    .Should().BeFalse();

                parsingResult
                    .Value
                    .Must().BeStrictEquivalentTo(theory.ParsedCost);
            }

            [Fact]
            public void WhenGettingInvalidCost_ShouldIncludeMessage()
            {
                // Arrange.

                var unparsedCost = "[_MOCK_COST_]";

                // Act.

                var parsingResult = MagicCardParser.ParseCost(unparsedCost);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .HasError
                    .Should().BeTrue();

                parsingResult
                    .Messages
                    .Should().Contain("<Root> Cost [[_MOCK_COST_]] parsing could not continue after processing '[' at [0:0]!");
            }

            [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
            private static class TestData
            {
                public static IEnumerable<object[]> ParsingManaCostTheories
                {
                    get
                    {
                        yield return ParsingCostTheory
                            .Create("{0}")
                            .Expect(DefinedBlob.PayingManaCost.Free)
                            .WithLabel(1, "Parsing zero mana cost")
                            .ToXunitTheory();

                        yield return ParsingCostTheory
                            .Create("{42}")
                            .Expect(DefinedBlob.PayingManaCost.Builder
                                .Create()
                                .WithAmount(Mana.Colorless, 42)
                                .Build())
                            .WithLabel(2, "Parsing colorless mana cost")
                            .ToXunitTheory();

                        yield return ParsingCostTheory
                            .Create("{G}")
                            .Expect(DefinedBlob.PayingManaCost.Builder
                                .Create()
                                .WithAmount(Mana.Green, 1)
                                .Build())
                            .WithLabel(3, "Parsing mono-color mana cost")
                            .ToXunitTheory();

                        yield return ParsingCostTheory
                            .Create("{1}{W}{W}{U}{U}{U}{B}{B}{B}{B}{R}{R}{R}{R}{R}{G}{G}{G}{G}{G}{G}")
                            .Expect(DefinedBlob.PayingManaCost.Builder
                                .Create()
                                .WithAmount(Mana.Colorless, 1)
                                .WithAmount(Mana.White, 2)
                                .WithAmount(Mana.Blue, 3)
                                .WithAmount(Mana.Black, 4)
                                .WithAmount(Mana.Red, 5)
                                .WithAmount(Mana.Green, 6)
                                .Build())
                            .WithLabel(4, "Parsing colorless and all colors mana cost")
                            .ToXunitTheory();
                    }
                }
            }

            public sealed class ParsingCostTheory : CopTheory
            {
                public string UnparsedCost { get; private init; }

                public DefinedBlob.Cost ParsedCost { get; private set; }

                public static ParsingCostTheory Create(string unparsedCost)
                {
                    Guard
                        .Require(unparsedCost, nameof(unparsedCost))
                        .Is.Not.Empty();

                    return new ParsingCostTheory
                    {
                        UnparsedCost = unparsedCost,
                        ParsedCost = DefinedBlob.UnknownCost.Instance
                    };
                }

                public ParsingCostTheory Expect(DefinedBlob.Cost parsedCost)
                {
                    Guard
                        .Require(parsedCost, nameof(parsedCost))
                        .Is.Not.Null();

                    this.ParsedCost = parsedCost;

                    return this;
                }
            }
        }
    }
}