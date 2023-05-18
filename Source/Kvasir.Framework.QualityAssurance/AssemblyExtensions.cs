// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyExtensions.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, May 5, 2020 6:00:27 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace System.Reflection;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Framework;
using nGratis.Cop.Olympus.Contract;
using YamlDotNet.Serialization;

public static class AssemblyExtensions
{
    public static Stream FetchSessionStream(this string name)
    {
        Guard
            .Require(name, nameof(name))
            .Is.Not.Empty();

        var dataStream = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream($"nGratis.AI.Kvasir.Framework.Data.{name}.ngksession");

        if (dataStream == null)
        {
            throw new KvasirTestingException(
                "Session data must be embedded!",
                ("Name", name));
        }

        return dataStream;
    }

    public static IEnumerable<StubCreature> FetchCreatures(this string name)
    {
        Guard
            .Require(name, nameof(name))
            .Is.Not.Empty();

        using var dataStream = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream($"nGratis.AI.Kvasir.Framework.Data.{name}.ngkcard");

        if (dataStream == null)
        {
            throw new KvasirTestingException(
                "Creatures data must be embedded!",
                ("Name", name));
        }

        return dataStream
            .ReadText()
            .DeserializeFromYaml<List<StubCreature>>()
            .ToImmutableArray();
    }

    public static IEnumerable<DefinedBlob.Card> FetchProcessedCards(this string name)
    {
        Guard
            .Require(name, nameof(name))
            .Is.Not.Empty();

        using var dataStream = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream($"nGratis.AI.Kvasir.Framework.Data.{name}.ngkset");

        if (dataStream == null)
        {
            throw new KvasirTestingException(
                "Cards data must be embedded!",
                ("Name", name));
        }

        return dataStream
            .ReadText()
            .DeserializeFromYaml<List<DefinedBlob.Card>>()
            .ToImmutableArray();
    }
}