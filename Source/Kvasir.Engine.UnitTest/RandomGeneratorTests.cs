// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RandomGeneratorTests.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, 5 February 2019 8:51:27 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine.UnitTest;

using System.Linq;
using FluentAssertions;
using nGratis.AI.Kvasir.Engine;
using Xunit;

public class RandomGeneratorTests
{
    public class GenerateShufflingIndexesMethod
    {
        [Fact]
        public void WhenGettingValueCount_ShouldGenerateRandomSequence()
        {
            // Arrange.

            var randomGenerator = new RandomGenerator(42);

            // Act.

            var shufflingIndexes = randomGenerator
                .GenerateShufflingIndexes(60)
                .ToArray();

            // Assert.

            shufflingIndexes
                .Should().NotBeNull()
                .And.HaveCount(60)
                .And.BeEquivalentTo(Enumerable.Range(0, 60), "indexes should contain unique value")
                .And.NotBeInAscendingOrder("indexes should be shuffled")
                .And.NotBeInDescendingOrder("indexes should be shuffled");
        }
    }
}