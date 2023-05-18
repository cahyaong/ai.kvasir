// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IIndexManager.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, 10 November 2018 5:41:54 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core;

using System;
using Lucene.Net.Index;

public interface IIndexManager : IDisposable
{
    bool HasIndex(IndexKind indexKind);

    IndexReader FindIndexReader(IndexKind indexKind);

    IndexWriter FindIndexWriter(IndexKind indexKind);
}