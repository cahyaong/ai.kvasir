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
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Core.Contract;

    internal class CachingMessageHandler : DelegatingHandler
    {
        private readonly DataSpec _cachingSpec;

        private readonly IStorageManager _storageManager;

        private readonly ReaderWriterLockSlim _storageLock;

        private readonly IUniqueKeyCalculator _keyCalculator;

        private readonly Lazy<ZipArchive> _deferredArchive;

        private readonly Subject<SavingRequest> _whenEntrySavingRequested;

        private bool _isDisposed;

        public CachingMessageHandler(
            string name,
            IStorageManager storageManager,
            IUniqueKeyCalculator cachingKeyCalculator,
            HttpMessageHandler delegatingHandler)
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
            this._storageLock = new ReaderWriterLockSlim();
            this._keyCalculator = cachingKeyCalculator ?? DefaultUniqueKeyCalculator.Instance;
            this._whenEntrySavingRequested = new Subject<SavingRequest>();

            this._deferredArchive = new Lazy<ZipArchive>(
                this.LoadArchive,
                LazyThreadSafetyMode.ExecutionAndPublication);

            this._whenEntrySavingRequested
                .ObserveOn(ThreadPoolScheduler.Instance)
                .Subscribe(this.SaveEntry);
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage requestMessage,
            CancellationToken cancellationToken)
        {
            Guard
                .Require(requestMessage, nameof(requestMessage))
                .Is.Not.Null();

            var responseMessage = default(HttpResponseMessage);

            var foundEntry = default(ZipArchiveEntry);
            var entryKey = this._keyCalculator.Calculate(requestMessage.RequestUri);

            if (string.IsNullOrEmpty(entryKey))
            {
                throw new KvasirException(
                    $"Entry key is not calculated properly for URL [{requestMessage.RequestUri}]. " +
                    $"Calculator: [{this._keyCalculator.GetType().FullName}].");
            }

            this._storageLock.EnterReadLock();

            try
            {
                foundEntry = this
                    ._deferredArchive.Value
                    .Entries
                    .SingleOrDefault(entry => entry.FullName == entryKey);
            }
            finally
            {
                this._storageLock.ExitReadLock();
            }

            if (foundEntry != null)
            {
                using (var entryStream = foundEntry.Open())
                {
                    responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(entryStream.ReadBlob())
                    };
                }
            }
            else
            {
                responseMessage = await base.SendAsync(requestMessage, cancellationToken);

                if (responseMessage.IsSuccessStatusCode && !this._whenEntrySavingRequested.IsDisposed)
                {
                    this._whenEntrySavingRequested.OnNext(new SavingRequest
                    {
                        EntryKey = entryKey,
                        Blob = await responseMessage.Content.ReadAsByteArrayAsync()
                    });
                }
            }

            return responseMessage;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (this._isDisposed)
            {
                return;
            }

            if (isDisposing)
            {
                this._whenEntrySavingRequested.Dispose();

                this._storageLock.EnterWriteLock();

                if (this._deferredArchive.IsValueCreated)
                {
                    try
                    {
                        this._deferredArchive.Value.Dispose();
                    }
                    finally
                    {
                        this._storageLock.ExitWriteLock();
                    }
                }
            }

            base.Dispose(isDisposing);

            this._isDisposed = true;
        }

        private ZipArchive LoadArchive()
        {
            if (!this._storageManager.HasEntry(this._cachingSpec))
            {
                var archiveStream = new MemoryStream();

                try
                {
                    var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true);
                    archive.Dispose();

                    this._storageManager.SaveEntry(this._cachingSpec, archiveStream, false);
                }
                finally
                {
                    archiveStream.Dispose();
                }
            }

            var dataStream = this._storageManager.LoadEntry(this._cachingSpec);

            return new ZipArchive(dataStream, ZipArchiveMode.Update, true);
        }

        private void SaveEntry(SavingRequest request)
        {
            if (this._isDisposed)
            {
                return;
            }

            var isEntryExisted = this
                ._deferredArchive.Value
                .Entries
                .Any(entry => entry.Name == request.EntryKey);

            if (isEntryExisted)
            {
                return;
            }

            this._storageLock.EnterWriteLock();

            try
            {
                var createdEntry = this
                    ._deferredArchive.Value
                    .CreateEntry(request.EntryKey, CompressionLevel.Optimal);

                using (var entryStream = createdEntry.Open())
                {
                    entryStream.Write(request.Blob, 0, request.Blob.Length);
                    entryStream.Flush();
                }
            }
            finally
            {
                this._storageLock.ExitWriteLock();
            }
        }

        private sealed class SavingRequest
        {
            public string EntryKey { get; set; }

            public byte[] Blob { get; set; }
        }
    }
}