// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SerializationExtensionsTests.cs" company="nGratis">
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
// <creation_timestamp>Thursday, July 30, 2020 3:15:30 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Test
{
    using FluentAssertions;
    using FluentAssertions.Execution;
    using nGratis.AI.Kvasir.Contract;
    using Xunit;
    using YamlDotNet.Serialization;

    public class SerializationExtensionsTests
    {
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

                var yaml = deck.SerializeToYaml();

                // Assert.

                yaml
                    .Should().Be(Yaml.Deck.Trim(), "because YAML should contain deck content");
            }
        }

        public class DeserializeFromYamlMethod_Deck
        {
            [Fact]
            public void WhenGettingValidContent_ShouldDeserializeImportantFields()
            {
                // Arrange.

                // Act.

                var deck = Yaml.Deck.DeserializeFromYaml<DefinedBlob.Deck>();

                // Assert.

                deck
                    .Should().NotBeNull();

                using (new AssertionScope())
                {
                    deck
                        .Code
                        .Should().Be("[_MOCK_DECK_CODE_]", "because deck should have code");

                    deck
                        .Name
                        .Should().Be("[_MOCK_DECK_NAME_]", "because deck should have name");

                    deck
                        .Cards
                        .Should().NotBeNull()
                        .And.HaveCount(4, "because deck should contain defined card names");

                    deck["Armageddon", "POR", 5]
                        .Should().Be(1, "because deck should contain [Armageddon]");

                    deck["Assassin's Blade", "POR", 80]
                        .Should().Be(2, "because deck should contain [Assassin's Blade]");

                    deck["Deep-Sea Serpent", "POR", 51]
                        .Should().Be(3, "because deck should contain [Deep-Sea Serpent]");

                    deck["Déjà Vu", "POR", 53]
                        .Should().Be(4, "because deck should contain [Déjà Vu]");
                }
            }
        }

        private static class Yaml
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
    }
}