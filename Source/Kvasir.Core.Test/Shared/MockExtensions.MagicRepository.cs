// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MockExtensions.UnprocessedMagicRepository.cs" company="nGratis">
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
// <creation_timestamp>Tuesday, April 7, 2020 5:49:29 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace Moq.AI.Kvasir
{
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
}