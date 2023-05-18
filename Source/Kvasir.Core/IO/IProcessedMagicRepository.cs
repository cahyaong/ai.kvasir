// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IProcessedMagicRepository.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, October 16, 2020 6:16:52 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core;

using System.Threading.Tasks;
using nGratis.AI.Kvasir.Contract;

public interface IProcessedMagicRepository
{
    Task<DefinedBlob.Card> LoadCardAsync(string setCode, ushort number);

    Task SaveCardAsync(DefinedBlob.Card card);
}