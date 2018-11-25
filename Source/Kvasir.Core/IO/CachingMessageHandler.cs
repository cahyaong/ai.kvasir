// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CachingMessageHandler.cs" company="nGratis">
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
// <creation_timestamp>Saturday, 17 November 2018 9:54:32 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core
{
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Core;
    using nGratis.Cop.Core.Contract;

    internal class CachingMessageHandler : DelegatingHandler
    {
        private readonly DataSpec _cachingSpec;

        private readonly IStorageManager _storageManager;

        public CachingMessageHandler(string name, IStorageManager storageManager, HttpMessageHandler delegatingHandler)
            : base(delegatingHandler)
        {
            Guard
                .Require(name, nameof(name))
                .Is.Not.Null();

            Guard
                .Require(storageManager, nameof(storageManager))
                .Is.Not.Null();

            this._cachingSpec = new DataSpec(name, KvasirMime.Caching);
            this._storageManager = storageManager;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage requestMessage,
            CancellationToken cancellationToken)
        {
            Guard
                .Require(requestMessage, nameof(requestMessage))
                .Is.Not.Null();

            var responseMessage = default(HttpResponseMessage);

            var archiveStream = default(Stream);
            var archive = default(ZipArchive);
            var foundEntry = default(ZipArchiveEntry);
            var isModified = false;
            var entryKey = requestMessage.RequestUri.Segments.Last();

            try
            {
                archiveStream = new MemoryStream();

                if (this._storageManager.HasEntry(this._cachingSpec))
                {
                    using (var dataStream = this._storageManager.LoadEntry(this._cachingSpec))
                    {
                        await dataStream.CopyToAsync(archiveStream);
                        archive = new ZipArchive(archiveStream, ZipArchiveMode.Update, true);
                    }

                    foundEntry = archive
                        .Entries
                        .SingleOrDefault(entry => entry.FullName == entryKey);
                }
                else
                {
                    archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true);
                }

                if (foundEntry != null)
                {
                    using (var entryStream = foundEntry.Open())
                    {
                        responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(entryStream.AsString())
                        };
                    }
                }
                else
                {
                    responseMessage = await base.SendAsync(requestMessage, cancellationToken);

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var createdEntry = archive.CreateEntry(entryKey, CompressionLevel.Optimal);

                        using (var entryStream = createdEntry.Open())
                        {
                            var buffer = Encoding.UTF8.GetBytes(await responseMessage.Content.ReadAsStringAsync());
                            entryStream.Write(buffer, 0, buffer.Length);
                            await entryStream.FlushAsync(cancellationToken);
                        }

                        isModified = true;
                    }
                }

                archive.Dispose();

                if (isModified)
                {
                    this._storageManager.SaveEntry(this._cachingSpec, archiveStream, true);
                }
            }
            catch
            {
                archive?.Dispose();
                throw;
            }
            finally
            {
                archiveStream?.Dispose();
            }

            return responseMessage;
        }
    }
}