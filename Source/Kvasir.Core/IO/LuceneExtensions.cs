// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LuceneExtensions.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2018 Cahya Ong
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

namespace Lucene.Net
{
    using System;
    using Lucene.Net.Analysis.Standard;
    using Lucene.Net.Index;
    using Lucene.Net.Store;
    using Lucene.Net.Util;
    using nGratis.Cop.Core.Contract;

    internal static class LuceneExtensions
    {
        public static IndexWriter CreateLuceneWriter(this Directory directory)
        {
            Guard
                .Require(directory, nameof(directory))
                .Is.Not.Null();

            var analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);

            var configuration = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer)
            {
                OpenMode = OpenMode.CREATE_OR_APPEND
            };

            return new IndexWriter(directory, configuration);
        }

        public static IndexReader CreateLuceneReader(this Directory directory)
        {
            Guard
                .Require(directory, nameof(directory))
                .Is.Not.Null();

            return DirectoryReader.Open(directory);
        }

        public static bool HasLuceneIndex(this Uri rootFolderUri, string indexName)
        {
            Guard
                .Require(rootFolderUri, nameof(rootFolderUri))
                .Is.Not.Null()
                .Is.Folder();

            indexName = indexName.Replace(" ", "_");

            Guard
                .Require(indexName, nameof(indexName))
                .Is.Not.Empty();

            return System.IO.Directory.Exists(System.IO.Path.Combine(rootFolderUri.LocalPath, indexName));
        }

        public static Directory CreateLuceneDirectory(this Uri rootFolderUri, string indexName)
        {
            Guard
                .Require(rootFolderUri, nameof(rootFolderUri))
                .Is.Not.Null()
                .Is.Folder();

            Guard
                .Require(indexName, nameof(indexName))
                .Is.Not.Empty();

            indexName = indexName.Replace(" ", "_");

            var indexFolderPath = System.IO.Path.Combine(rootFolderUri.LocalPath, indexName);

            if (!rootFolderUri.HasLuceneIndex(indexName))
            {
                System.IO.Directory.CreateDirectory(indexFolderPath);
            }

            return FSDirectory.Open(indexFolderPath);
        }
    }
}