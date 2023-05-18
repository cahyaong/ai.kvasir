// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicRepositoryExtensions.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, April 4, 2020 6:26:37 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core;

using System.Linq;
using System.Threading.Tasks;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public static class MagicRepositoryExtensions
{
    public static async Task<UnparsedBlob.CardSet> GetCardSetAsync(
        this IUnprocessedMagicRepository unprocessedRepository,
        string name)
    {
        Guard
            .Require(name, nameof(name))
            .Is.Not.Empty();

        var cardSets = (await unprocessedRepository
            .GetCardSetsAsync())
            .Where(cardSet => cardSet.Name == name)
            .ToArray();

        return cardSets.Length switch
        {
            <= 0 => throw new KvasirException(
                "Found no card set!",
                ("Name", name)),

            > 1 => throw new KvasirException(
                "Found more than 1 card set",
                ("Name", name)),

            _ => cardSets.Single()
        };
    }
}