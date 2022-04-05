// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Image.cs" company="nGratis">
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
// <creation_timestamp>Sunday, August 16, 2020 2:06:30 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core;

using System;
using System.IO;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;

public static class Image
{
    public static IImage Empty => EmptyImage.Instance;
}

public class WritableImage : IImage
{
    private SixLabors.ImageSharp.Image _image;

    public int Width => this._image.Width;

    public int Height => this._image.Height;

    public void LoadData(Stream dataSteam)
    {
        Guard
            .Require(dataSteam, nameof(dataSteam))
            .Is.Not.Null()
            .Is.Readable();

        // TODO: Handle a case when input data contains transparency.

        dataSteam.Position = 0;

        this._image = SixLabors.ImageSharp.Image.Load(dataSteam);
    }

    public Stream SaveData()
    {
        var dataStream = new MemoryStream();

        var encoder = this
            ._image
            .GetConfiguration()
            .ImageFormatsManager
            .FindEncoder(PngFormat.Instance);

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