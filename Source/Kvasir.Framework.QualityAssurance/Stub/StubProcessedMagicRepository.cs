// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StubProcessedMagicRepository.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
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