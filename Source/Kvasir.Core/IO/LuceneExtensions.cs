// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LuceneExtensions.cs" company="nGratis">
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
// <creation_timestamp>Thursday, 1 November 2018 8:34:23 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace Lucene.Net;

using System;
using Lucene.Net.Store;
using nGratis.AI.Kvasir.Core;
using nGratis.Cop.Olympus.Contract;

internal static partial class LuceneExtensions
{
    public static Directory CreateLuceneDirectory(this Uri rootFolderUri, IndexKind indexKind)
    {
        Guard
            .Require(rootFolderUri, nameof(rootFolderUri))
            .Is.Not.Null()
            .Is.Folder()
            .Is.Exist();

        Guard
            .Require(indexKind, nameof(indexKind))
            .Is.Not.Default();

        var indexFolderPath = System.IO.Path.Combine(rootFolderUri.LocalPath, $"Lucene_{indexKind}");

        if (!System.IO.Directory.Exists(indexFolderPath))
        {
            System.IO.Directory.CreateDirectory(indexFolderPath);
        }

        return FSDirectory.Open(indexFolderPath);
    }
}