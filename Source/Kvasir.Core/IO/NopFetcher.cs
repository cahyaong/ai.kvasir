// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NopFetcher.cs" company="nGratis">
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
// <creation_timestamp>Thursday, April 9, 2020 6:36:02 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using nGratis.AI.Kvasir.Contract;

public class NopFetcher : IMagicFetcher
{
    public ExternalResources AvailableResources => ExternalResources.All;

    public Task<IReadOnlyCollection<UnparsedBlob.CardSet>> FetchCardSetsAsync()
    {
        throw new NotSupportedException("Fetching card sets is not allowed!");
    }

    public Task<IReadOnlyCollection<UnparsedBlob.Card>> FetchCardsAsync(UnparsedBlob.CardSet cardSet)
    {
        throw new NotSupportedException("Fetching cards is not allowed!");
    }

    public Task<IImage> FetchCardImageAsync(UnparsedBlob.Card card)
    {
        throw new NotSupportedException("Fetching card image is not allowed!");
    }

    public Task<IReadOnlyCollection<UnparsedBlob.Rule>> FetchRulesAsync()
    {
        throw new NotSupportedException("Fetching rules is not allowed!");
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}