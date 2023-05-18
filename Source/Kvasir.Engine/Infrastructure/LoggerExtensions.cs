// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoggerExtensions.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, March 26, 2022 6:51:05 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using nGratis.Cop.Olympus.Contract;

public static class LoggerExtensions
{
    public static void LogDiagnostic(this ILogger logger, ITabletop tabletop)
    {
        logger.LogDebug(
            "Processing phase...",
            ("ID", $"{tabletop.TurnId:D4}-{tabletop.Phase}"),
            ("Active Player", tabletop.ActivePlayer.Name));
    }
}