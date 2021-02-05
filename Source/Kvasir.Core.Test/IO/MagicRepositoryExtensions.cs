// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicRepositoryExtensions.cs" company="nGratis">
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
// <creation_timestamp>Tuesday, April 7, 2020 5:45:08 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Test
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using FluentAssertions.Execution;
    using Moq.AI.Kvasir;
    using nGratis.AI.Kvasir.Contract;
    using Xunit;
    using MockBuilder = Moq.AI.Kvasir.MockBuilder;

    public class MagicRepositoryExtensions
    {
        public class GetCardSetAsyncMethod
        {
            [Fact]
            public async Task WhenGettingNameForExistingCardSet_ShouldReturnIt()
            {
                // Arrange.

                var mockRepository = MockBuilder
                    .CreateMock<IUnprocessedMagicRepository>()
                    .WithCardSets("[_MOCK_NAME_01_]", "[_MOCK_NAME_02_]", "[_MOCK_NAME_03_]");

                // Act.

                var cardSet = await mockRepository.Object.GetCardSetAsync("[_MOCK_NAME_02_]");

                // Assert.

                cardSet
                    .Should().NotBeNull();

                using (new AssertionScope())
                {
                    cardSet
                        .Code
                        .Should().Be("[_MOCK_CODE_02_]");

                    cardSet
                        .Name
                        .Should().Be("[_MOCK_NAME_02_]");

                    cardSet
                        .ReleasedTimestamp
                        .Should().Be(DateTime.Parse("2020-01-01"));
                }
            }

            [Fact]
            public void WhenGettingNameForNonExistingCardSet_ShouldThrowKvasirException()
            {
                // Arrange.

                var mockRepository = MockBuilder
                    .CreateMock<IUnprocessedMagicRepository>()
                    .WithCardSets("[_MOCK_NAME_]");

                // Act & Assert.

                mockRepository.Object
                    .Awaiting(self => self.GetCardSetAsync("[_MOCK_ANOTHER_NAME_]"))
                    .Should().Throw<KvasirException>()
                    .WithMessage(
                        "Failed to find card set! " +
                        "Name: [[_MOCK_ANOTHER_NAME_]].");
            }

            [Fact]
            public void WhenGettingNameForMultipleDifferentCardSets_ShouldThrowKvasirException()
            {
                // Arrange.

                var mockRepository = MockBuilder
                    .CreateMock<IUnprocessedMagicRepository>()
                    .WithCardSets("[_MOCK_NAME_]", "[_MOCK_NAME_]");

                // Act & Assert.

                mockRepository.Object
                    .Awaiting(self => self.GetCardSetAsync("[_MOCK_NAME_]"))
                    .Should().Throw<KvasirException>()
                    .WithMessage(
                        "Found more than 1 card set! " +
                        "Name: [[_MOCK_NAME_]].");
            }
        }
    }
}