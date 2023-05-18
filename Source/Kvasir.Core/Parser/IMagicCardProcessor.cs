// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMagicCardProcessor.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, 25 December 2018 11:13:40 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Parser;

using nGratis.AI.Kvasir.Contract;

public interface IMagicCardProcessor
{
    ProcessingResult Process(UnparsedBlob.Card unparsedCard);
}