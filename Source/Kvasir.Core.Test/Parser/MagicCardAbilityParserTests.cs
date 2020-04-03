// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicCardAbilityParserTests.cs" company="nGratis">
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
// <creation_timestamp>Thursday, 17 January 2019 7:49:51 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Test
{
    using System.Collections.Generic;
    using FluentAssertions;
    using JetBrains.Annotations;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Core.Parser;
    using nGratis.Cop.Olympus.Testing;
    using Xunit;

    public class MagicCardAbilityParserTests
    {
        public class ParseMethod
        {
            [Theory]
            [MemberData(nameof(TestData.ProducingManaTheories), MemberType = typeof(TestData))]
            public void WhenGettingAbilityToProduceMana_ShouldParseValue(AbilityTheory theory)
            {
                // Arrange.

                // Act.

                var parsingResult = MagicCardAbilityParser.Parse(theory.UnparsedAbility);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .Messages
                    .Should().BeEmpty();

                parsingResult
                    .GetValue<DefinedBlob.Ability>()
                    .Should().BeEquivalentTo(theory.DefinedAbility);
            }

            [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
            private static class TestData
            {
                public static IEnumerable<object[]> ProducingManaTheories
                {
                    get
                    {
                        yield return AbilityTheory
                            .Create("({T}: Add {W}.)")
                            .ExpectProducingMana("{W}")
                            .WithLabel(1, "Plains main ability.")
                            .ToXunitTheory();

                        yield return AbilityTheory
                            .Create("({T}: Add {U}.)")
                            .ExpectProducingMana("{U}")
                            .WithLabel(2, "Island main ability.")
                            .ToXunitTheory();

                        yield return AbilityTheory
                            .Create("({T}: Add {B}.)")
                            .ExpectProducingMana("{B}")
                            .WithLabel(3, "Swamp main ability.")
                            .ToXunitTheory();

                        yield return AbilityTheory
                            .Create("({T}: Add {R}.)")
                            .ExpectProducingMana("{R}")
                            .WithLabel(4, "Mountain main ability.")
                            .ToXunitTheory();

                        yield return AbilityTheory
                            .Create("({T}: Add {G}.)")
                            .ExpectProducingMana("{G}")
                            .WithLabel(5, "Forest main ability.")
                            .ToXunitTheory();
                    }
                }
            }
        }

        public class AbilityTheory : CopTheory
        {
            public string UnparsedAbility { get; private set; }

            public DefinedBlob.Ability DefinedAbility { get; private set; }

            public static AbilityTheory Create(string unparsedAbility)
            {
                return new AbilityTheory
                {
                    UnparsedAbility = unparsedAbility,
                    DefinedAbility = DefinedBlob.Ability.NotSupported
                };
            }

            public AbilityTheory ExpectProducingMana(string unparsedMana)
            {
                this.DefinedAbility = new DefinedBlob.Ability
                {
                    Kind = AbilityKind.Activated,
                    Costs = new[]
                    {
                        new DefinedBlob.Cost
                        {
                            Kind = CostKind.Tapping,
                            Amount = string.Empty
                        }
                    },
                    Effects = new[]
                    {
                        new DefinedBlob.Effect
                        {
                            Kind = EffectKind.ProducingMana,
                            Amount = unparsedMana
                        }
                    }
                };

                return this;
            }
        }
    }
}