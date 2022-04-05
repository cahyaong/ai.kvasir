// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUnprocessedMagicRepository.cs" company="nGratis">
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
// <creation_timestamp>Thursday, 25 October 2018 10:48:23 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using nGratis.AI.Kvasir.Contract;

// TODO: Consider implementing paging for <Card> after allowing native sort and filter capabilities?

public interface IUnprocessedMagicRepository
{
    event EventHandler CardSetIndexed;

    event EventHandler CardIndexed;

    Task<int> GetCardSetCountAsync();

    Task<int> GetCardCountAsync();

    Task<int> GetRuleCountAsync();

    Task<IReadOnlyCollection<UnparsedBlob.CardSet>> GetCardSetsAsync();

    Task<IReadOnlyCollection<UnparsedBlob.CardSet>> GetCardSetsAsync(int pagingIndex, int itemCount);

    Task<IReadOnlyCollection<UnparsedBlob.Card>> GetCardsAsync(UnparsedBlob.CardSet cardSet);

    Task<IImage> GetCardImageAsync(UnparsedBlob.Card card);

    Task<IReadOnlyCollection<UnparsedBlob.Rule>> GetRulesAsync();

    Task<IReadOnlyCollection<UnparsedBlob.Rule>> GetRulesAsync(int pagingIndex, int itemCount);
}