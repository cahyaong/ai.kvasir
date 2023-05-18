// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexManager.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, 10 November 2018 5:43:31 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Lucene.Net;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public sealed class IndexManager : IIndexManager
{
    private readonly IReadOnlyDictionary<IndexKind, Directory> _directoryByIndexKindLookup;

    private readonly ConcurrentDictionary<IndexKind, Lazy<IndexWriter>> _deferredIndexWriterByIndexKindLookup;

    private bool _isDisposed;

    public IndexManager(Uri rootFolderUri)
    {
        this._directoryByIndexKindLookup = Enum
            .GetValues(typeof(IndexKind))
            .Cast<IndexKind>()
            .Where(indexKind => indexKind != IndexKind.Unknown)
            .ToDictionary(indexKind => indexKind, rootFolderUri.CreateLuceneDirectory);

        this._deferredIndexWriterByIndexKindLookup = new ConcurrentDictionary<IndexKind, Lazy<IndexWriter>>();
    }

    public bool HasIndex(IndexKind indexKind)
    {
        Guard
            .Require(indexKind, nameof(indexKind))
            .Is.Not.Default();

        return
            this._directoryByIndexKindLookup.TryGetValue(indexKind, out var directory) &&
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

        if (!this._directoryByIndexKindLookup.TryGetValue(indexKind, out var directory))
        {
            throw new KvasirException($"Lucene directory is not registered for [{indexKind}]!");
        }

        IndexWriter CreateIndexWriter()
        {
            var analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);

            var configuration = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer)
            {
                OpenMode = OpenMode.CREATE_OR_APPEND
            };

            return new IndexWriter(directory, configuration);
        }

        return this._deferredIndexWriterByIndexKindLookup
            .GetOrAdd(
                indexKind,
                _ => new Lazy<IndexWriter>(CreateIndexWriter, LazyThreadSafetyMode.ExecutionAndPublication))
            .Value;
    }

    public void Dispose()
    {
        if (this._isDisposed)
        {
            return;
        }

        this
            ._deferredIndexWriterByIndexKindLookup
            .Values
            .Where(deferredWriter => deferredWriter.IsValueCreated)
            .ForEach(deferredWriter => deferredWriter.Value.Dispose());

        this._isDisposed = true;
    }
}