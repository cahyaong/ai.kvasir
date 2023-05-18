// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMagicEntityFactory.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Monday, 28 January 2019 5:02:42 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using nGratis.AI.Kvasir.Contract;

public interface IMagicEntityFactory
{
    IPlayer CreatePlayer(DefinedBlob.Player definedPlayer);
}