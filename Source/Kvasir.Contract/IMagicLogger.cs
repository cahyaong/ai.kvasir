// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMagicLogger.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, March 23, 2024 2:33:52 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using nGratis.Cop.Olympus.Contract;

public interface IMagicLogger
{
    void Log(ITabletop tabletop);

    void Log(Verbosity verbosity, string message);
}