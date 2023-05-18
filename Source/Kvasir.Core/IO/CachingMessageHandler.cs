// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CachingMessageHandler.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, 17 November 2018 9:54:32 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core;

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
            .Is.Not.Empty();

        // TODO: Consider injecting compressed storage managers!

        this._sharedStorageManager = new CompressedStorageManager(
            new DataSpec(name, KvasirMime.Cache),
            storageManager);

        this._imageStorageManager = new CompressedStorageManager(
            new DataSpec($"{name}_Image", KvasirMime.Cache),
            storageManager);

        this._keyCalculator = keyCalculator;
        this._whenEntrySavingRequested = new Subject<SavingRequest>();

        this._whenEntrySavingRequested
            .ObserveOn(ThreadPoolScheduler.Instance)
            .Subscribe(this.SaveEntry);
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var response = default(HttpResponseMessage);

        var entrySpec = request.RequestUri != null
            ? this._keyCalculator.Calculate(request.RequestUri)
            : throw new KvasirException("Request URI is not defined!");

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
            response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(foundBlob)
            };
        }
        else
        {
            response = await base.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode && !this._whenEntrySavingRequested.IsDisposed)
            {
                this._whenEntrySavingRequested.OnNext(new SavingRequest
                {
                    EntrySpec = entrySpec,
                    Blob = await response.Content.ReadAsByteArrayAsync(cancellationToken)
                });
            }
        }

        return response;
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
        public DataSpec EntrySpec { get; init; } = DataSpec.None;

        public byte[] Blob { get; init; } = Array.Empty<byte>();
    }
}