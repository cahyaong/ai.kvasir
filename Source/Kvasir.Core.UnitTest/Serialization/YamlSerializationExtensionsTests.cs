// --------------------------------------------------------------------------------------------------------------------
// <copyright file="YamlSerializationExtensionsTests.cs" company="nGratis">
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
// <creation_timestamp>Thursday, July 30, 2020 3:15:30 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.UnitTest;

using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Execution;
using JetBrains.Annotations;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;
using nGratis.Cop.Olympus.Framework;
using Xunit;
using YamlDotNet.Serialization;

public class YamlSerializationExtensionsTests
{
    public class SerializeToYamlMethod_Cost
    {
        [Theory]
        [MemberData(nameof(TestData.SerializingCostTheories), MemberType = typeof(TestData))]
        public void WhenGettingValidCost_ShouldSerializeImportantFields(SerializingCostTheory theory)
        {
            // Arrange.

            // Act.

            var blob = theory
                .DeserializedInstance
                .SerializeToYaml();

            // Assert.

            blob
                .Should().Be(theory.SerializedBlob.Trim(), "because YAML should contain cost content");
        }
    }

    public class SerializeToYamlMethod_Effect
    {
        [Theory]
        [MemberData(nameof(TestData.SerializingEffectTheories), MemberType = typeof(TestData))]
        public void WhenGettingValidCost_ShouldSerializeImportantFields(SerializingEffectTheory theory)
        {
            // Arrange.

            // Act.

            var blob = theory
                .DeserializedInstance
                .SerializeToYaml();

            // Assert.

            blob
                .Should().Be(theory.SerializedBlob.Trim(), "because YAML should contain effect content");
        }
    }

    public class SerializeToYamlMethod_Deck
    {
        [Fact]
        public void WhenGettingValidDeck_ShouldSerializeImportantFields()
        {
            // Arrange.

            var deck = DefinedBlob.Deck.Builder
                .Create()
                .WithCode("[_MOCK_DECK_CODE_]")
                .WithName("[_MOCK_DECK_NAME_]")
                .WithCardAndQuantity("Armageddon", "POR", 5, 1)
                .WithCardAndQuantity("Assassin's Blade", "POR", 80, 2)
                .WithCardAndQuantity("Deep-Sea Serpent", "POR", 51, 3)
                .WithCardAndQuantity("Déjà Vu", "POR", 53, 4)
                .Build();

            // Act.

            var blob = deck.SerializeToYaml();

            // Assert.

            blob
                .Should().Be(Yaml.Deck.Trim(), "because YAML should contain deck content");
        }
    }

    public class DeserializedFromYamlMethod_Cost
    {
        [Theory]
        [MemberData(nameof(TestData.SerializingCostTheories), MemberType = typeof(TestData))]
        public void WhenGettingValidContent_ShouldDeserializeImportantFields(SerializingCostTheory theory)
        {
            // Arrange.

            // Act.

            var instance = theory
                .SerializedBlob
                .DeserializeFromYaml<DefinedBlob.Cost>();

            // Assert.

            instance
                .Must().BeStrictEquivalentTo(theory.DeserializedInstance);
        }
    }

    public class DeserializedFromYamlMethod_Effect
    {
        [Theory]
        [MemberData(nameof(TestData.SerializingEffectTheories), MemberType = typeof(TestData))]
        public void WhenGettingValidContent_ShouldDeserializeImportantFields(SerializingEffectTheory theory)
        {
            // Arrange.

            // Act.

            var instance = theory
                .SerializedBlob
                .DeserializeFromYaml<DefinedBlob.Effect>();

            // Assert.

            instance
                .Must().BeStrictEquivalentTo(theory.DeserializedInstance);
        }
    }

