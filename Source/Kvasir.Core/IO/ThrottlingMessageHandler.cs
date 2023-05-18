// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThrottlingMessageHandler.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, 15 December 2018 9:48:16 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core;

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public class ThrottlingMessageHandler : DelegatingHandler
{
    private readonly TimeSpan _waitingDuration;

    private readonly ConcurrentDictionary<string, Lazy<ThrottlingInfo>> _deferredThrottlingInfoByUrlLookup;

    public ThrottlingMessageHandler(TimeSpan waitingDuration, HttpMessageHandler delegatingHandler)
        : base(delegatingHandler)
    {
        this._waitingDuration = waitingDuration > TimeSpan.Zero
            ? waitingDuration
            : Default.WaitingDuration;

        this._deferredThrottlingInfoByUrlLookup = new ConcurrentDictionary<string, Lazy<ThrottlingInfo>>();
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var hostUrl = request.RequestUri?.Host;

        if (string.IsNullOrEmpty(hostUrl))
        {
            throw new KvasirException("Request message must have valid host URL!");
        }

        ThrottlingInfo CreateThrottlingInfo() => new(
            hostUrl,
            DateTimeOffset.UtcNow - this._waitingDuration);

        var throttlingInfo = this._deferredThrottlingInfoByUrlLookup
            .GetOrAdd(
                hostUrl,
                _ => new Lazy<ThrottlingInfo>(CreateThrottlingInfo, LazyThreadSafetyMode.ExecutionAndPublication))
            .Value;

        await throttlingInfo.Semaphore.WaitAsync(cancellationToken);

        try
        {
            var elapsedDuration = DateTimeOffset.UtcNow - throttlingInfo.LastExecutionTimestamp;

            if (elapsedDuration < this._waitingDuration)
            {
                await Task.Delay(this._waitingDuration - elapsedDuration, cancellationToken);
            }

            var response = await base.SendAsync(request, cancellationToken);

            throttlingInfo.LastExecutionTimestamp = DateTimeOffset.UtcNow;

            return response;
        }
        finally
        {
            throttlingInfo.Semaphore.Release();
        }
    }

    private static class Default
    {
        public static readonly TimeSpan WaitingDuration = TimeSpan.FromSeconds(3);
    }

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    private sealed class ThrottlingInfo
    {
        public ThrottlingInfo(string hostUrl, DateTimeOffset lastExecutionTimestamp)
        {
            Guard
                .Require(hostUrl, nameof(hostUrl))
                .Is.Not.Empty();

            this.HostUrl = hostUrl;
            this.LastExecutionTimestamp = lastExecutionTimestamp;
            this.Semaphore = new SemaphoreSlim(1, 1);
        }

        public string HostUrl { get; }

        public DateTimeOffset LastExecutionTimestamp { get; set; }

        public SemaphoreSlim Semaphore { get; }
    }
}