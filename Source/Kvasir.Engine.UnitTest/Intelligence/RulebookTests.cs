// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RulebookTests.cs" company="nGratis">
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
// <creation_timestamp>Wednesday, July 7, 2021 5:35:31 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine.UnitTest;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Framework;
using Xunit;

public class RulebookTests
{
    public class FindCreaturesMethod
    {
        [Theory]
        [MemberData(nameof(TestData.FindingCreaturesControlledByActivePlayerTheories), MemberType = typeof(TestData))]
        public void WhenGettingCreaturesControlledByActivePlayer_ShouldReturnFilteredCreatures(
            FindingCreaturesTheory theory)
        {
            // Arrange.

            // Act.

            var creatures = theory
                .Tabletop
                .FindCreatures(PlayerModifier.Active, theory.CreatureModifier)
                .ToImmutableList();

            // Assert.

            creatures
                .Should().NotBeNull()
                .And.HaveCount(
                    theory.ExpectedCreatureNames.Count(),
                    "because query should return correct count of filtered creatures");

            creatures
                .Select(creature => creature.Permanent.Name)
                .Should().Contain(theory.ExpectedCreatureNames, "because query should find correct creature");
        }

        [Theory]
        [MemberData(
            nameof(TestData.FindingCreaturesControlledByNonActivePlayerTheories),
            MemberType = typeof(TestData))]
        public void WhenGettingCreaturesControlledByNonActivePlayer_ShouldReturnFilteredCreatures(
            FindingCreaturesTheory theory)
        {
            // Arrange.

            // Act.

            var creatures = theory
                .Tabletop
                .FindCreatures(PlayerModifier.NonActive, theory.CreatureModifier)
                .ToImmutableList();

            // Assert.

            creatures
                .Should().NotBeNull()
                .And.HaveCount(
                    theory.ExpectedCreatureNames.Count(),
                    "because query should return correct count of filtered creatures");

            creatures
                .Select(creature => creature.Permanent.Name)
                .Should().Contain(theory.ExpectedCreatureNames, "because query should find correct creature");
        }

        [Fact]
        public void WhenHavingNonCreatures_ShouldNotIncludeThemInQuery()
        {
            // Arrange.

            var tabletop = new Tabletop
            {
                ActivePlayer = new Player()
            };

            tabletop.Battlefield.AddToTop(new Permanent
            {
                Card = new Card
                {
                    Name = "[_MOCK_LAND_]",
                    Kind = CardKind.Land
                },
                Owner = tabletop.ActivePlayer,
                Controller = tabletop.ActivePlayer
            });

            // Act.

            var creatures = tabletop
                .FindCreatures(PlayerModifier.Active, CreatureModifier.CanAttack)
                .ToImmutableList();

            // Assert.

            creatures
                .Should().NotBeNull()
                .And.BeEmpty("because battlefield doesn't have any creature");
        }

        private static class TestData
        {
            public static IEnumerable<object[]> FindingCreaturesControlledByActivePlayerTheories
            {
                get
                {
                    yield return FindingCreaturesTheory
                        .Create(CreatureModifier.None)
                        .WithActivePlayerAsOwnerAndController()
                        .ExpectAllCreatures()
                        .WithLabel(1, "Finding creatures controlled by active player with 'None' modifier")
                        .ToXunitTheory();

                    yield return FindingCreaturesTheory
                        .Create(CreatureModifier.CanAttack)
                        .WithActivePlayerAsOwnerAndController()
                        .ExpectCreature("[_MOCK_CREATURE_001_]")
                        .WithLabel(2, "Finding creatures controlled by active player with 'Can Attack' modifier")
                        .ToXunitTheory();

                    yield return FindingCreaturesTheory
                        .Create(CreatureModifier.CanBlock)
                        .WithActivePlayerAsOwnerAndController()
                        .ExpectCreature("[_MOCK_CREATURE_001_]", "[_MOCK_CREATURE_003_]")
                        .WithLabel(3, "Finding creatures controlled by active player with 'Can Block' modifier")
                        .ToXunitTheory();
                }
            }

            public static IEnumerable<object[]> FindingCreaturesControlledByNonActivePlayerTheories
            {
                get
                {
                    yield return FindingCreaturesTheory
                        .Create(CreatureModifier.None)
                        .WithNonActivePlayerAsOwnerAndController()
                        .ExpectAllCreatures()
                        .WithLabel(1, "Finding creatures controlled by nonactive player with 'None' modifier")
                        .ToXunitTheory();

                    yield return FindingCreaturesTheory
                        .Create(CreatureModifier.CanAttack)
                        .WithNonActivePlayerAsOwnerAndController()
                        .ExpectCreature("[_MOCK_CREATURE_001_]")
                        .WithLabel(2, "Finding creatures controlled by nonactive player with 'Can Attack' modifier")
                        .ToXunitTheory();

                    yield return FindingCreaturesTheory
                        .Create(CreatureModifier.CanBlock)
                        .WithNonActivePlayerAsOwnerAndController()
                        .ExpectCreature("[_MOCK_CREATURE_001_]", "[_MOCK_CREATURE_003_]")
                        .WithLabel(3, "Finding creatures controlled by nonactive player with 'Can Block' modifier")
                        .ToXunitTheory();
                }
            }
        }

        public sealed class FindingCreaturesTheory : CopTheory
        {
            private FindingCreaturesTheory()
            {
            }

            public Tabletop Tabletop { get; private init; }

            public CreatureModifier CreatureModifier { get; private init; }

            public IEnumerable<string> ExpectedCreatureNames { get; private set; }

            public static FindingCreaturesTheory Create(CreatureModifier creatureModifier)
            {
                var tabletop = new Tabletop
                {
                    ActivePlayer = new Player
                    {
                        Name = "[_MOCK_PLAYER_01_]"
                    },
                    NonActivePlayer = new Player
                    {
                        Name = "[_MOCK_PLAYER_02_]"
                    }
                };

                "Theory_FindingCreatures"
                    .FetchCreatures()
                    .ForEach(creature => tabletop.Battlefield.AddToTop(creature.AsPermanent()));

                return new FindingCreaturesTheory
                {
                    Tabletop = tabletop,
                    CreatureModifier = creatureModifier
                };
            }

            public FindingCreaturesTheory WithActivePlayerAsOwnerAndController()
            {
                this.Tabletop
                    .Battlefield
                    .FindAll()
                    .ForEach(card =>
                    {
                        card.Owner = this.Tabletop.ActivePlayer;
                        card.Controller = this.Tabletop.ActivePlayer;
                    });

                return this;
            }

            public FindingCreaturesTheory WithNonActivePlayerAsOwnerAndController()
            {
                this.Tabletop
                    .Battlefield
                    .FindAll()
                    .ForEach(card =>
                    {
                        card.Owner = this.Tabletop.NonActivePlayer;
                        card.Controller = this.Tabletop.NonActivePlayer;
                    });

                return this;
            }

            public FindingCreaturesTheory ExpectAllCreatures()
            {
                this.ExpectedCreatureNames = this
                    .Tabletop.Battlefield
                    .FindAll()
                    .Select(card => card.Name)
                    .Distinct();

                return this;
            }

            public FindingCreaturesTheory ExpectCreature(params string[] names)
            {
                this.ExpectedCreatureNames = names;

                return this;
            }
        }
    }
}