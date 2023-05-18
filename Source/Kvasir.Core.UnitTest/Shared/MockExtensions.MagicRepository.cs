// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MockExtensions.UnprocessedMagicRepository.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, April 7, 2020 5:49:29 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace Moq.AI.Kvasir;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Core;
using nGratis.Cop.Olympus.Contract;

internal static partial class MockExtensions
{
    public static Mock<IUnprocessedMagicRepository> WithCardSets(
        this Mock<IUnprocessedMagicRepository> mockRepository,
        params string[] names)
    {
        Guard
            .Require(mockRepository, nameof(mockRepository))
            .Is.Not.Null();

        Guard
            .Require(names, nameof(names))
            .Is.Not.Null()
            .Is.Not.Empty();

        var cardSets = names
            .Select((name, index) => new UnparsedBlob.CardSet
            {
                Code = $"[_MOCK_CODE_{(index + 1):D2}_]",
                Name = name,
                ReleasedTimestamp = DateTime.Parse("2020-01-01")
            })
            .ToArray();

        mockRepository
            .Setup(mock => mock.GetCardSetsAsync())
            .Returns(Task.FromResult((IReadOnlyCollection<UnparsedBlob.CardSet>)cardSets));

        return mockRepository;
    }
}