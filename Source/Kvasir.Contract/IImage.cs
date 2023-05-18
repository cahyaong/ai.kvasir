// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IImage.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Sunday, August 16, 2020 1:16:48 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System.IO;

public interface IImage
{
    int Width { get; }

    int Height { get; }

    void LoadData(Stream dataSteam);

    Stream SaveData();
}