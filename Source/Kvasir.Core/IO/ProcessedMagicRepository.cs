// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessedMagicRepository.cs" company="nGratis">
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
// <creation_timestamp>Friday, October 16, 2020 6:17:22 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;
using YamlDotNet.Serialization;

public class ProcessedMagicRepository : IProcessedMagicRepository
{
    private readonly IStorageManager _storageManager;

    public ProcessedMagicRepository(IStorageManager storageManager)
    {
        Guard
            .Require(storageManager, nameof(storageManager))
            .Is.Not.Null();

        this._storageManager = storageManager;
    }

    public async Task<DefinedBlob.Card> LoadCardAsync(string setCode, ushort number)
    {
        Guard
            .Require(setCode, nameof(setCode))
            .Is.Not.Empty();

        Guard
            .Require(number, nameof(number))
            .Is.Positive();

        var foundEntries = this
            ._storageManager
            .FindEntries($"{setCode}_{number:D3}", Mime.Yaml)
            .ToArray();

        if (foundEntries.Length != 1)
        {
            throw new KvasirException(
                @"Expecting to find exactly single defined card!",
                $"Set Code: [{setCode}].",
                $"Number: [{number}].");
        }

        await using var cardStream = this._storageManager.LoadEntry(foundEntries.Single());

        return cardStream
            .ReadText()
            .DeserializeFromYaml<DefinedBlob.Card>();
    }

    public async Task SaveCardAsync(DefinedBlob.Card card)
    {
        Guard
            .Require(card, nameof(card))
            .Is.Not.Null();

        await using var cardStream = card
            .SerializeToYaml()
            .AsStream();

        this._storageManager.SaveEntry(
            new DataSpec($"{card.SetCode}_{card.Number:D3}", Mime.Yaml),
            cardStream,
            true);
    }
}