// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicRepositoryTests.cs" company="nGratis">
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
// <creation_timestamp>Monday, 5 November 2018 7:58:24 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Test
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using nGratis.AI.Kvasir.Contract;
    using Xunit;

    public class MagicRepositoryTests
    {
        public class GetCardSetsAsyncMethod
        {
            [Fact]
            public async Task WhenGettingEmptyIndex_ShouldPopulateItFromFetcher()
            {
                // Arrange.

                var mockIndexManager = MockBuilder
                    .CreateMock<IIndexManager>()
                    .WithDefaultIndexReader(IndexKind.CardSet)
                    .WithDefaultIndexWriter(IndexKind.CardSet);

                var mockMagicFetcher = MockBuilder
                    .CreateMock<IMagicFetcher>()
                    .WithCardSets(MockBuilder.CreateCardSets(3));

                var magicRepository = new MagicRepository(mockIndexManager.Object, mockMagicFetcher.Object);

                // Act.

                var cardSets = await magicRepository.GetCardSetsAsync();

                // Assert.

                cardSets
                    .Should().NotBeNullOrEmpty()
                    .And.HaveCount(3)
                    .And.Contain(set =>
                        !string.IsNullOrEmpty(set.Code) &&
                        !string.IsNullOrEmpty(set.Name) &&
                        set.ReleasedTimestamp > DateTime.MinValue);

                mockIndexManager.Verify(
                    mock => mock.HasIndex(IndexKind.CardSet),
                    Times.Once);

                mockIndexManager.Verify(
                    mock => mock.CreateIndexReader(IndexKind.CardSet),
                    Times.Never);

                mockIndexManager.Verify(
                    mock => mock.CreateIndexWriter(IndexKind.CardSet),
                    Times.Once);

                mockMagicFetcher.Verify(
                    mock => mock.GetCardSetsAsync(),
                    Times.Once);
            }

            [Fact]
            public async Task WhenGettingNotEmptyIndex_ShouldPopulateItFromIndex()
            {
                // Arrange.

                var mockIndexManager = MockBuilder
                    .CreateMock<IIndexManager>()
                    .WithDefaultIndexWriter(IndexKind.CardSet)
                    .WithExistingCardSets(MockBuilder.CreateCardSets(3));

                var mockMagicFetcher = MockBuilder
                    .CreateMock<IMagicFetcher>()
                    .WithoutCardSets();

                var magicRepository = new MagicRepository(mockIndexManager.Object, mockMagicFetcher.Object);

                // Act.

                var cardSets = await magicRepository.GetCardSetsAsync();

                // Assert.

                cardSets
                    .Should().NotBeNullOrEmpty()
                    .And.HaveCount(3)
                    .And.Contain(set =>
                        !string.IsNullOrEmpty(set.Code) &&
                        !string.IsNullOrEmpty(set.Name) &&
                        set.ReleasedTimestamp > DateTime.MinValue);

                mockIndexManager.Verify(
                    mock => mock.HasIndex(IndexKind.CardSet),
                    Times.Once);

                mockIndexManager.Verify(
                    mock => mock.CreateIndexReader(IndexKind.CardSet),
                    Times.Once);

                mockIndexManager.Verify(
                    mock => mock.CreateIndexWriter(IndexKind.CardSet),
                    Times.Never);

                mockMagicFetcher.Verify(
                    mock => mock.GetCardSetsAsync(),
                    Times.Never);
            }
        }
    }
}