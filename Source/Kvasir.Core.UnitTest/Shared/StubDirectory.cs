﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StubDirectory.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Wednesday, 7 November 2018 9:20:10 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.UnitTest;

using System;
using System.Collections.Generic;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Documents.Extensions;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;
using Polly;

internal class StubDirectory : RAMDirectory
{
    private StubDirectory()
    {
    }

    public static StubDirectory Create()
    {
        return new StubDirectory();
    }

    public StubDirectory WithUnparsedCardSet(params UnparsedBlob.CardSet[] cardSets)
    {
        Guard
            .Require(cardSets, nameof(cardSets))
            .Is.Not.Empty();

        using var luceneWriter = new IndexWriter(this, StubDirectory.CreateLuceneConfiguration());

        foreach (var cardSet in cardSets)
        {
            var document = new Document();

            document.AddStringField("code", cardSet.Code, Field.Store.YES);
            document.AddStringField("name", cardSet.Name, Field.Store.YES);
            document.AddInt64Field("released-timestamp", cardSet.ReleasedTimestamp.Ticks, Field.Store.YES);

            luceneWriter.AddDocument(document);
        }

        luceneWriter.CommitWithRetrying();

        return this;
    }

    public StubDirectory WithUnparsedCard(params UnparsedBlob.Card[] cards)
    {
        Guard
            .Require(cards, nameof(cards))
            .Is.Not.Empty();

        using var luceneWriter = new IndexWriter(this, StubDirectory.CreateLuceneConfiguration());

        foreach (var card in cards)
        {
            var document = new Document();

            document.AddInt32Field("multiverse-id", card.MultiverseId, Field.Store.YES);
            document.AddStringField("scryfall-id", card.ScryfallId, Field.Store.YES);
            document.AddStringField("scryfall-image-url", card.ScryfallImageUrl, Field.Store.YES);
            document.AddStringField("set-code", card.SetCode, Field.Store.YES);
            document.AddStringField("name", card.Name, Field.Store.YES);
            document.AddStringField("mana-cost", card.ManaCost, Field.Store.YES);
            document.AddStringField("type", card.Type, Field.Store.YES);
            document.AddStringField("rarity", card.Rarity, Field.Store.YES);
            document.AddStringField("text", card.Text, Field.Store.YES);
            document.AddStringField("flavor-text", card.FlavorText, Field.Store.YES);
            document.AddStringField("power", card.Power, Field.Store.YES);
            document.AddStringField("toughness", card.Toughness, Field.Store.YES);
            document.AddStringField("number", card.Number, Field.Store.YES);
            document.AddStringField("artist", card.Artist, Field.Store.YES);

            luceneWriter.AddDocument(document);
        }

        luceneWriter.CommitWithRetrying();

        return this;
    }

    public StubDirectory WithDocument(params IEnumerable<IIndexableField>[] documents)
    {
        Guard
            .Require(documents, nameof(documents))
            .Is.Not.Empty();

        using var luceneWriter = new IndexWriter(this, StubDirectory.CreateLuceneConfiguration());

        luceneWriter.AddDocuments(documents);
        luceneWriter.CommitWithRetrying();

        return this;
    }

    private static IndexWriterConfig CreateLuceneConfiguration()
    {
        var analyzer = new StandardAnalyzer(LuceneVersion.LUCENE_48);

        return new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer)
        {
            OpenMode = OpenMode.CREATE_OR_APPEND
        };
    }
}

internal static class LuceneExtensions
{
    internal static void CommitWithRetrying(this IndexWriter luceneWriter)
    {
        Guard
            .Require(luceneWriter, nameof(luceneWriter))
            .Is.Not.Null();

        Policy
            .Handle<InvalidCastException>()
            .Retry(3)
            .Execute(luceneWriter.Commit);
    }
}