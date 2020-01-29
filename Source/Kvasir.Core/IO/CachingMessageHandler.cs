// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CachingMessageHandler.cs" company="nGratis">
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

    internal sealed class CachingMessageHandler : DelegatingHandler
    {
        private readonly CachingPool _sharedCachingPool;

        private readonly CachingPool _imageCachingPool;

        private readonly IKeyCalculator _keyCalculator;

        private readonly Subject<SavingRequest> _whenEntrySavingRequested;

        private bool _isDisposed;

        public CachingMessageHandler(
            string name,
            IStorageManager storageManager,
            IKeyCalculator keyCalculator,
            HttpMessageHandler delegatingHandler)
            : base(delegatingHandler)
        {
            Guard
                .Require(name, nameof(name))
                .Is.Not.Null();

            this._sharedCachingPool = new CachingPool(name, storageManager);
            this._imageCachingPool = new CachingPool($"{name}_Image", storageManager);

            this._keyCalculator = keyCalculator ?? SimpleKeyCalculator.Instance;
            this._whenEntrySavingRequested = new Subject<SavingRequest>();

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

            var entrySpec = this._keyCalculator.Calculate(requestMessage.RequestUri);

            if (entrySpec == null)
            {
                throw new KvasirException(
                    $"Entry spec. is not calculated properly for URL [{requestMessage.RequestUri}]. " +
                    $"Calculator: [{this._keyCalculator.GetType().FullName}].");
            }

            var cachingPool = entrySpec.Mime.IsImage
                ? this._imageCachingPool
                : this._sharedCachingPool;

            var foundBlob = entrySpec != DataSpec.None
                ? cachingPool.LoadEntry($"{entrySpec.Name}{entrySpec.Mime.FileExtension}")
                : new byte[0];

            if (foundBlob.Any())
            {
                responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(foundBlob)
                };
            }
            else
            {
                responseMessage = await base.SendAsync(requestMessage, cancellationToken);

                if (responseMessage.IsSuccessStatusCode && !this._whenEntrySavingRequested.IsDisposed)
                {
                    this._whenEntrySavingRequested.OnNext(new SavingRequest
                    {
                        EntrySpec = entrySpec,
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
                this._sharedCachingPool.Dispose();
                this._imageCachingPool.Dispose();
                this._whenEntrySavingRequested.Dispose();
            }

            base.Dispose(isDisposing);

            this._isDisposed = true;
        }

        private void SaveEntry(SavingRequest request)
        {
            if (this._isDisposed)
            {
                return;
            }

            var cachingPool = request.EntrySpec.Mime.IsImage
                ? this._imageCachingPool
                : this._sharedCachingPool;

            if (request.EntrySpec != DataSpec.None)
            {
                cachingPool.SaveEntry(
                    $"{request.EntrySpec.Name}{request.EntrySpec.Mime.FileExtension}",
                    request.Blob);
            }
        }

        private sealed class SavingRequest
        {
            public DataSpec EntrySpec { get; set; }

            public byte[] Blob { get; set; }
        }

        private sealed class CachingPool : IDisposable
        {
            private readonly DataSpec _archiveSpec;

            private readonly ReaderWriterLockSlim _archiveLock;

            private readonly IStorageManager _storageManager;

            private readonly Lazy<ZipArchive> _deferredArchive;

            private bool _isDisposed;

            public CachingPool(string name, IStorageManager storageManager)
            {
                Guard
                    .Require(name, nameof(name))
                    .Is.Not.Empty();

                Guard
                    .Require(storageManager, nameof(storageManager))
                    .Is.Not.Null();

                this._archiveSpec = new DataSpec(name, KvasirMime.Caching);
                this._archiveLock = new ReaderWriterLockSlim();
                this._storageManager = storageManager;

                this._deferredArchive = new Lazy<ZipArchive>(
                    this.LoadArchive,
                    LazyThreadSafetyMode.ExecutionAndPublication);
            }

            ~CachingPool()
            {
                this.Dispose(false);
            }

            public byte[] LoadEntry(string key)
            {
                Guard
                    .Require(key, nameof(key))
                    .Is.Not.Empty();

                var foundEntry = default(ZipArchiveEntry);

                this._archiveLock.EnterReadLock();

                try
                {
                    foundEntry = this
                        ._deferredArchive.Value
                        .Entries
                        .SingleOrDefault(entry => entry.FullName == key);
                }
                finally
                {
                    this._archiveLock.ExitReadLock();
                }

                if (foundEntry == null)
                {
                    return new byte[0];
                }

                using (var entryStream = foundEntry.Open())
                {
                    return entryStream.ReadBlob();
                }
            }

            public void SaveEntry(string key, byte[] blob)
            {
                Guard
                    .Require(key, nameof(key))
                    .Is.Not.Empty();

                Guard
                    .Require(blob, nameof(blob))
                    .Is.Not.Null()
                    .Is.Not.Empty();

                if (this._isDisposed)
                {
                    return;
                }

                var isEntryExisted = this
                    ._deferredArchive.Value
                    .Entries
                    .Any(entry => entry.Name == key);

                if (isEntryExisted)
                {
                    return;
                }

                this._archiveLock.EnterWriteLock();

                try
                {
                    var createdEntry = this
                        ._deferredArchive.Value
                        .CreateEntry(key, CompressionLevel.Optimal);

                    using (var entryStream = createdEntry.Open())
                    {
                        entryStream.Write(blob, 0, blob.Length);
                        entryStream.Flush();
                    }
                }
                finally
                {
                    this._archiveLock.ExitWriteLock();
                }
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            private ZipArchive LoadArchive()
            {
                if (!this._storageManager.HasEntry(this._archiveSpec))
                {
                    var archiveStream = new MemoryStream();

                    try
                    {
                        var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true);
                        archive.Dispose();

                        this._storageManager.SaveEntry(this._archiveSpec, archiveStream, false);
                    }
                    finally
                    {
                        archiveStream.Dispose();
                    }
                }

                var dataStream = this._storageManager.LoadEntry(this._archiveSpec);

                return new ZipArchive(dataStream, ZipArchiveMode.Update, true);
            }

            private void Dispose(bool isDisposing)
            {
                if (this._isDisposed)
                {
                    return;
                }

                if (isDisposing)
                {
                    if (this._deferredArchive.IsValueCreated)
                    {
                        this._archiveLock.EnterWriteLock();

                        try
                        {
                            this._deferredArchive.Value.Dispose();
                        }
                        finally
                        {
                            this._archiveLock.ExitWriteLock();
                        }
                    }
                }

                this._isDisposed = true;
            }
        }
    }
}