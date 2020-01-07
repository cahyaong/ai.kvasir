// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MockExtensions.StorageManager.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2020 Cahya Ong
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
// <creation_timestamp>Thursday, 22 November 2018 9:00:55 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Test
{
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using Moq;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Core.Contract;

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
                .WithData(name, KvasirMime.Caching);
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
                .WithData(name, KvasirMime.Caching);
        }

        public static Mock<IStorageManager> WithSelfCaching(this Mock<IStorageManager> mockManager)
        {
            Guard
                .Require(mockManager, nameof(mockManager))
                .Is.Not.Null();

            var blobLookup = new Dictionary<DataSpec, byte[]>();

            mockManager
                .Setup(mock => mock.SaveEntry(It.IsAny<DataSpec>(), It.IsAny<Stream>(), It.IsAny<bool>()))
                .Callback<DataSpec, Stream, bool>((spec, stream, _) =>
                 {
                     blobLookup[spec] = stream.ReadBlob();

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
                    if (!blobLookup.TryGetValue(spec, out var blob))
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

        private static Mock<IStorageManager> WithData(this Mock<IStorageManager> mockManager, string name, Mime mime)
        {
            mockManager
                .Setup(mock => mock.IsAvailable)
                .Returns(true)
                .Verifiable();

            mockManager
                .Setup(mock => mock.FindEntries(name, mime))
                .Returns(new[] { new DataInfo(name, mime) })
                .Verifiable();

            mockManager
                .Setup(mock => mock.HasEntry(Arg.DataSpec.Is(name, mime)))
                .Returns(true)
                .Verifiable();

            return mockManager;
        }
    }
}