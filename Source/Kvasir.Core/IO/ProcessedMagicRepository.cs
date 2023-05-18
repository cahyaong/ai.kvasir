// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessedMagicRepository.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
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
        await using var cardStream = card
            .SerializeToYaml()
            .AsStream();

        this._storageManager.SaveEntry(
            new DataSpec($"{card.SetCode}_{card.Number:D3}", Mime.Yaml),
            cardStream,
            true);
    }
}