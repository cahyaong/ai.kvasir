// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThrottlingMessageHandlerTests.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, 15 December 2018 9:50:49 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.UnitTest;

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

            var throttlingHandler = new ThrottlingMessageHandler(1.Seconds(), stubHandler);

            var response = default(HttpResponseMessage);
            var stopwatch = new Stopwatch();

            // Act.

            using (var client = new HttpClient(throttlingHandler))
            {
                stopwatch.Start();

                response = await client.GetAsync("http://www.mock-url.com");
                response = await client.GetAsync("http://www.mock-url.com");

                stopwatch.Stop();
            }

            // Assert.

            stopwatch
                .Elapsed
                .Should().BeGreaterOrEqualTo(950.Milliseconds());

            response
                .Should().NotBeNull();

            var content = await response.Content.ReadAsStringAsync();

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

            var response = default(HttpResponseMessage);
            var stopwatch = new Stopwatch();

            // Act.

            using (var client = new HttpClient(throttlingHandler))
            {
                stopwatch.Start();

                response = await client.GetAsync("http://www.mock-url.com");
                response = await client.GetAsync("http://www.another-mock-url.com");

                stopwatch.Stop();
            }

            // Assert.

            stopwatch
                .Elapsed
                .Should().BeLessThan(1.Minutes());

            response
                .Should().NotBeNull();
        }
    }
}