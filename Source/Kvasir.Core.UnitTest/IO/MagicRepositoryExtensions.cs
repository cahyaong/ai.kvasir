// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicRepositoryExtensions.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, April 7, 2020 5:45:08 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.UnitTest;

using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using nGratis.AI.Kvasir.Contract;
using Xunit;

using MockBuilder = AI.Kvasir.Framework.MockBuilder;

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
                .Should().ThrowAsync<KvasirException>()
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
                .Should().ThrowAsync<KvasirException>()
                .WithMessage(
                    "Found more than 1 card set! " +
                    "Name: [[_MOCK_NAME_]].");
        }
    }
}