    public class DeserializeFromYamlMethod_Deck
    {
        [Fact]
        public void WhenGettingValidContent_ShouldDeserializeImportantFields()
        {
            // Arrange.

            // Act.

            var instance = Yaml.Deck.DeserializeFromYaml<DefinedBlob.Deck>();

            // Assert.

            instance
                .Should().NotBeNull();

            using (new AssertionScope())
            {
                instance
                    .Code
                    .Should().Be("[_MOCK_DECK_CODE_]", "because deck should have code");

                instance
                    .Name
                    .Should().Be("[_MOCK_DECK_NAME_]", "because deck should have name");

                instance
                    .Entries
                    .Should().NotBeNull()
                    .And.HaveCount(4, "because deck should contain defined card names");

                instance["Armageddon", "POR", 5]
                    .Should().Be(1, "because deck should contain [Armageddon]");

                instance["Assassin's Blade", "POR", 80]
                    .Should().Be(2, "because deck should contain [Assassin's Blade]");

                instance["Deep-Sea Serpent", "POR", 51]
                    .Should().Be(3, "because deck should contain [Deep-Sea Serpent]");

                instance["Déjà Vu", "POR", 53]
                    .Should().Be(4, "because deck should contain [Déjà Vu]");
            }
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    private static class TestData
    {
        public static IEnumerable<object[]> SerializingCostTheories
        {
            get
            {
                yield return SerializingCostTheory
                    .Create()
                    .WithSerializedBlob("kind: Tapping")
                    .WithDeserializedInstance(DefinedBlob.TappingCost.Instance)
                    .WithLabel(10, "Serializing tapping cost")
                    .ToXunitTheory();

                yield return SerializingCostTheory
                    .Create()
                    .WithSerializedBlob(@"
kind: PayingMana
amount:
  colorless: 1
  white: 2
  blue: 3
  black: 4
  red: 5
  green: 6")
                    .WithDeserializedInstance(DefinedBlob.PayingManaCost.Builder
                        .Create()
                        .WithAmount(Mana.Colorless, 1)
                        .WithAmount(Mana.White, 2)
                        .WithAmount(Mana.Blue, 3)
                        .WithAmount(Mana.Black, 4)
                        .WithAmount(Mana.Red, 5)
                        .WithAmount(Mana.Green, 6)
                        .Build())
                    .WithLabel(20, "Serializing paying mana cost with all basic colors")
                    .ToXunitTheory();

                yield return SerializingCostTheory
                    .Create()
                    .WithSerializedBlob(@"
kind: PayingMana
amount:
  white: 1
  green: 2")
                    .WithDeserializedInstance(DefinedBlob.PayingManaCost.Builder
                        .Create()
                        .WithAmount(Mana.White, 1)
                        .WithAmount(Mana.Green, 2)
                        .Build())
                    .WithLabel(21, "Serializing paying mana cost with specific colors")
                    .ToXunitTheory();

                yield return SerializingCostTheory
                    .Create()
                    .WithSerializedBlob(@"
kind: PayingMana
amount: {}")
                    .WithDeserializedInstance(DefinedBlob.PayingManaCost.Free)
                    .WithLabel(22, "Serializing paying without mana cost")
                    .ToXunitTheory();
            }
        }

        public static IEnumerable<object[]> SerializingEffectTheories
        {
            get
            {
                yield return SerializingEffectTheory
                    .Create()
                    .WithSerializedBlob(@"
kind: ProducingMana
amount:
  colorless: 1
  white: 2
  blue: 3
  black: 4
  red: 5
  green: 6")
                    .WithDeserializedInstance(DefinedBlob.ProducingManaEffect.Builder
                        .Create()
                        .WithAmount(Mana.Colorless, 1)
                        .WithAmount(Mana.White, 2)
                        .WithAmount(Mana.Blue, 3)
                        .WithAmount(Mana.Black, 4)
                        .WithAmount(Mana.Red, 5)
                        .WithAmount(Mana.Green, 6)
                        .Build())
                    .WithLabel(10, "Serializing producing mana cost with all basic colors")
                    .ToXunitTheory();

                yield return SerializingEffectTheory
                    .Create()
                    .WithSerializedBlob(@"
kind: ProducingMana
amount:
  white: 1
  green: 2")
                    .WithDeserializedInstance(DefinedBlob.ProducingManaEffect.Builder
                        .Create()
                        .WithAmount(Mana.White, 1)
                        .WithAmount(Mana.Green, 2)
                        .Build())
                    .WithLabel(11, "Serializing producing mana cost with specific colors")
                    .ToXunitTheory();
            }
        }
    }
}

public sealed class SerializingCostTheory : CopTheory
{
    public string SerializedBlob { get; private set; }

    public DefinedBlob.Cost DeserializedInstance { get; private set; }

    public static SerializingCostTheory Create()
    {
        return new SerializingCostTheory();
    }

    public SerializingCostTheory WithSerializedBlob(string serializedBlob)
    {
        Guard
            .Require(serializedBlob, nameof(serializedBlob))
            .Is.Not.Empty();

        this.SerializedBlob = serializedBlob;

        return this;
    }

    public SerializingCostTheory WithDeserializedInstance(DefinedBlob.Cost deserializedInstance)
    {
        this.DeserializedInstance = deserializedInstance;

        return this;
    }
}

public sealed class SerializingEffectTheory : CopTheory
{
    public string SerializedBlob { get; private set; }

    public DefinedBlob.Effect DeserializedInstance { get; private set; }

    public static SerializingEffectTheory Create()
    {
        return new SerializingEffectTheory();
    }

    public SerializingEffectTheory WithSerializedBlob(string serializedBlob)
    {
        Guard
            .Require(serializedBlob, nameof(serializedBlob))
            .Is.Not.Empty();

        this.SerializedBlob = serializedBlob;

        return this;
    }

    public SerializingEffectTheory WithDeserializedInstance(DefinedBlob.Effect deserializedInstance)
    {
        this.DeserializedInstance = deserializedInstance;

        return this;
    }
}

internal static class Yaml
{
    public static readonly string Deck = @"
code: '[_MOCK_DECK_CODE_]'
name: '[_MOCK_DECK_NAME_]'
mainboard:
- 1 Armageddon (POR) 5
- 2 Assassin's Blade (POR) 80
- 3 Deep-Sea Serpent (POR) 51
- 4 Déjà Vu (POR) 53";
}