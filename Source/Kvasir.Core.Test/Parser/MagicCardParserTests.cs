// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicCardParserTests.cs" company="nGratis">
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
// <creation_timestamp>Wednesday, 26 December 2018 8:53:54 AM UTC</creation_timestamp>
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
    using nGratis.Cop.Core.Contract;
    using nGratis.Cop.Core.Testing;
    using Xunit;

    public class MagicCardParserTests
    {
        public class ParseMethod
        {
            [Fact]
            public void WhenGettingValidCard_ShouldReturnValidParsingResult()
            {
                // Arrange.

                var rawCard = new RawCard
                {
                    Name = "Llanowar Elves",
                    Type = "Creature - Elf Druid",
                    ManaCost = "{G}",
                    Power = "1",
                    Toughness = "1"
                };

                // Act.

                var parsingResult = MagicCardParser.Instance.Parse(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .Messages
                    .Should().NotBeNull()
                    .And.BeEmpty();

                parsingResult
                    .IsValid
                    .Should().BeTrue();

                parsingResult
                    .GetValue<DefinedBlob.Card>()
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<DefinedBlob.Card>()
                    .Cost
                    .Should().NotBe(DefinedBlob.Cost.Unknown);
            }

            [Fact]
            public void WhenGettingInvalidCard_ShouldInvalidParsingResult()
            {
                // Arrange.

                var rawCard = new RawCard();

                // Act.

                var parsingResult = MagicCardParser.Instance.Parse(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .Messages
                    .Should().NotBeEmpty();

                parsingResult
                    .IsValid
                    .Should().BeFalse();

                parsingResult
                    .GetValue<DefinedBlob.Card>()
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<DefinedBlob.Card>()
                    .Cost
                    .Should().Be(DefinedBlob.Cost.Unknown);
            }

            [Fact]
            public void WhenGettingValidMultiverseId_ShouldSetAsIs()
            {
                // Arrange.

                var rawCard = new RawCard
                {
                    MultiverseId = 42
                };

                // Act.

                var parsingResult = MagicCardParser.Instance.Parse(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<DefinedBlob.Card>()
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<DefinedBlob.Card>()
                    .MultiverseId
                    .Should().Be(42);
            }

            [Fact]
            public void WhenGettingInvalidMultiverseId_ShouldAddMessage()
            {
                // Arrange.

                var rawCard = new RawCard
                {
                    MultiverseId = -42
                };

                // Act.

                var parsingResult = MagicCardParser.Instance.Parse(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .Messages
                    .Should().Contain("<MultiverseId> Value must be zero or positive.");

                parsingResult
                    .GetValue<DefinedBlob.Card>()
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<DefinedBlob.Card>()
                    .MultiverseId
                    .Should().Be(0);
            }

            [Fact]
            public void WhenGettingValidName_ShouldSetAsIs()
            {
                // Arrange.

                var rawCard = new RawCard
                {
                    Name = "Llanowar Elves"
                };

                // Act.

                var parsingResult = MagicCardParser.Instance.Parse(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<DefinedBlob.Card>()
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<DefinedBlob.Card>()
                    .Name
                    .Should().Be("Llanowar Elves");
            }

            [Theory]
            [MemberData(nameof(TestData.ValidTypeTheories), MemberType = typeof(TestData))]
            public void WhenGettingValidType_ShouldParseRawValue(TypeTheory theory)
            {
                // Arrange.

                var rawCard = new RawCard
                {
                    Type = theory.RawType
                };

                // Act.

                var parsingResult = MagicCardParser.Instance.Parse(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<DefinedBlob.Card>()
                    .Should().NotBeNull();

                using (new AssertionScope())
                {
                    parsingResult
                        .GetValue<DefinedBlob.Card>()
                        .Kind
                        .Should().Be(theory.ParsedCardKind);

                    parsingResult
                        .GetValue<DefinedBlob.Card>()
                        .SuperKind
                        .Should().Be(theory.ParsedCardSuperKind);

                    parsingResult
                        .GetValue<DefinedBlob.Card>()
                        .SubKinds
                        .Should().BeEquivalentTo(theory.ParsedCardSubKinds);

                    parsingResult
                        .GetValue<DefinedBlob.Card>()
                        .IsTribal
                        .Should().BeFalse();
                }
            }

            [Fact]
            public void WhenGettingValidTribalType_ShouldSetIsTribal()
            {
                // Arrange.

                var rawCard = new RawCard
                {
                    Type = "Tribal Creature - Merfolk Wizard"
                };

                // Act.

                var parsingResult = MagicCardParser.Instance.Parse(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<DefinedBlob.Card>()
                    .Should().NotBeNull();

                using (new AssertionScope())
                {
                    parsingResult
                        .GetValue<DefinedBlob.Card>()
                        .IsTribal
                        .Should().BeTrue();

                    parsingResult
                        .GetValue<DefinedBlob.Card>()
                        .SuperKind
                        .Should().Be(CardSuperKind.None);

                    parsingResult
                        .GetValue<DefinedBlob.Card>()
                        .Kind
                        .Should().Be(CardKind.Creature);

                    parsingResult
                        .GetValue<DefinedBlob.Card>()
                        .SubKinds
                        .Should().BeEquivalentTo(CardSubKind.Merfolk, CardSubKind.Wizard);
                }
            }

            [Theory]
            [MemberData(nameof(TestData.InvalidTypeTheories), MemberType = typeof(TestData))]
            public void WhenGettingInvalidType_ShouldAddMessage(TypeTheory theory)
            {
                // Arrange.

                var rawCard = new RawCard
                {
                    Type = theory.RawType
                };

                // Act.

                var parsingResult = MagicCardParser.Instance.Parse(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .Messages
                    .Should().Contain(theory.Message);
            }

            [Fact]
            public void WhenGettingValidLandType_ShouldSetNoManaCost()
            {
                // Arrange.

                var rawCard = new RawCard
                {
                    Type = "Land",
                    ManaCost = string.Empty
                };

                // Act.

                var parsingResult = MagicCardParser.Instance.Parse(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<DefinedBlob.Card>()
                    .Cost
                    .Should().Be(DefinedBlob.Cost.Free);
            }

            [Theory]
            [MemberData(nameof(TestData.ValidManaCostTheories), MemberType = typeof(TestData))]
            public void WhenGettingValidManaCost_ShouldParseRawValue(ManaCostTheory theory)
            {
                // Arrange.

                var rawCard = new RawCard
                {
                    Type = theory.RawType,
                    ManaCost = theory.RawManaCost
                };

                // Act.

                var parsingResult = MagicCardParser.Instance.Parse(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<DefinedBlob.Card>()
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<DefinedBlob.Card>()
                    .Cost
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<DefinedBlob.Card>()
                    .Cost.Kind
                    .Should().Be(theory.ParsedCostKind);

                parsingResult
                    .GetValue<DefinedBlob.Card>()
                    .Cost.Amount
                    .Should().Be(theory.ParsedAmount);
            }

            [Theory]
            [MemberData(nameof(TestData.InvalidManaCostTheories), MemberType = typeof(TestData))]
            public void WhenGettingInvalidManaCost_ShouldAddMessage(ManaCostTheory theory)
            {
                // Arrange.

                var rawCard = new RawCard
                {
                    Type = theory.RawType,
                    ManaCost = theory.RawManaCost
                };

                // Act.

                var parsingResult = MagicCardParser.Instance.Parse(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .Messages
                    .Should().Contain(theory.Message);

                parsingResult
                    .GetValue<DefinedBlob.Card>()
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<DefinedBlob.Card>()
                    .Cost
                    .Should().Be(DefinedBlob.Cost.Unknown);
            }

            [Theory]
            [MemberData(nameof(TestData.ValidPowerTheories), MemberType = typeof(TestData))]
            public void WhenGettingValidPower_ShouldParseRawValue(PowerTheory theory)
            {
                // Arrange.

                var rawCard = new RawCard
                {
                    Type = theory.RawType,
                    Power = theory.RawPower
                };

                // Act.

                var parsingResult = MagicCardParser.Instance.Parse(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<DefinedBlob.Card>()
                    .Power
                    .Should().Be(theory.ParsedPower);
            }

            [Theory]
            [MemberData(nameof(TestData.InvalidPowerTheories), MemberType = typeof(TestData))]
            public void WhenGettingInvalidPower_ShouldAddMessage(PowerTheory theory)
            {
                // Arrange.

                var rawCard = new RawCard
                {
                    Type = theory.RawType,
                    Power = theory.RawPower
                };

                // Act.

                var parsingResult = MagicCardParser.Instance.Parse(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .Messages
                    .Should().Contain(theory.Message);

                parsingResult
                    .GetValue<DefinedBlob.Card>()
                    .Power.Should().Be(0);
            }

            [Theory]
            [MemberData(nameof(TestData.ValidToughnessTheories), MemberType = typeof(TestData))]
            public void WhenGettingValidToughness_ShouldParseRawValue(ToughnessTheory theory)
            {
                // Arrange.

                var rawCard = new RawCard
                {
                    Type = theory.RawType,
                    Toughness = theory.RawToughness
                };

                // Act.

                var parsingResult = MagicCardParser.Instance.Parse(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<DefinedBlob.Card>()
                    .Toughness
                    .Should().Be(theory.ParsedToughness);
            }

            [Theory]
            [MemberData(nameof(TestData.InvalidToughnessTheories), MemberType = typeof(TestData))]
            public void WhenGettingInvalidToughness_ShouldAddMessage(ToughnessTheory theory)
            {
                // Arrange.

                var rawCard = new RawCard
                {
                    Type = theory.RawType,
                    Toughness = theory.RawToughness
                };

                // Act.

                var parsingResult = MagicCardParser.Instance.Parse(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .Messages
                    .Should().Contain(theory.Message);

                parsingResult
                    .GetValue<DefinedBlob.Card>()
                    .Toughness.Should().Be(0);
            }

            [Fact]
            public void WhenGettingInvalidText_ShouldSetNotSupportedAbility()
            {
                // Arrange.

                var rawCard = new RawCard
                {
                    Text =
                        "[_MOCK_UNPARSED_ABILITY_01_]" + Environment.NewLine +
                        "[_MOCK_UNPARSED_ABILITY_02_]"
                };

                // Act.

                var parsingResult = MagicCardParser.Instance.Parse(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<DefinedBlob.Card>()
                    .Abilities
                    .Should().AllBeEquivalentTo(DefinedBlob.Ability.NotSupported);
            }

            [UsedImplicitly(ImplicitUseTargetFlags.Members)]
            private static class TestData
            {
                public static IEnumerable<object[]> ValidTypeTheories
                {
                    get
                    {
                        yield return TypeTheory
                            .Create("Artifact")
                            .ExpectValid(
                                CardKind.Artifact,
                                CardSuperKind.None)
                            .WithLabel(1, "Card type without sub-type.")
                            .ToXunitTheory();

                        yield return TypeTheory
                            .Create("Creature - Elf Warrior")
                            .ExpectValid(
                                CardKind.Creature,
                                CardSuperKind.None,
                                CardSubKind.Elf,
                                CardSubKind.Warrior)
                            .WithLabel(2, "Card type with multiple sub-types.")
                            .ToXunitTheory();

                        yield return TypeTheory
                            .Create("Legendary Creature - Goblin Shaman")
                            .ExpectValid(
                                CardKind.Creature,
                                CardSuperKind.Legendary,
                                CardSubKind.Goblin,
                                CardSubKind.Shaman)
                            .WithLabel(3, "Card type with super-type and multiple sub-types.")
                            .ToXunitTheory();

                        yield return TypeTheory
                            .Create("Creature — Kithkin Soldier")
                            .ExpectValid(
                                CardKind.Creature,
                                CardSuperKind.None,
                                CardSubKind.Kithkin,
                                CardSubKind.Soldier)
                            .WithLabel(4, "Card type with separator from MTGJSON.")
                            .ToXunitTheory();
                    }
                }

                public static IEnumerable<object[]> InvalidTypeTheories
                {
                    get
                    {
                        yield return TypeTheory
                            .Create(string.Empty)
                            .ExpectInvalid("<Kind> Value must not be <null> or empty.")
                            .WithLabel(1, "Empty type.")
                            .ToXunitTheory();

                        yield return TypeTheory
                            .Create("Food")
                            .ExpectInvalid("<Kind> No mapping for value [Food].")
                            .WithLabel(2, "Invalid card type.")
                            .ToXunitTheory();

                        yield return TypeTheory
                            .Create("Extraordinary Creature")
                            .ExpectInvalid("<SuperKind> No mapping for value [Extraordinary].")
                            .WithLabel(3, "Invalid card super-type.")
                            .ToXunitTheory();

                        yield return TypeTheory
                            .Create("Creature - Quokka Ranger")
                            .ExpectInvalid("<SubKind> No mapping for value [Quokka], [Ranger].")
                            .WithLabel(4, "Invalid card sub - types.")
                            .ToXunitTheory();
                    }
                }

                public static IEnumerable<object[]> ValidManaCostTheories
                {
                    get
                    {
                        yield return ManaCostTheory
                            .Create("Creature", "{0}")
                            .ExpectValid(CostKind.Mana, "{0}")
                            .WithLabel(1, "Non-land with zero amount.")
                            .ToXunitTheory();

                        yield return ManaCostTheory
                            .Create("Creature", "{42}")
                            .ExpectValid(CostKind.Mana, "{42}")
                            .WithLabel(2, "Non-land with colorless amount.")
                            .ToXunitTheory();

                        yield return ManaCostTheory
                            .Create("Creature", "{G}")
                            .ExpectValid(CostKind.Mana, "{G}")
                            .WithLabel(3, "Non-land with mono-color amount.")
                            .ToXunitTheory();

                        yield return ManaCostTheory
                            .Create("Creature", "{1}{W}{U}{B}{R}{G}")
                            .ExpectValid(CostKind.Mana, "{1}{W}{U}{B}{R}{G}")
                            .WithLabel(4, "Non-land with colorless and all colors amount.")
                            .ToXunitTheory();

                        yield return ManaCostTheory
                            .Create("Land", string.Empty)
                            .ExpectValid(CostKind.Free, string.Empty)
                            .WithLabel(5, "Land with empty amount.")
                            .ToXunitTheory();
                    }
                }

                public static IEnumerable<object[]> InvalidManaCostTheories
                {
                    get
                    {
                        yield return ManaCostTheory
                            .Create("Creature", string.Empty)
                            .ExpectInvalid("<ManaCost> Value must not be <null> or empty.")
                            .WithLabel(1, "Non-land with empty mana cost.")
                            .ToXunitTheory();

                        yield return ManaCostTheory
                            .Create("Creature", "{1}{W}{U}{B}{R}{G}{-}{A}{C}{E}")
                            .ExpectInvalid("<ManaCost> Symbol(s) has no mapping for value [{1}{W}{U}{B}{R}{G}{-}{A}{C}{E}].")
                            .WithLabel(2, "Non-land with invalid mana symbol.")
                            .ToXunitTheory();

                        yield return ManaCostTheory
                            .Create("Land", "{42}")
                            .ExpectInvalid("<ManaCost> Non-empty value for type [Land].")
                            .WithLabel(3, "Land with non-empty mana cost.")
                            .ToXunitTheory();
                    }
                }

                public static IEnumerable<object[]> ValidPowerTheories
                {
                    get
                    {
                        yield return PowerTheory
                            .Create("Creature", "42")
                            .ExpectValid(42)
                            .WithLabel(1, "Creature with non-zero power.")
                            .ToXunitTheory();

                        yield return PowerTheory
                            .Create("Legendary Creature - Elf Warrior", "42")
                            .ExpectValid(42)
                            .WithLabel(2, "Creature with super-, sub-types and non-zero power.")
                            .ToXunitTheory();

                        yield return PowerTheory
                            .Create("Artifact", string.Empty)
                            .ExpectValid(0)
                            .WithLabel(3, "Non-creature with empty power.")
                            .ToXunitTheory();
                    }
                }

                public static IEnumerable<object[]> InvalidPowerTheories
                {
                    get
                    {
                        yield return PowerTheory
                            .Create("Creature", "X")
                            .ExpectInvalid("<Power> Invalid value [X].")
                            .WithLabel(1, "Creature with invalid power.")
                            .ToXunitTheory();

                        yield return PowerTheory
                            .Create("Basic Land - Forest", "42")
                            .ExpectInvalid("<Power> Non-empty value for non-creature type [Land].")
                            .WithLabel(2, "Non-creature with non-empty power.")
                            .ToXunitTheory();
                    }
                }

                public static IEnumerable<object[]> ValidToughnessTheories
                {
                    get
                    {
                        yield return ToughnessTheory
                            .Create("Creature", "42")
                            .ExpectValid(42)
                            .WithLabel(1, "Creature with non-zero toughness.")
                            .ToXunitTheory();

                        yield return ToughnessTheory
                            .Create("Legendary Creature - Elf Warrior", "42")
                            .ExpectValid(42)
                            .WithLabel(2, "Creature with super-, sub-types and non-zero toughness.")
                            .ToXunitTheory();

                        yield return ToughnessTheory
                            .Create("Artifact", string.Empty)
                            .ExpectValid(0)
                            .WithLabel(3, "Non-creature with empty toughness.")
                            .ToXunitTheory();
                    }
                }

                public static IEnumerable<object[]> InvalidToughnessTheories
                {
                    get
                    {
                        yield return ToughnessTheory
                            .Create("Creature", "X")
                            .ExpectInvalid("<Toughness> Invalid value [X].")
                            .WithLabel(1, "Creature with invalid toughness.")
                            .ToXunitTheory();

                        yield return ToughnessTheory
                            .Create("Basic Land - Forest", "42")
                            .ExpectInvalid("<Toughness> Non-empty value for non-creature type [Land].")
                            .WithLabel(2, "Non-creature with non-empty toughness.")
                            .ToXunitTheory();
                    }
                }
            }
        }

        public class TypeTheory : ParsingTheory
        {
            public string RawType { get; private set; }

            public CardKind ParsedCardKind { get; private set; }

            public CardSuperKind ParsedCardSuperKind { get; private set; }

            public IEnumerable<CardSubKind> ParsedCardSubKinds { get; private set; }

            public static TypeTheory Create(string rawType)
            {
                return new TypeTheory
                {
                    RawType = rawType,
                    ParsedCardKind = CardKind.Unknown,
                    ParsedCardSuperKind = CardSuperKind.Unknown,
                    ParsedCardSubKinds = Enumerable.Empty<CardSubKind>(),
                    Message = string.Empty
                };
            }

            public TypeTheory ExpectValid(
                CardKind parsedCardKind,
                CardSuperKind parsedCardSuperKind,
                params CardSubKind[] parsedCardSubKinds)
            {
                Guard
                    .Require(parsedCardKind, nameof(parsedCardKind))
                    .Is.Not.EqualTo(CardKind.Unknown);

                Guard
                    .Require(parsedCardSuperKind, nameof(parsedCardSuperKind))
                    .Is.Not.EqualTo(CardSuperKind.Unknown);

                // TODO: Extend <Guard> class to support collection check against specific value(s).

                var hasInvalidSubKind = parsedCardSubKinds
                    .Any(subKind => subKind == CardSubKind.Unknown);

                Guard
                    .Require(hasInvalidSubKind, nameof(hasInvalidSubKind))
                    .Is.False();

                this.ParsedCardKind = parsedCardKind;
                this.ParsedCardSuperKind = parsedCardSuperKind;
                this.ParsedCardSubKinds = parsedCardSubKinds;

                return this;
            }
        }

        public class ManaCostTheory : ParsingTheory
        {
            public string RawType { get; private set; }

            public string RawManaCost { get; private set; }

            public CostKind ParsedCostKind { get; private set; }

            public string ParsedAmount { get; private set; }

            public static ManaCostTheory Create(string rawType, string rawManaCost)
            {
                return new ManaCostTheory
                {
                    RawType = rawType,
                    RawManaCost = rawManaCost,
                    ParsedCostKind = CostKind.Unknown,
                    ParsedAmount = string.Empty,
                    Message = string.Empty
                };
            }

            public ManaCostTheory ExpectValid(CostKind parsedCostKind, string parsedAmount)
            {
                this.ParsedCostKind = parsedCostKind;
                this.ParsedAmount = parsedAmount;

                return this;
            }
        }

        public class PowerTheory : ParsingTheory
        {
            public string RawType { get; private set; }

            public string RawPower { get; private set; }

            public ushort ParsedPower { get; private set; }

            public static PowerTheory Create(string rawType, string rawPower)
            {
                return new PowerTheory
                {
                    RawType = rawType,
                    RawPower = rawPower,
                    ParsedPower = 0,
                    Message = string.Empty
                };
            }

            public PowerTheory ExpectValid(ushort parsedPower)
            {
                this.ParsedPower = parsedPower;

                return this;
            }
        }

        public class ToughnessTheory : ParsingTheory
        {
            public string RawType { get; private set; }

            public string RawToughness { get; private set; }

            public ushort ParsedToughness { get; private set; }

            public static ToughnessTheory Create(string rawType, string rawToughness)
            {
                return new ToughnessTheory
                {
                    RawType = rawType,
                    RawToughness = rawToughness,
                    ParsedToughness = 0,
                    Message = string.Empty
                };
            }

            public ToughnessTheory ExpectValid(ushort parsedToughness)
            {
                this.ParsedToughness = parsedToughness;

                return this;
            }
        }

        public abstract class ParsingTheory : CopTheory
        {
            public string Message { get; protected set; }

            public ParsingTheory ExpectInvalid(string message)
            {
                Guard
                    .Require(message, nameof(message))
                    .Is.Not.Empty();

                this.Message = message;

                return this;
            }
        }
    }
}