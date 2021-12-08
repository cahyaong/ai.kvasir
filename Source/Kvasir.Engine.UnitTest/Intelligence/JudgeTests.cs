// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JudgeTests.cs" company="nGratis">
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

namespace nGratis.AI.Kvasir.Engine.UnitTest
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;
    using FluentAssertions;
    using JetBrains.Annotations;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Olympus.Contract;
    using nGratis.Cop.Olympus.Framework;
    using Xunit;

    public class JudgeTests
    {
        public class FindCreaturesMethod
        {
            [Theory]
            [MemberData(nameof(TestData.FindingCreaturesControlledByPlayerTheories), MemberType = typeof(TestData))]
            public void WhenGettingCreaturesControlledByPlayer_ShouldReturnFilteredCreatures(
                FindingCreaturesTheory theory)
            {
                // Arrange.

                var judge = new Judge(theory.Tabletop);

                // Act.

                var creatures = judge
                    .FindCreatures(theory.Player, theory.QueryModifier)
                    .ToImmutableList();

                // Assert.

                creatures
                    .Should().NotBeNull()
                    .And.HaveCount(theory.ExpectedCreatureNames.Count());

                creatures
                    .Select(creature => creature.Name)
                    .Should().Contain(theory.ExpectedCreatureNames);
            }

            [Theory]
            [MemberData(
                nameof(TestData.FindingCreaturesControlledByOtherPlayerTheories),
                MemberType = typeof(TestData))]
            public void WhenGettingCreaturesControlledByAnotherPlayer_ShouldReturnFilteredCreatures(
                FindingCreaturesTheory theory)
            {
                // Arrange.

                var judge = new Judge(theory.Tabletop);

                // Act.

                var creatures = judge
                    .FindCreatures(theory.Player, theory.QueryModifier)
                    .ToImmutableList();

                // Assert.

                creatures
                    .Should().NotBeNull()
                    .And.HaveCount(theory.ExpectedCreatureNames.Count());

                if (theory.ExpectedCreatureNames.Any())
                {
                    creatures
                        .Select(creature => creature.Name)
                        .Should().Contain(theory.ExpectedCreatureNames);
                }
            }

            [Fact]
            public void WhenHavingNonCreatures_ShouldNotIncludeThemInQuery()
            {
                // Arrange.

                var player = new Player();

                var battlefield = new Zone(ZoneKind.Battlefield, Visibility.Public);
                battlefield.AddCardToTop(new Land("[_MOCK_LAND_]")
                {
                    Owner = player,
                    Controller = player
                });

                var judge = new Judge(new Tabletop
                {
                    Battlefield = battlefield
                });

                // Act.

                var creatures = judge
                    .FindCreatures(player, QueryModifier.None)
                    .ToImmutableList();

                // Assert.

                creatures
                    .Should().NotBeNull()
                    .And.BeEmpty();
            }

            [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
            private static class TestData
            {
                public static IEnumerable<object[]> FindingCreaturesControlledByPlayerTheories
                {
                    get
                    {
                        yield return FindingCreaturesTheory
                            .Create(QueryModifier.None)
                            .Expect(
                                "[_MOCK_CREATURE_001_]",
                                "[_MOCK_CREATURE_002_]",
                                "[_MOCK_CREATURE_003_]",
                                "[_MOCK_CREATURE_004_]")
                            .WithLabel(1, "Finding creatures controlled by player with 'None' modifier")
                            .ToXunitTheory();

                        yield return FindingCreaturesTheory
                            .Create(QueryModifier.CanAttack)
                            .Expect("[_MOCK_CREATURE_001_]")
                            .WithLabel(2, "Finding creatures controlled by player with 'Can Attack' modifier")
                            .ToXunitTheory();

                        yield return FindingCreaturesTheory
                            .Create(QueryModifier.CanBlock)
                            .Expect("[_MOCK_CREATURE_001_]", "[_MOCK_CREATURE_003_]")
                            .WithLabel(2, "Finding creatures controlled by player with 'Can Block' modifier")
                            .ToXunitTheory();
                    }
                }

                public static IEnumerable<object[]> FindingCreaturesControlledByOtherPlayerTheories
                {
                    get
                    {
                        yield return FindingCreaturesTheory
                            .Create(QueryModifier.None)
                            .WithOtherPlayerAsController()
                            .Expect(
                                "[_MOCK_CREATURE_001_]",
                                "[_MOCK_CREATURE_002_]",
                                "[_MOCK_CREATURE_003_]",
                                "[_MOCK_CREATURE_004_]")
                            .WithLabel(1, "Finding creatures controlled by other player with 'None' modifier")
                            .ToXunitTheory();

                        yield return FindingCreaturesTheory
                            .Create(QueryModifier.CanAttack)
                            .WithOtherPlayerAsController()
                            .Expect()
                            .WithLabel(2, "Finding creatures controlled by other player with 'Can Attack' modifier")
                            .ToXunitTheory();

                        yield return FindingCreaturesTheory
                            .Create(QueryModifier.CanBlock)
                            .WithOtherPlayerAsController()
                            .Expect()
                            .WithLabel(2, "Finding creatures controlled by other player with 'Can Block' modifier")
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

                public Player Player { get; private init; }

                public QueryModifier QueryModifier { get; private init; }

                public IEnumerable<string> ExpectedCreatureNames { get; private set; }

                public static FindingCreaturesTheory Create(QueryModifier queryModifier)
                {
                    var battlefield = new Zone(ZoneKind.Battlefield, Visibility.Public);
                    var player = new Player()
                    {
                        Name = "[_MOCK_PLAYER_01_]"
                    };

                    battlefield.LoadCreatureData("Theory_FindingCreatures");

                    battlefield
                        .Cards
                        .ForEach(card =>
                        {
                            card.Owner = player;
                            card.Controller = player;
                        });

                    var tabletop = new Tabletop
                    {
                        Battlefield = battlefield
                    };

                    return new FindingCreaturesTheory
                    {
                        Tabletop = tabletop,
                        Player = player,
                        QueryModifier = queryModifier
                    };
                }

                public FindingCreaturesTheory WithOtherPlayerAsController()
                {
                    var otherPlayer = new Player
                    {
                        Name = "[_MOCK_PLAYER_02_]"
                    };

                    this.Tabletop
                        .Battlefield.Cards
                        .ForEach(card => card.Controller = otherPlayer);

                    return this;
                }

                public FindingCreaturesTheory Expect(params string[] creatureNames)
                {
                    Guard
                        .Require(creatureNames, nameof(creatureNames))
                        .Is.Not.Null();

                    this.ExpectedCreatureNames = creatureNames;

                    return this;
                }
            }
        }
    }
}