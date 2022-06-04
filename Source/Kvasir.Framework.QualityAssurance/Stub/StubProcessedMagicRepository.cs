// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StubProcessedMagicRepository.cs" company="nGratis">
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
// <creation_timestamp>Thursday, June 2, 2022 5:47:33 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Framework;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading.Tasks;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Core;

public class StubProcessedMagicRepository : IProcessedMagicRepository
{
    private static readonly string SupportedSetCode = "POR";

    private readonly IReadOnlyDictionary<ushort, DefinedBlob.Card> _cardByNumberLookup;

    public StubProcessedMagicRepository()
    {
        this._cardByNumberLookup = $"Processed_{StubProcessedMagicRepository.SupportedSetCode}"
            .FetchProcessedCards()
            .ToImmutableDictionary(card => card.Number);
    }

    public async Task<DefinedBlob.Card> LoadCardAsync(string setCode, ushort number)
    {
        if (setCode != StubProcessedMagicRepository.SupportedSetCode)
        {
            throw new KvasirTestingException(
                "Set code other than 'POR' is not supported!",
                ("Set Code", setCode));
        }

        if (!this._cardByNumberLookup.TryGetValue(number, out var card))
        {
            throw new KvasirTestingException(
                "There is no card with given number!",
                ("Number", number));
        }

        return await Task.FromResult(card);
    }

    public async Task SaveCardAsync(DefinedBlob.Card card)
    {
        await Task.CompletedTask;
    }
}