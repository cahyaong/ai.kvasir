// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThrottlingMessageHandlerTests.cs" company="nGratis">
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
// <creation_timestamp>Saturday, 15 December 2018 9:50:49 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Test
{
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading.Tasks;
    using FluentAssertions;
    using FluentAssertions.Extensions;
    using nGratis.AI.Kvasir.Framework;
    using Xunit;

    public class ThrottlingMessageHandlerTests
    {
        public class SendAsyncMethod
        {
            [Fact]
            public async Task WhenMakingRequestWithinWaitingDuration_ShouldDelayRequest()
            {
                // Arrange.

                var stubHandler = StubHttpMessageHandler
                    .Create()
                    .WithSuccessfulResponse("http://www.mock-url.com", "[_MOCK_HTML_CONTENT_]");

                var throttlingHandler = new ThrottlingMessageHandler(3.Seconds(), stubHandler);

                var responseMessage = default(HttpResponseMessage);
                var stopwatch = new Stopwatch();

                // Act.

                using (var client = new HttpClient(throttlingHandler))
                {
                    stopwatch.Start();

                    responseMessage = await client.GetAsync("http://www.mock-url.com");
                    responseMessage = await client.GetAsync("http://www.mock-url.com");

                    stopwatch.Stop();
                }

                // Assert.

                stopwatch
                    .Elapsed
                    .Should().BeGreaterOrEqualTo(2950.Milliseconds());

                responseMessage
                    .Should().NotBeNull();

                var content = await responseMessage.Content.ReadAsStringAsync();

                content
                    .Should().Be("[_MOCK_HTML_CONTENT_]");
            }

            [Fact]
            public async Task WhenMakingRequestToDifferentTargetUrl_ShouldTrackWaitingDurationIndependently()
            {
                // Arrange.

                var stubHandler = StubHttpMessageHandler
                    .Create()
                    .WithSuccessfulResponse("http://www.mock-url.com", "[_MOCK_HTML_CONTENT_]")
                    .WithSuccessfulResponse("http://www.another-mock-url.com", "[_MOCK_HTML_CONTENT_]");

                var throttlingHandler = new ThrottlingMessageHandler(1.Minutes(), stubHandler);

                var responseMessage = default(HttpResponseMessage);
                var stopwatch = new Stopwatch();

                // Act.

                using (var client = new HttpClient(throttlingHandler))
                {
                    stopwatch.Start();

                    responseMessage = await client.GetAsync("http://www.mock-url.com");
                    responseMessage = await client.GetAsync("http://www.another-mock-url.com");

                    stopwatch.Stop();
                }

                // Assert.

                stopwatch
                    .Elapsed
                    .Should().BeLessThan(1.Minutes());

                responseMessage
                    .Should().NotBeNull();
            }
        }
    }
}