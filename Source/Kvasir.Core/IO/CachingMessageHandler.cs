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
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading;
    using System.Threading.Tasks;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Olympus.Contract;
    using nGratis.Cop.Olympus.Framework;

    internal sealed class CachingMessageHandler : DelegatingHandler
    {
        private readonly CompressedStorageManager _sharedStorageManager;

        private readonly CompressedStorageManager _imageStorageManager;

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

            // TODO: Consider injecting compressed storage managers!

            this._sharedStorageManager = new CompressedStorageManager(
                new DataSpec(name, KvasirMime.Cache),
                storageManager);

            this._imageStorageManager = new CompressedStorageManager(
                new DataSpec($"{name}_Image", KvasirMime.Cache),
                storageManager);

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

            var storageManager = entrySpec.Mime.IsImage
                ? this._imageStorageManager
                : this._sharedStorageManager;

            var foundBlob = default(byte[]);

            if (entrySpec != DataSpec.None)
            {
                await using var foundStream = storageManager.LoadEntry(entrySpec);

                foundBlob = foundStream.ReadBlob();
            }

            if (foundBlob?.Any() == true)
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
                this._sharedStorageManager.Dispose();
                this._imageStorageManager.Dispose();
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
                ? this._imageStorageManager
                : this._sharedStorageManager;

            if (request.EntrySpec != DataSpec.None)
            {
                cachingPool.SaveEntry(
                    request.EntrySpec,
                    new MemoryStream(request.Blob),
                    true);
            }
        }

        private sealed class SavingRequest
        {
            public DataSpec EntrySpec { get; set; }

            public byte[] Blob { get; set; }
        }
    }
}