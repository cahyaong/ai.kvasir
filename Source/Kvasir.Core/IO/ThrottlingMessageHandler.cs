// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThrottlingMessageHandler.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2021 Cahya Ong
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
// <creation_timestamp>Saturday, 15 December 2018 9:48:16 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using nGratis.Cop.Olympus.Contract;

    public class ThrottlingMessageHandler : DelegatingHandler
    {
        private readonly TimeSpan _waitingDuration;

        private readonly ConcurrentDictionary<string, Lazy<ThrottlingInfo>> _deferredInfoLookup;

        public ThrottlingMessageHandler(TimeSpan waitingDuration, HttpMessageHandler delegatingHandler)
            : base(delegatingHandler)
        {
            Guard
                .Require(delegatingHandler, nameof(delegatingHandler))
                .Is.Not.Null();

            this._waitingDuration = waitingDuration > TimeSpan.Zero
                ? waitingDuration
                : Default.WaitingDuration;

            this._deferredInfoLookup = new ConcurrentDictionary<string, Lazy<ThrottlingInfo>>();
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage requestMessage,
            CancellationToken cancellationToken)
        {
            var hostUrl = requestMessage.RequestUri.Host;

            ThrottlingInfo CreateThrottlingInfo() => new ThrottlingInfo(
                hostUrl,
                DateTimeOffset.UtcNow - this._waitingDuration);

            var throttlingInfo = this._deferredInfoLookup
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

                var responseMessage = await base.SendAsync(requestMessage, cancellationToken);

                throttlingInfo.LastExecutionTimestamp = DateTimeOffset.UtcNow;

                return responseMessage;
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
}