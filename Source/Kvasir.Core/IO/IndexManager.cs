// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexManager.cs" company="nGratis">
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
// <creation_timestamp>Saturday, 10 November 2018 5:43:31 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Lucene.Net;
    using Lucene.Net.Analysis.Standard;
    using Lucene.Net.Index;
    using Lucene.Net.Store;
    using Lucene.Net.Util;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Core.Contract;

    public sealed class IndexManager : IIndexManager
    {
        private readonly IReadOnlyDictionary<IndexKind, Directory> _directoryLookup;

        private readonly ConcurrentDictionary<IndexKind, IndexWriter> _writerLookup;

        private bool _isDisposed;

        public IndexManager(Uri rootFolderUri)
        {
            Guard
                .Require(rootFolderUri, nameof(rootFolderUri))
                .Is.Not.Null();

            this._directoryLookup = Enum
                .GetValues(typeof(IndexKind))
                .Cast<IndexKind>()
                .Where(indexKind => indexKind != IndexKind.Undefined)
                .ToDictionary(indexKind => indexKind, rootFolderUri.CreateLuceneDirectory);

            this._writerLookup = new ConcurrentDictionary<IndexKind, IndexWriter>();
        }

        public bool HasIndex(IndexKind indexKind)
        {
            Guard
                .Require(indexKind, nameof(indexKind))
                .Is.Not.Default();

            return
                this._directoryLookup.TryGetValue(indexKind, out var directory) &&
                directory.ListAll().Any();
        }

        public IndexReader FindIndexReader(IndexKind indexKind)
        {
            Guard
                .Require(indexKind, nameof(indexKind))
                .Is.Not.Default();

            return this
                .FindIndexWriter(indexKind)
                .GetReader(true);
        }

        public IndexWriter FindIndexWriter(IndexKind indexKind)
        {
            Guard
                .Require(indexKind, nameof(indexKind))
                .Is.Not.Default();

            if (!this._directoryLookup.TryGetValue(indexKind, out var directory))
            {
                throw new KvasirException($"Lucene directory is not registered for [{indexKind}]!");
            }

            return this._writerLookup.GetOrAdd(
                indexKind,
                _ =>
                {
                    var analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);

                    var configuration = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer)
                    {
                        OpenMode = OpenMode.CREATE_OR_APPEND
                    };

                    return new IndexWriter(directory, configuration);
                });
        }

        public void Dispose()
        {
            if (this._isDisposed)
            {
                return;
            }

            this
                ._writerLookup?
                .Values
                .ForEach(writer => writer.Dispose());

            this._isDisposed = true;
        }
    }
}