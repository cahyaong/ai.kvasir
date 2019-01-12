// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicParserTests.cs" company="nGratis">
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

    public class MagicParserTests
    {
        public class ParseRawCardMethod
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

                var parsingResult = MagicParser.Instance.ParseRawCard(rawCard);

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
                    .GetValue<CardInfo>()
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<CardInfo>()
                    .ManaCost
                    .Should().NotBeNull();
            }

            [Fact]
            public void WhenGettingInvalidCard_ShouldInvalidParsingResult()
            {
                // Arrange.

                var rawCard = new RawCard();

                // Act.

                var parsingResult = MagicParser.Instance.ParseRawCard(rawCard);

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
                    .GetValue<CardInfo>()
                    .Should().NotBeNull();
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

                var parsingResult = MagicParser.Instance.ParseRawCard(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<CardInfo>()
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<CardInfo>()
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

                var parsingResult = MagicParser.Instance.ParseRawCard(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .Messages
                    .Should().Contain("<MultiverseId> Value must be zero or positive.");

                parsingResult
                    .GetValue<CardInfo>()
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<CardInfo>()
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

                var parsingResult = MagicParser.Instance.ParseRawCard(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<CardInfo>()
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<CardInfo>()
                    .Name
                    .Should().Be("Llanowar Elves");
            }

            [Theory]
            [MemberData(nameof(TestData.ValidCardKindTheories), MemberType = typeof(TestData))]
            public void WhenGettingValidType_ShouldParseRawValue(CardKindTheory theory)
            {
                // Arrange.

                var rawCard = new RawCard
                {
                    Type = theory.RawType
                };

                // Act.

                var parsingResult = MagicParser.Instance.ParseRawCard(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<CardInfo>()
                    .Should().NotBeNull();

                using (new AssertionScope())
                {
                    parsingResult
                        .GetValue<CardInfo>()
                        .Kind
                        .Should().Be(theory.ParsedKind);

                    parsingResult
                        .GetValue<CardInfo>()
                        .SuperKind
                        .Should().Be(theory.ParsedSuperKind);

                    parsingResult
                        .GetValue<CardInfo>()
                        .SubKinds
                        .Should().BeEquivalentTo(theory.ParsedSubKinds);

                    parsingResult
                        .GetValue<CardInfo>()
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

                var parsingResult = MagicParser.Instance.ParseRawCard(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<CardInfo>()
                    .Should().NotBeNull();

                using (new AssertionScope())
                {
                    parsingResult
                        .GetValue<CardInfo>()
                        .IsTribal
                        .Should().BeTrue();

                    parsingResult
                        .GetValue<CardInfo>()
                        .SuperKind
                        .Should().Be(CardSuperKind.None);

                    parsingResult
                        .GetValue<CardInfo>()
                        .Kind
                        .Should().Be(CardKind.Creature);

                    parsingResult
                        .GetValue<CardInfo>()
                        .SubKinds
                        .Should().BeEquivalentTo(CardSubKind.Merfolk, CardSubKind.Wizard);
                }
            }

            [Theory]
            [MemberData(nameof(TestData.InvalidCardKindTheories), MemberType = typeof(TestData))]
            public void WhenGettingInvalidType_ShouldAddMessage(CardKindTheory theory)
            {
                // Arrange.

                var rawCard = new RawCard
                {
                    Type = theory.RawType
                };

                // Act.

                var parsingResult = MagicParser.Instance.ParseRawCard(rawCard);

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

                var parsingResult = MagicParser.Instance.ParseRawCard(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<CardInfo>()
                    .ManaCost
                    .Should().Be(ManaCost.Free);
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

                var parsingResult = MagicParser.Instance.ParseRawCard(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<CardInfo>()
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<CardInfo>()
                    .ManaCost
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<CardInfo>()
                    .ManaCost.ConvertedAmount
                    .Should().Be(theory.ParsedConvertedAmount);

                using (new AssertionScope())
                {
                    theory
                        .ParsedAmountLookup
                        .ForEach(kvp => parsingResult
                            .GetValue<CardInfo>()
                            .ManaCost[kvp.Key]
                            .Should().Be(kvp.Value));

                    Enum
                        .GetValues(typeof(Mana))
                        .Cast<Mana>()
                        .Where(mana => mana != Mana.Unknown)
                        .Where(mana => !theory.ParsedAmountLookup.ContainsKey(mana))
                        .ForEach(mana => parsingResult
                            .GetValue<CardInfo>()
                            .ManaCost[mana]
                            .Should().Be(0));
                }
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

                var parsingResult = MagicParser.Instance.ParseRawCard(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .Messages
                    .Should().Contain(theory.Message);

                parsingResult
                    .GetValue<CardInfo>()
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<CardInfo>()
                    .ManaCost
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<CardInfo>()
                    .ManaCost.ConvertedAmount
                    .Should().Be(0);

                using (new AssertionScope())
                {
                    Enum
                        .GetValues(typeof(Mana))
                        .Cast<Mana>()
                        .Where(mana => mana != Mana.Unknown)
                        .ForEach(mana => parsingResult
                            .GetValue<CardInfo>()
                            .ManaCost[mana]
                            .Should().Be(0));
                }
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

                var parsingResult = MagicParser.Instance.ParseRawCard(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<CardInfo>()
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

                var parsingResult = MagicParser.Instance.ParseRawCard(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .Messages
                    .Should().Contain(theory.Message);

                parsingResult
                    .GetValue<CardInfo>()
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

                var parsingResult = MagicParser.Instance.ParseRawCard(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<CardInfo>()
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

                var parsingResult = MagicParser.Instance.ParseRawCard(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .Messages
                    .Should().Contain(theory.Message);

                parsingResult
                    .GetValue<CardInfo>()
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

                var parsingResult = MagicParser.Instance.ParseRawCard(rawCard);

                // Assert.

                parsingResult
                    .Should().NotBeNull();

                parsingResult
                    .GetValue<CardInfo>()
                    .Abilities
                    .Should().BeEquivalentTo(Ability.NotSupported, Ability.NotSupported);
            }

            [UsedImplicitly(ImplicitUseTargetFlags.Members)]
            private static class TestData
            {
                public static IEnumerable<object[]> ValidCardKindTheories
                {
                    get
                    {
                        yield return CardKindTheory
                            .Create("Artifact")
                            .ExpectValid(
                                CardKind.Artifact,
                                CardSuperKind.None)
                            .WithLabel("CASE 01 -> Card type without sub-type.")
                            .ToXunitTheory();

                        yield return CardKindTheory
                            .Create("Creature - Elf Warrior")
                            .ExpectValid(
                                CardKind.Creature,
                                CardSuperKind.None,
                                CardSubKind.Elf,
                                CardSubKind.Warrior)
                            .WithLabel("CASE 02 -> Card type with multiple sub-types.")
                            .ToXunitTheory();

                        yield return CardKindTheory
                            .Create("Legendary Creature - Goblin Shaman")
                            .ExpectValid(
                                CardKind.Creature,
                                CardSuperKind.Legendary,
                                CardSubKind.Goblin,
                                CardSubKind.Shaman)
                            .WithLabel("CASE 03 -> Card type with super-type and multiple sub-types.")
                            .ToXunitTheory();

                        yield return CardKindTheory
                            .Create("Creature — Kithkin Soldier")
                            .ExpectValid(
                                CardKind.Creature,
                                CardSuperKind.None,
                                CardSubKind.Kithkin,
                                CardSubKind.Soldier)
                            .WithLabel("CASE 04 -> Card type with separator from MTGJSON.")
                            .ToXunitTheory();
                    }
                }

                public static IEnumerable<object[]> InvalidCardKindTheories
                {
                    get
                    {
                        yield return CardKindTheory
                            .Create(string.Empty)
                            .ExpectInvalid("<Kind> Value must not be <null> or empty.")
                            .WithLabel("CASE 01 -> Empty type.")
                            .ToXunitTheory();

                        yield return CardKindTheory
                            .Create("Food")
                            .ExpectInvalid("<Kind> No mapping for value [Food].")
                            .WithLabel("CASE 02 -> Invalid card type.")
                            .ToXunitTheory();

                        yield return CardKindTheory
                            .Create("Extraordinary Creature")
                            .ExpectInvalid("<SuperKind> No mapping for value [Extraordinary].")
                            .WithLabel("CASE 03 -> Invalid card super-type.")
                            .ToXunitTheory();

                        yield return CardKindTheory
                            .Create("Creature - Quokka Ranger")
                            .ExpectInvalid("<SubKind> No mapping for value [Quokka], [Ranger].")
                            .WithLabel("CASE 04 -> Invalid card sub-types.")
                            .ToXunitTheory();
                    }
                }

                public static IEnumerable<object[]> ValidManaCostTheories
                {
                    get
                    {
                        yield return ManaCostTheory
                            .Create("Creature", "{0}")
                            .ExpectValid(new Dictionary<Mana, ushort>())
                            .WithLabel("CASE 01 -> Non-land with zero amount.")
                            .ToXunitTheory();

                        yield return ManaCostTheory
                            .Create("Creature", "{42}")
                            .ExpectValid(new Dictionary<Mana, ushort>
                            {
                                [Mana.Colorless] = 42
                            })
                            .WithLabel("CASE 02 -> Non-land with colorless amount.")
                            .ToXunitTheory();

                        yield return ManaCostTheory
                            .Create("Creature", "{G}")
                            .ExpectValid(new Dictionary<Mana, ushort>
                            {
                                [Mana.Green] = 1
                            })
                            .WithLabel("CASE 03 -> Non-land with mono-color amount.")
                            .ToXunitTheory();

                        yield return ManaCostTheory
                            .Create("Creature", "{1}{W}{W}{U}{U}{U}{B}{B}{B}{B}{R}{R}{R}{R}{R}{G}{G}{G}{G}{G}{G}")
                            .ExpectValid(new Dictionary<Mana, ushort>
                            {
                                [Mana.Colorless] = 1,
                                [Mana.White] = 2,
                                [Mana.Blue] = 3,
                                [Mana.Black] = 4,
                                [Mana.Red] = 5,
                                [Mana.Green] = 6
                            })
                            .WithLabel("CASE 04 -> Non-land with colorless and all colors amount.")
                            .ToXunitTheory();

                        yield return ManaCostTheory
                            .Create("Land", string.Empty)
                            .ExpectValid(new Dictionary<Mana, ushort>())
                            .WithLabel("CASE 05 -> Land with empty amount.")
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
                            .WithLabel("CASE 01 -> Non-land with empty mana cost.")
                            .ToXunitTheory();

                        yield return ManaCostTheory
                            .Create("Creature", "{1}{W}{U}{B}{R}{G}{-}{A}{C}{E}")
                            .ExpectInvalid("<ManaCost> Symbol(s) has no mapping for value [{1}{W}{U}{B}{R}{G}{-}{A}{C}{E}].")
                            .WithLabel("CASE 02 -> Non-land with invalid mana symbol.")
                            .ToXunitTheory();

                        yield return ManaCostTheory
                            .Create("Land", "{42}")
                            .ExpectInvalid("<ManaCost> Non-empty value for type [Land].")
                            .WithLabel("CASE 03 -> Land with non-empty mana cost.")
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
                            .WithLabel("CASE 01 -> Creature with non-zero power.")
                            .ToXunitTheory();

                        yield return PowerTheory
                            .Create("Legendary Creature - Elf Warrior", "42")
                            .ExpectValid(42)
                            .WithLabel("CASE 02 -> Creature with super-, sub-types and non-zero power.")
                            .ToXunitTheory();

                        yield return PowerTheory
                            .Create("Artifact", string.Empty)
                            .ExpectValid(0)
                            .WithLabel("CASE 03 -> Non-creature with empty power.")
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
                            .WithLabel("CASE 01 -> Creature with invalid power.")
                            .ToXunitTheory();

                        yield return PowerTheory
                            .Create("Basic Land - Forest", "42")
                            .ExpectInvalid("<Power> Non-empty value for non-creature type [Land].")
                            .WithLabel("CASE 02 -> Non-creature with non-empty power.")
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
                            .WithLabel("CASE 01 -> Creature with non-zero toughness.")
                            .ToXunitTheory();

                        yield return ToughnessTheory
                            .Create("Legendary Creature - Elf Warrior", "42")
                            .ExpectValid(42)
                            .WithLabel("CASE 02 -> Creature with super-, sub-types and non-zero toughness.")
                            .ToXunitTheory();

                        yield return ToughnessTheory
                            .Create("Artifact", string.Empty)
                            .ExpectValid(0)
                            .WithLabel("CASE 03 -> Non-creature with empty toughness.")
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
                            .WithLabel("CASE 01 -> Creature with invalid toughness.")
                            .ToXunitTheory();

                        yield return ToughnessTheory
                            .Create("Basic Land - Forest", "42")
                            .ExpectInvalid("<Toughness> Non-empty value for non-creature type [Land].")
                            .WithLabel("CASE 02 -> Non-creature with non-empty toughness.")
                            .ToXunitTheory();
                    }
                }
            }
        }

        public class CardKindTheory : BaseParsingTheory
        {
            public string RawType { get; private set; }

            public CardKind ParsedKind { get; private set; }

            public CardSuperKind ParsedSuperKind { get; private set; }

            public IEnumerable<CardSubKind> ParsedSubKinds { get; private set; }

            public static CardKindTheory Create(string rawType)
            {
                return new CardKindTheory
                {
                    RawType = rawType,
                    ParsedKind = CardKind.Unknown,
                    ParsedSuperKind = CardSuperKind.Unknown,
                    ParsedSubKinds = Enumerable.Empty<CardSubKind>(),
                    Message = string.Empty
                };
            }

            public CardKindTheory ExpectValid(
                CardKind parsedKind,
                CardSuperKind parsedSuperKind,
                params CardSubKind[] parsedSubKinds)
            {
                Guard
                    .Require(parsedKind, nameof(parsedKind))
                    .Is.Not.EqualTo(CardKind.Unknown);

                Guard
                    .Require(parsedSuperKind, nameof(parsedSuperKind))
                    .Is.Not.EqualTo(CardSuperKind.Unknown);

                // TODO: Extend <Guard> class to support collection check against specific value(s).

                var hasInvalidSubKind = parsedSubKinds
                    .Any(subKind => subKind == CardSubKind.Unknown);

                Guard
                    .Require(hasInvalidSubKind, nameof(hasInvalidSubKind))
                    .Is.False();

                this.ParsedKind = parsedKind;
                this.ParsedSuperKind = parsedSuperKind;
                this.ParsedSubKinds = parsedSubKinds;

                return this;
            }
        }

        public class ManaCostTheory : BaseParsingTheory
        {
            public string RawType { get; private set; }

            public string RawManaCost { get; private set; }

            public uint ParsedConvertedAmount { get; private set; }

            public IReadOnlyDictionary<Mana, ushort> ParsedAmountLookup { get; private set; }

            public static ManaCostTheory Create(string rawType, string rawManaCost)
            {
                return new ManaCostTheory
                {
                    RawType = rawType,
                    RawManaCost = rawManaCost,
                    ParsedConvertedAmount = 0,
                    ParsedAmountLookup = new Dictionary<Mana, ushort>(),
                    Message = string.Empty
                };
            }

            public ManaCostTheory ExpectValid(IReadOnlyDictionary<Mana, ushort> parsedAmountLookup)
            {
                this.ParsedConvertedAmount = (uint)parsedAmountLookup.Values.Sum(amount => amount);
                this.ParsedAmountLookup = parsedAmountLookup;

                return this;
            }
        }

        public class PowerTheory : BaseParsingTheory
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

        public class ToughnessTheory : BaseParsingTheory
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

        public abstract class BaseParsingTheory : CopTheory
        {
            public string Message { get; protected set; }

            public BaseParsingTheory ExpectInvalid(string message)
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