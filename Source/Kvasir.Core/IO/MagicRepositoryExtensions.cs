// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicRepositoryExtensions.cs" company="nGratis">
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