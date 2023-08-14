// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Config.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, February 26, 2021 4:21:38 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System;
using System.IO;
using nGratis.Cop.Olympus.Contract;

public static class Config
{
    public static DataSpec ProcessedContentSpec { get; } = new("Processed_KVASIR", OlympusMime.Cache);

    public static Uri FindDataFolderUri()
    {
        var dataFolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "NGRATIS",
            "ai.kvasir");

        if (!Directory.Exists(dataFolderPath))
        {
            Directory.CreateDirectory(dataFolderPath);
        }

        return new Uri(dataFolderPath);
    }
}