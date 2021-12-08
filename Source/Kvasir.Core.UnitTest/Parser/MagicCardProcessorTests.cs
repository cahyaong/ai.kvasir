// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicCardProcessorTests.cs" company="nGratis">
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
// <creation_timestamp>Wednesday, 26 December 2018 8:53:54 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using FluentAssertions.Execution;
    using JetBrains.Annotations;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Core.Parser;
    using nGratis.Cop.Olympus.Contract;
    using nGratis.Cop.Olympus.Framework;
    using Xunit;

    public class MagicCardProcessorTests
    {
        public class ProcessMethod
        {
            [Fact]
            public void WhenGettingValidCard_ShouldReturnValidProcessingResult()
            {
                // Arrange.

                var unparsedCard = new UnparsedBlob.Card
                {
                    Number = "165",
                    Name = "Elvish Ranger",
                    Type = "Creature - Elf",
                    ManaCost = "{2}{G}",
                    Power = "4",
                    Toughness = "1"
                };

                var cardProcessor = new MagicCardProcessor();

                // Act.

                var processingResult = cardProcessor.Process(unparsedCard);

                // Assert.

                processingResult
                    .Should().NotBeNull();

                processingResult
                    .Messages
                    .Should().NotBeNull()
                    .And.BeEmpty();

                processingResult
                    .IsValid
                    .Should().BeTrue();

                processingResult
                    .GetValue<DefinedBlob.Card>()
                    .Should().NotBeNull();

                processingResult
                    .GetValue<DefinedBlob.Card>()
                    .Cost
                    .Should().NotBe(DefinedBlob.UnknownCost.Instance);
            }

            [Fact]
            public void WhenGettingInvalidCard_ShouldInvalidProcessingResult()
            {
                // Arrange.

                var unparsedCard = new UnparsedBlob.Card();
                var cardProcessor = new MagicCardProcessor();

                // Act.

                var processingResult = cardProcessor.Process(unparsedCard);

                // Assert.

                processingResult
                    .Should().NotBeNull();

                processingResult
                    .Messages
                    .Should().NotBeEmpty();

                processingResult
                    .IsValid
                    .Should().BeFalse();

                processingResult
                    .GetValue<DefinedBlob.Card>()
                    .Should().NotBeNull();

                processingResult
                    .GetValue<DefinedBlob.Card>()
                    .Cost
                    .Should().Be(DefinedBlob.UnknownCost.Instance);
            }

            [Fact]
            public void WhenGettingValidNumber_ShouldSetAsIs()
            {
                // Arrange.

                var unparsedCard = new UnparsedBlob.Card
                {
                    Number = "42"
                };

                var cardProcessor = new MagicCardProcessor();

                // Act.

                var processingResult = cardProcessor.Process(unparsedCard);

                // Assert.

                processingResult
                    .Should().NotBeNull();

                processingResult
                    .GetValue<DefinedBlob.Card>()
                    .Should().NotBeNull();

                processingResult
                    .GetValue<DefinedBlob.Card>()
                    .Number
                    .Should().Be(42);
            }

            [Fact]
            public void WhenGettingInvalidNumber_ShouldAddMessage()
            {
                // Arrange.

                var unparsedCard = new UnparsedBlob.Card
                {
                    Number = "-42"
                };

                var cardProcessor = new MagicCardProcessor();

                // Act.

                var processingResult = cardProcessor.Process(unparsedCard);

                // Assert.

                processingResult
                    .Should().NotBeNull();

                processingResult
                    .Messages
                    .Should().Contain("<Number> Value must be positive.");

                processingResult
                    .GetValue<DefinedBlob.Card>()
                    .Should().NotBeNull();

                processingResult
                    .GetValue<DefinedBlob.Card>()
                    .MultiverseId
                    .Should().Be(0);
            }

            [Fact]
            public void WhenGettingValidMultiverseId_ShouldSetAsIs()
            {
                // Arrange.

                var unparsedCard = new UnparsedBlob.Card
                {
                    MultiverseId = 42
                };

                var cardProcessor = new MagicCardProcessor();

                // Act.

                var processingResult = cardProcessor.Process(unparsedCard);

                // Assert.

                processingResult
                    .Should().NotBeNull();

                processingResult
                    .GetValue<DefinedBlob.Card>()
                    .Should().NotBeNull();

                processingResult
                    .GetValue<DefinedBlob.Card>()
                    .MultiverseId
                    .Should().Be(42);
            }

            [Fact]
            public void WhenGettingInvalidMultiverseId_ShouldAddMessage()
            {
                // Arrange.

                var unparsedCard = new UnparsedBlob.Card
                {
                    MultiverseId = -42
                };

                var cardProcessor = new MagicCardProcessor();

                // Act.

                var processingResult = cardProcessor.Process(unparsedCard);

                // Assert.

                processingResult
                    .Should().NotBeNull();

                processingResult
                    .Messages
                    .Should().Contain("<MultiverseId> Value must be zero or positive.");

                processingResult
                    .GetValue<DefinedBlob.Card>()
                    .Should().NotBeNull();

                processingResult
                    .GetValue<DefinedBlob.Card>()
                    .MultiverseId
                    .Should().Be(0);
            }

            [Fact]
            public void WhenGettingValidName_ShouldSetAsIs()
            {
                // Arrange.

                var unparsedCard = new UnparsedBlob.Card
                {
                    Name = "Llanowar Elves"
                };

                var cardProcessor = new MagicCardProcessor();

                // Act.

                var processingResult = cardProcessor.Process(unparsedCard);

                // Assert.

                processingResult
                    .Should().NotBeNull();

                processingResult
                    .GetValue<DefinedBlob.Card>()
                    .Should().NotBeNull();

                processingResult
                    .GetValue<DefinedBlob.Card>()
                    .Name
                    .Should().Be("Llanowar Elves");
            }

            [Theory]
            [MemberData(nameof(TestData.ParsingValidTypeTheories), MemberType = typeof(TestData))]
            public void WhenGettingValidType_ShouldParseValue(ParsingTypeTheory theory)
            {
                // Arrange.

                var unparsedCard = new UnparsedBlob.Card
                {
                    Type = theory.UnparsedType
                };

                var cardProcessor = new MagicCardProcessor();

                // Act.

                var processingResult = cardProcessor.Process(unparsedCard);

                // Assert.

                processingResult
                    .Should().NotBeNull();

                processingResult
                    .GetValue<DefinedBlob.Card>()
                    .Should().NotBeNull();

                using (new AssertionScope())
                {
                    processingResult
                        .GetValue<DefinedBlob.Card>()
                        .Kind
                        .Should().Be(theory.ExpectedCardKind);

                    processingResult
                        .GetValue<DefinedBlob.Card>()
                        .SuperKind
                        .Should().Be(theory.ExpectedCardSuperKind);

                    processingResult
                        .GetValue<DefinedBlob.Card>()
                        .SubKinds
                        .Should().BeEquivalentTo(theory.ExpectedCardSubKinds);

                    processingResult
                        .GetValue<DefinedBlob.Card>()
                        .IsTribal
                        .Should().BeFalse();
                }
            }

            [Fact]
            public void WhenGettingValidTribalType_ShouldSetIsTribal()
            {
                // Arrange.

                var unparsedCard = new UnparsedBlob.Card
                {
                    Type = "Tribal Creature - Merfolk Wizard"
                };

                var cardProcessor = new MagicCardProcessor();

                // Act.

                var processingResult = cardProcessor.Process(unparsedCard);

                // Assert.

                processingResult
                    .Should().NotBeNull();

                processingResult
                    .GetValue<DefinedBlob.Card>()
                    .Should().NotBeNull();

                using (new AssertionScope())
                {
                    processingResult
                        .GetValue<DefinedBlob.Card>()
                        .IsTribal
                        .Should().BeTrue();

                    processingResult
                        .GetValue<DefinedBlob.Card>()
                        .SuperKind
                        .Should().Be(CardSuperKind.None);

                    processingResult
                        .GetValue<DefinedBlob.Card>()
                        .Kind
                        .Should().Be(CardKind.Creature);

                    processingResult
                        .GetValue<DefinedBlob.Card>()
                        .SubKinds
                        .Should().BeEquivalentTo(CardSubKind.Merfolk, CardSubKind.Wizard);
                }
            }

            [Theory]
            [MemberData(nameof(TestData.ParsingInvalidTypeTheories), MemberType = typeof(TestData))]
            public void WhenGettingInvalidType_ShouldAddMessage(ParsingTypeTheory theory)
            {
                // Arrange.

                var unparsedCard = new UnparsedBlob.Card
                {
                    Type = theory.UnparsedType
                };

                var cardProcessor = new MagicCardProcessor();

                // Act.

                var processingResult = cardProcessor.Process(unparsedCard);

                // Assert.

                processingResult
                    .Should().NotBeNull();

                processingResult
                    .Messages
                    .Should().Contain(theory.ExpectedMessage);
            }

            [Fact]
            public void WhenGettingValidLandType_ShouldSetNoManaCost()
            {
                // Arrange.

                var unparsedCard = new UnparsedBlob.Card
                {
                    Type = "Land",
                    ManaCost = string.Empty
                };

                var cardProcessor = new MagicCardProcessor();

                // Act.

                var processingResult = cardProcessor.Process(unparsedCard);

                // Assert.

                processingResult
                    .Should().NotBeNull();

                processingResult
                    .GetValue<DefinedBlob.Card>()
                    .Cost
                    .Should().Be(DefinedBlob.PayingManaCost.Free);
            }

            [Theory]
            [MemberData(nameof(TestData.ParsingValidManaCostTheories), MemberType = typeof(TestData))]
            public void WhenGettingValidManaCost_ShouldParseValue(ParsingManaCostTheory theory)
            {
                // Arrange.

                var unparsedCard = new UnparsedBlob.Card
                {
                    Type = theory.UnparsedType,
                    ManaCost = theory.UnparsedManaCost
                };

                var cardProcessor = new MagicCardProcessor();

                // Act.

                var processingResult = cardProcessor.Process(unparsedCard);

                // Assert.

                processingResult
                    .Should().NotBeNull();

                processingResult
                    .GetValue<DefinedBlob.Card>()
                    .Should().NotBeNull();

                processingResult
                    .GetValue<DefinedBlob.Card>()
                    .Cost
                    .Must().BeStrictEquivalentTo(theory.ExpectedCost);
            }

            [Theory]
            [MemberData(nameof(TestData.ParsingInvalidManaCostTheories), MemberType = typeof(TestData))]
            public void WhenGettingInvalidManaCost_ShouldAddMessage(ParsingManaCostTheory theory)
            {
                // Arrange.

                var unparsedCard = new UnparsedBlob.Card
                {
                    Type = theory.UnparsedType,
                    ManaCost = theory.UnparsedManaCost
                };

                var cardProcessor = new MagicCardProcessor();

                // Act.

                var processingResult = cardProcessor.Process(unparsedCard);

                // Assert.

                processingResult
                    .Should().NotBeNull();

                processingResult
                    .Messages
                    .Should().Contain(theory.ExpectedMessage);

                processingResult
                    .GetValue<DefinedBlob.Card>()
                    .Should().NotBeNull();

                processingResult
                    .GetValue<DefinedBlob.Card>()
                    .Cost
                    .Should().Be(DefinedBlob.UnknownCost.Instance);
            }

            [Theory]
            [MemberData(nameof(TestData.ParsingValidPowerTheories), MemberType = typeof(TestData))]
            public void WhenGettingValidPower_ShouldParseValue(ParsingPowerTheory theory)
            {
                // Arrange.

                var unparsedCard = new UnparsedBlob.Card
                {
                    Type = theory.UnparsedType,
                    Power = theory.UnparsedPower
                };

                var cardProcessor = new MagicCardProcessor();

                // Act.

                var processingResult = cardProcessor.Process(unparsedCard);

                // Assert.

                processingResult
                    .Should().NotBeNull();

                processingResult
                    .GetValue<DefinedBlob.Card>()
                    .Power
                    .Should().Be(theory.ExpectedPower);
            }

            [Theory]
            [MemberData(nameof(TestData.ParsingInvalidPowerTheories), MemberType = typeof(TestData))]
            public void WhenGettingInvalidPower_ShouldAddMessage(ParsingPowerTheory theory)
            {
                // Arrange.

                var unparsedCard = new UnparsedBlob.Card
                {
                    Type = theory.UnparsedType,
                    Power = theory.UnparsedPower
                };

                var cardProcessor = new MagicCardProcessor();

                // Act.

                var processingResult = cardProcessor.Process(unparsedCard);

                // Assert.

                processingResult
                    .Should().NotBeNull();

                processingResult
                    .Messages
                    .Should().Contain(theory.ExpectedMessage);

                processingResult
                    .GetValue<DefinedBlob.Card>()
                    .Power.Should().Be(0);
            }

            [Theory]
            [MemberData(nameof(TestData.ParsingValidToughnessTheories), MemberType = typeof(TestData))]
            public void WhenGettingValidToughness_ShouldParseValue(ParsingToughnessTheory theory)
            {
                // Arrange.

                var unparsedCard = new UnparsedBlob.Card
                {
                    Type = theory.UnparsedType,
                    Toughness = theory.UnparsedToughness
                };

                var cardProcessor = new MagicCardProcessor();

                // Act.

                var processingResult = cardProcessor.Process(unparsedCard);

                // Assert.

                processingResult
                    .Should().NotBeNull();

                processingResult
                    .GetValue<DefinedBlob.Card>()
                    .Toughness
                    .Should().Be(theory.ExpectedToughness);
            }

            [Theory]
            [MemberData(nameof(TestData.ParsingInvalidToughnessTheories), MemberType = typeof(TestData))]
            public void WhenGettingInvalidToughness_ShouldAddMessage(ParsingToughnessTheory theory)
            {
                // Arrange.

                var unparsedCard = new UnparsedBlob.Card
                {
                    Type = theory.UnparsedType,
                    Toughness = theory.UnparsedToughness
                };

                var cardProcessor = new MagicCardProcessor();

                // Act.

                var processingResult = cardProcessor.Process(unparsedCard);

                // Assert.

                processingResult
                    .Should().NotBeNull();

                using (new AssertionScope())
                {
                    processingResult
                        .Messages
                        .Should().Contain(theory.ExpectedMessage);

                    processingResult
                        .GetValue<DefinedBlob.Card>()
                        .Toughness.Should().Be(0);
                }
            }

            [Fact]
            public void WhenGettingInvalidText_ShouldAddMessage()
            {
                // Arrange.

                var unparsedCard = new UnparsedBlob.Card
                {
                    Text =
                        "[_MOCK_UNPARSED_ABILITY_01_]" + Environment.NewLine +
                        "[_MOCK_UNPARSED_ABILITY_02_]"
                };

                var cardProcessor = new MagicCardProcessor();

                // Act.

                var processingResult = cardProcessor.Process(unparsedCard);

                // Assert.

                processingResult
                    .Should().NotBeNull();

                using (new AssertionScope())
                {
                    processingResult
                        .Messages
                        .Should().NotBeNull()
                        .And.Contain("<Ability> No support for value [[_MOCK_UNPARSED_ABILITY_01_] [_MOCK_UNPARSED_ABILITY_02_]].");

                    processingResult
                        .GetValue<DefinedBlob.Card>().Abilities
                        .Should().NotBeNull()
                        .And.HaveCount(1)
                        .And.AllBeEquivalentTo(DefinedBlob.Ability.NotSupported);
                }
            }

            [UsedImplicitly(ImplicitUseTargetFlags.Members)]
            private static class TestData
            {
                public static IEnumerable<object[]> ParsingValidTypeTheories
                {
                    get
                    {
                        yield return ParsingTypeTheory
                            .Create("Artifact")
                            .ExpectValid(
                                CardKind.Artifact,
                                CardSuperKind.None)
                            .WithLabel(1, "Parsing card type without sub-type")
                            .ToXunitTheory();

                        yield return ParsingTypeTheory
                            .Create("Creature - Elf Warrior")
                            .ExpectValid(
                                CardKind.Creature,
                                CardSuperKind.None,
                                CardSubKind.Elf,
                                CardSubKind.Warrior)
                            .WithLabel(2, "Parsing card type with multiple sub-types")
                            .ToXunitTheory();

                        yield return ParsingTypeTheory
                            .Create("Legendary Creature - Goblin Shaman")
                            .ExpectValid(
                                CardKind.Creature,
                                CardSuperKind.Legendary,
                                CardSubKind.Goblin,
                                CardSubKind.Shaman)
                            .WithLabel(3, "Parsing card type with super-type and multiple sub-types")
                            .ToXunitTheory();

                        yield return ParsingTypeTheory
                            .Create("Creature — Kithkin Soldier")
                            .ExpectValid(
                                CardKind.Creature,
                                CardSuperKind.None,
                                CardSubKind.Kithkin,
                                CardSubKind.Soldier)
                            .WithLabel(4, "Parsing card type with separator from MTGJSON")
                            .ToXunitTheory();
                    }
                }

                public static IEnumerable<object[]> ParsingInvalidTypeTheories
                {
                    get
                    {
                        yield return ParsingTypeTheory
                            .Create(string.Empty)
                            .ExpectInvalid("<Kind> Value must not be <null> or empty.")
                            .WithLabel(1, "Parsing empty card type")
                            .ToXunitTheory();

                        yield return ParsingTypeTheory
                            .Create("Food")
                            .ExpectInvalid("<Kind> No mapping for value [Food].")
                            .WithLabel(2, "Parsing invalid card type")
                            .ToXunitTheory();

                        yield return ParsingTypeTheory
                            .Create("Extraordinary Creature")
                            .ExpectInvalid("<SuperKind> No mapping for value [Extraordinary].")
                            .WithLabel(3, "Parsing invalid card super-type")
                            .ToXunitTheory();

                        yield return ParsingTypeTheory
                            .Create("Creature - Quokka Ranger")
                            .ExpectInvalid("<SubKind> No mapping for value [Quokka], [Ranger].")
                            .WithLabel(4, "Parsing invalid card sub-types")
                            .ToXunitTheory();
                    }
                }

                public static IEnumerable<object[]> ParsingValidManaCostTheories
                {
                    get
                    {
                        yield return ParsingManaCostTheory
                            .Create("Creature", "{0}")
                            .ExpectValid(DefinedBlob.PayingManaCost.Free)
                            .WithLabel(1, "Parsing non-land card with zero amount")
                            .ToXunitTheory();

                        yield return ParsingManaCostTheory
                            .Create("Creature", "{42}")
                            .ExpectValid(DefinedBlob.PayingManaCost.Builder
                                .Create()
                                .WithAmount(Mana.Colorless, 42)
                                .Build())
                            .WithLabel(2, "Parsing non-land card with colorless amount")
                            .ToXunitTheory();

                        yield return ParsingManaCostTheory
                            .Create("Creature", "{G}")
                            .ExpectValid(DefinedBlob.PayingManaCost.Builder
                                .Create()
                                .WithAmount(Mana.Green, 1)
                                .Build())
                            .WithLabel(3, "Parsing non-land card with mono-color amount")
                            .ToXunitTheory();

                        yield return ParsingManaCostTheory
                            .Create("Creature", "{1}{W}{U}{B}{R}{G}")
                            .ExpectValid(DefinedBlob.PayingManaCost.Builder
                                .Create()
                                .WithAmount(Mana.Colorless, 1)
                                .WithAmount(Mana.White, 1)
                                .WithAmount(Mana.Blue, 1)
                                .WithAmount(Mana.Black, 1)
                                .WithAmount(Mana.Red, 1)
                                .WithAmount(Mana.Green, 1)
                                .Build())
                            .WithLabel(4, "Parsing non-land card with colorless and all colors amount")
                            .ToXunitTheory();

                        yield return ParsingManaCostTheory
                            .Create("Land", string.Empty)
                            .ExpectValid(DefinedBlob.PayingManaCost.Free)
                            .WithLabel(5, "Parsing land card with empty amount")
                            .ToXunitTheory();
                    }
                }

                public static IEnumerable<object[]> ParsingInvalidManaCostTheories
                {
                    get
                    {
                        yield return ParsingManaCostTheory
                            .Create("Creature", string.Empty)
                            .ExpectInvalid("<ManaCost> Value must not be <null> or empty.")
                            .WithLabel(1, "Parsing non-land card with empty mana cost")
                            .ToXunitTheory();

                        yield return ParsingManaCostTheory
                            .Create("Creature", "{1}{W}{U}{B}{R}{G}{-}{A}{C}{E}")
                            .ExpectInvalid("<ManaCost> Value [{1}{W}{U}{B}{R}{G}{-}{A}{C}{E}] has invalid symbol.")
                            .WithLabel(2, "Parsing non-land card with invalid mana symbol")
                            .ToXunitTheory();

                        yield return ParsingManaCostTheory
                            .Create("Land", "{42}")
                            .ExpectInvalid("<ManaCost> Non-empty value for type [Land].")
                            .WithLabel(3, "Parsing land card with non-empty mana cost")
                            .ToXunitTheory();
                    }
                }

                public static IEnumerable<object[]> ParsingValidPowerTheories
                {
                    get
                    {
                        yield return ParsingPowerTheory
                            .Create("Creature", "42")
                            .ExpectValid(42)
                            .WithLabel(1, "Parsing creature card with non-zero power")
                            .ToXunitTheory();

                        yield return ParsingPowerTheory
                            .Create("Legendary Creature - Elf Warrior", "42")
                            .ExpectValid(42)
                            .WithLabel(2, "Parsing creature card with super-, sub-types and non-zero power")
                            .ToXunitTheory();

                        yield return ParsingPowerTheory
                            .Create("Artifact", string.Empty)
                            .ExpectValid(0)
                            .WithLabel(3, "Parsing non-creature card with empty power")
                            .ToXunitTheory();
                    }
                }

                public static IEnumerable<object[]> ParsingInvalidPowerTheories
                {
                    get
                    {
                        yield return ParsingPowerTheory
                            .Create("Creature", "X")
                            .ExpectInvalid("<Power> Invalid value [X].")
                            .WithLabel(1, "Parsing creature card with invalid power")
                            .ToXunitTheory();

                        yield return ParsingPowerTheory
                            .Create("Basic Land - Forest", "42")
                            .ExpectInvalid("<Power> Non-empty value for non-creature type [Land].")
                            .WithLabel(2, "Parsing non-creature card with non-empty power")
                            .ToXunitTheory();
                    }
                }

                public static IEnumerable<object[]> ParsingValidToughnessTheories
                {
                    get
                    {
                        yield return ParsingToughnessTheory
                            .Create("Creature", "42")
                            .ExpectValid(42)
                            .WithLabel(1, "Parsing creature card with non-zero toughness")
                            .ToXunitTheory();

                        yield return ParsingToughnessTheory
                            .Create("Legendary Creature - Elf Warrior", "42")
                            .ExpectValid(42)
                            .WithLabel(2, "Parsing creature card with super-, sub-types and non-zero toughness")
                            .ToXunitTheory();

                        yield return ParsingToughnessTheory
                            .Create("Artifact", string.Empty)
                            .ExpectValid(0)
                            .WithLabel(3, "Parsing non-creature card with empty toughness")
                            .ToXunitTheory();
                    }
                }

                public static IEnumerable<object[]> ParsingInvalidToughnessTheories
                {
                    get
                    {
                        yield return ParsingToughnessTheory
                            .Create("Creature", "X")
                            .ExpectInvalid("<Toughness> Invalid value [X].")
                            .WithLabel(1, "Parsing creature card with invalid toughness")
                            .ToXunitTheory();

                        yield return ParsingToughnessTheory
                            .Create("Basic Land - Forest", "42")
                            .ExpectInvalid("<Toughness> Non-empty value for non-creature type [Land].")
                            .WithLabel(2, "Parsing non-creature card with non-empty toughness")
                            .ToXunitTheory();
                    }
                }
            }
        }

        public class ParsingTypeTheory : ParsingTheory
        {
            public string UnparsedType { get; private init; }

            public CardKind ExpectedCardKind { get; private set; }

            public CardSuperKind ExpectedCardSuperKind { get; private set; }

            public IEnumerable<CardSubKind> ExpectedCardSubKinds { get; private set; }

            public static ParsingTypeTheory Create(string unparsedType)
            {
                return new()
                {
                    UnparsedType = unparsedType,
                    ExpectedCardKind = CardKind.Unknown,
                    ExpectedCardSuperKind = CardSuperKind.Unknown,
                    ExpectedCardSubKinds = Enumerable.Empty<CardSubKind>(),
                    ExpectedMessage = string.Empty
                };
            }

            public ParsingTypeTheory ExpectValid(
                CardKind cardKind,
                CardSuperKind cardSuperKind,
                params CardSubKind[] cardSubKinds)
            {
                Guard
                    .Require(cardKind, nameof(cardKind))
                    .Is.Not.EqualTo(CardKind.Unknown);

                Guard
                    .Require(cardSuperKind, nameof(cardSuperKind))
                    .Is.Not.EqualTo(CardSuperKind.Unknown);

                // TODO: Extend <Guard> class to support collection check against specific value(s).

                var hasInvalidSubKind = cardSubKinds
                    .Any(subKind => subKind == CardSubKind.Unknown);

                Guard
                    .Require(hasInvalidSubKind, nameof(hasInvalidSubKind))
                    .Is.False();

                this.ExpectedCardKind = cardKind;
                this.ExpectedCardSuperKind = cardSuperKind;
                this.ExpectedCardSubKinds = cardSubKinds;

                return this;
            }
        }

        public class ParsingManaCostTheory : ParsingTheory
        {
            public string UnparsedType { get; private init; }

            public string UnparsedManaCost { get; private init; }

            public DefinedBlob.Cost ExpectedCost { get; private set; }

            public static ParsingManaCostTheory Create(string unparsedType, string unparsedManaCost)
            {
                return new()
                {
                    UnparsedType = unparsedType,
                    UnparsedManaCost = unparsedManaCost,
                    ExpectedCost = DefinedBlob.UnknownCost.Instance,
                    ExpectedMessage = string.Empty
                };
            }

            public ParsingManaCostTheory ExpectValid(DefinedBlob.PayingManaCost cost)
            {
                this.ExpectedCost = cost;

                return this;
            }
        }

        public class ParsingPowerTheory : ParsingTheory
        {
            public string UnparsedType { get; private init; }

            public string UnparsedPower { get; private init; }

            public ushort ExpectedPower { get; private set; }

            public static ParsingPowerTheory Create(string unparsedType, string unparsedPower)
            {
                return new()
                {
                    UnparsedType = unparsedType,
                    UnparsedPower = unparsedPower,
                    ExpectedPower = 0,
                    ExpectedMessage = string.Empty
                };
            }

            public ParsingPowerTheory ExpectValid(ushort power)
            {
                this.ExpectedPower = power;

                return this;
            }
        }

        public class ParsingToughnessTheory : ParsingTheory
        {
            public string UnparsedType { get; private init; }

            public string UnparsedToughness { get; private init; }

            public ushort ExpectedToughness { get; private set; }

            public static ParsingToughnessTheory Create(string unparsedType, string unparsedToughness)
            {
                return new()
                {
                    UnparsedType = unparsedType,
                    UnparsedToughness = unparsedToughness,
                    ExpectedToughness = 0,
                    ExpectedMessage = string.Empty
                };
            }

            public ParsingToughnessTheory ExpectValid(ushort toughness)
            {
                this.ExpectedToughness = toughness;

                return this;
            }
        }

        public abstract class ParsingTheory : CopTheory
        {
            public string ExpectedMessage { get; protected set; }

            public ParsingTheory ExpectInvalid(string message)
            {
                Guard
                    .Require(message, nameof(message))
                    .Is.Not.Empty();

                this.ExpectedMessage = message;

                return this;
            }
        }
    }
}