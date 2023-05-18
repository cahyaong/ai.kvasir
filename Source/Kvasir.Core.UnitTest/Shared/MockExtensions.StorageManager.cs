// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MockExtensions.StorageManager.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, 22 November 2018 9:00:55 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace Moq.AI.Kvasir;

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Moq;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;
using nGratis.Cop.Olympus.Framework;

internal static partial class MockExtensions
{
    public static Mock<IStorageManager> WithEmptyCaching(this Mock<IStorageManager> mockManager, string name)
    {
        Guard
            .Require(mockManager, nameof(mockManager))
            .Is.Not.Null();

        var archiveBlob = default(byte[]);

        using (var archiveStream = new MemoryStream())
        {
            using (var _ = new ZipArchive(archiveStream, ZipArchiveMode.Create, true))
            {
            }

            archiveBlob = archiveStream.GetBuffer();
        }

        mockManager
            .Setup(mock => mock.LoadEntry(Arg.DataSpec.IsKvasirCaching(name)))
            .Returns(new MemoryStream(archiveBlob))
            .Verifiable();

        return mockManager
            .WithAvailability(new DataSpec(name, KvasirMime.Cache));
    }

    public static Mock<IStorageManager> WithCaching(
        this Mock<IStorageManager> mockManager,
        string name,
        string entityKey,
        string entityContent)
    {
        Guard
            .Require(mockManager, nameof(mockManager))
            .Is.Not.Null();

        Guard
            .Require(entityKey, nameof(entityKey))
            .Is.Not.Empty();

        Guard
            .Require(entityContent, nameof(entityContent))
            .Is.Not.Empty();

        var archiveBlob = default(byte[]);

        using (var archiveStream = new MemoryStream())
        {
            using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true))
            {
                var createdEntry = archive.CreateEntry(entityKey);

                using (var entryStream = createdEntry.Open())
                {
                    var buffer = Encoding.UTF8.GetBytes(entityContent);
                    entryStream.Write(buffer, 0, buffer.Length);
                    entryStream.Flush();
                }
            }

            archiveBlob = archiveStream.GetBuffer();
        }

        mockManager
            .Setup(mock => mock.LoadEntry(Arg.DataSpec.IsKvasirCaching(name)))
            .Returns(() => new MemoryStream(archiveBlob))
            .Verifiable();

        return mockManager
            .WithAvailability(new DataSpec(name, KvasirMime.Cache));
    }

    public static Mock<IStorageManager> WithSelfCaching(this Mock<IStorageManager> mockManager)
    {
        Guard
            .Require(mockManager, nameof(mockManager))
            .Is.Not.Null();

        var blobBtDataSpecLookup = new Dictionary<DataSpec, byte[]>();

        mockManager
            .Setup(mock => mock.SaveEntry(It.IsAny<DataSpec>(), It.IsAny<Stream>(), It.IsAny<bool>()))
            .Callback<DataSpec, Stream, bool>((spec, stream, _) =>
            {
                blobBtDataSpecLookup[spec] = stream.ReadBlob();

                mockManager
                    .Setup(mock => mock.HasEntry(spec))
                    .Returns(true)
                    .Verifiable();
            })
            .Verifiable();

        mockManager
            .Setup(mock => mock.LoadEntry(It.IsAny<DataSpec>()))
            .Returns<DataSpec>(spec =>
            {
                if (!blobBtDataSpecLookup.TryGetValue(spec, out var blob))
                {
                    return null;
                }

                var stream = new MemoryStream();
                stream.Write(blob, 0, blob.Length);
                stream.Position = 0;

                return stream;
            })
            .Verifiable();

        return mockManager;
    }
}