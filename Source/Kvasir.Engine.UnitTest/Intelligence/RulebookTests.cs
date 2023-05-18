// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RulebookTests.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
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