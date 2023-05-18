// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LuceneExtensions.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
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