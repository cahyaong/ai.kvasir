// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Image.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Sunday, August 16, 2020 2:06:30 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core;

using System;
using System.IO;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;
using SixLabors.ImageSharp.Formats.Png;

public static class Image
{
    public static IImage Empty => EmptyImage.Instance;
}

public class WritableImage : IImage
{
    private SixLabors.ImageSharp.Image? _image;

    public int Width =>
        this._image?.Width ??
        0;

    public int Height =>
        this._image?.Height ??
        0;

    public void LoadData(Stream dataSteam)
    {
        Guard
            .Require(dataSteam, nameof(dataSteam))
            .Is.Readable();

        // TODO: Handle a case when input data contains transparency.

        dataSteam.Position = 0;

        this._image = SixLabors.ImageSharp.Image.Load(dataSteam);
    }

    public Stream SaveData()
    {
        if (this._image == null)
        {
            return new MemoryStream();
        }

        var dataStream = new MemoryStream();
        var encoder = this._image.Configuration.ImageFormatsManager.GetEncoder(PngFormat.Instance);

        this._image.Save(dataStream, encoder);

        return dataStream;
    }
}

internal class EmptyImage : IImage
{
    private EmptyImage()
    {
    }

    public static EmptyImage Instance { get; } = new();

    public int Width => 0;

    public int Height => 0;

    public void LoadData(Stream dataSteam)
    {
        throw new NotSupportedException("Loading data is not supported!");
    }

    public Stream SaveData()
    {
        throw new NotSupportedException("Saving data is not supported!");
    }
}