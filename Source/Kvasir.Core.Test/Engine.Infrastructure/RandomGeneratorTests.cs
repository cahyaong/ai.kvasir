// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RandomGeneratorTests.cs" company="nGratis">
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
// <creation_timestamp>Tuesday, 5 February 2019 8:51:27 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Test
{
    using System.Linq;
    using FluentAssertions;
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
                    .GenerateShufflingIndexes(60)?
                    .ToArray();

                // Assert.

                shufflingIndexes
                    .Should().NotBeNull()
                    .And.HaveCount(60)
                    .And.BeEquivalentTo(Enumerable.Range(0, 60), "indexes should contain unique value")
                    .And.NotBeAscendingInOrder("indexes should be shuffled")
                    .And.NotBeDescendingInOrder("indexes should be shuffled");
            }
        }
    }
}