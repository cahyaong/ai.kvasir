// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleKeyCalculator.cs" company="nGratis">
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
// <creation_timestamp>Friday, 21 December 2018 11:40:37 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core;

using System;
using System.IO;
using System.Linq;
using nGratis.Cop.Olympus.Contract;

internal class SimpleKeyCalculator : IKeyCalculator
{
    private SimpleKeyCalculator()
    {
    }

    public static SimpleKeyCalculator Instance { get; } = new();

    public DataSpec Calculate(Uri uri)
    {
        var key = uri.Segments.LastOrDefault();

        if (string.IsNullOrEmpty(key))
        {
            return new DataSpec(Default.Name, Mime.Unknown);
        }

        var name = Path.GetFileNameWithoutExtension(key);
        var mime = Mime.ParseByExtension(Path.GetExtension(key));

        return new DataSpec(name, mime);
    }

    public static class Default
    {
        public const string Name = "_unknown";
    }
}