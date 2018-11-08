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
    using nGratis.AI.Kvasir.Contract.Magic;
    using Xunit;

    public class MagicRepositoryTests
    {
        public class GetCardAsyncMethod
        {
            [Fact]
            public async Task WhenGettingEmptyIndex_ShouldPopulateItFromFetcher()
            {
                // Arrange.

                var mockFetcher = MockBuilder
                    .CreateMock<IMagicFetcher>()
                    .WithCardSets(MockBuilder.CreateCardSets(3));

                var stubDirectory = StubDirectory
                    .Create();

                var magicRepository = new MagicRepository(stubDirectory, mockFetcher.Object);

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

                stubDirectory
                    .HasValidIndex
                    .Should().BeTrue();

                stubDirectory
                    .ListAll()
                    .Should().NotBeNullOrEmpty();

                mockFetcher.Verify(mock => mock.GetCardSetsAsync(), Times.Once);
            }

            [Fact]
            public async Task WhenGettingNotEmptyIndex_ShouldPopulateItFromIndex()
            {
                // Arrange.

                var mockFetcher = MockBuilder
                    .CreateMock<IMagicFetcher>()
                    .WithoutCardSets();

                var stubDirectory = StubDirectory
                    .Create()
                    .WithCardSets(MockBuilder.CreateCardSets(3));

                var magicRepository = new MagicRepository(stubDirectory, mockFetcher.Object);

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

                mockFetcher.Verify(mock => mock.GetCardSetsAsync(), Times.Never);
            }
        }
    }
}