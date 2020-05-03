// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CachingMessageHandlerTests.cs" company="nGratis">
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
// <creation_timestamp>Saturday, 17 November 2018 10:08:47 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Test
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Moq.AI.Kvasir;
    using nGratis.AI.Kvasir.Testing;
    using nGratis.Cop.Olympus.Contract;
    using Xunit;
    using Arg = Moq.AI.Kvasir.Arg;
    using MockBuilder = Moq.AI.Kvasir.MockBuilder;

    public class CachingMessageHandlerTests
    {
        public class SendAsyncMethod
        {
            [Fact]
            public async Task WhenGettingRequestWithoutCachingBlob_ShouldInvokeDelegatingHandler()
            {
                // Arrange.

                var stubHandler = StubHttpMessageHandler
                    .Create()
                    .WithSuccessfulResponse("http://www.mock-url.com/mock.html", "[_MOCK_HTML_CONTENT_]");

                var mockStorageManager = MockBuilder
                    .CreateMock<IStorageManager>()
                    .WithSelfCaching();

                var mockKeyCalculator = MockBuilder
                    .CreateMock<IKeyCalculator>()
                    .WithMapping("http://www.mock-url.com/mock.html", "[_MOCK_KEY_]");

                var cachingHandler = new CachingMessageHandler(
                    "[_MOCK_CACHING_NAME_]",
                    mockStorageManager.Object,
                    mockKeyCalculator.Object,
                    stubHandler);

                using (var client = new HttpClient(cachingHandler))
                {
                    // Act.

                    var responseMessage = await client.GetAsync("http://www.mock-url.com/mock.html");

                    // Assert.

                    responseMessage
                        .Should().NotBeNull();

                    responseMessage
                        .StatusCode
                        .Should().Be(HttpStatusCode.OK);

                    var content = await responseMessage.Content.ReadAsStringAsync();

                    content
                        .Should().Be("[_MOCK_HTML_CONTENT_]");

                    stubHandler.VerifyInvoked("http://www.mock-url.com/mock.html", 1);

                    mockStorageManager.Verify(
                        mock => mock.LoadEntry(Arg.DataSpec.IsKvasirCaching("[_MOCK_CACHING_NAME_]")),
                        Times.Once);

                    mockStorageManager.Verify(
                        mock => mock.SaveEntry(
                            Arg.DataSpec.IsKvasirCaching("[_MOCK_CACHING_NAME_]"),
                            Arg.IsAny<System.IO.Stream>(),
                            Arg.IsAny<bool>()),
                        Times.Once);
                }
            }

            [Fact]
            public async Task WhenGettingRequestWithoutCachedResponse_ShouldInvokeDelegatingHandler()
            {
                // Arrange.

                var stubHandler = StubHttpMessageHandler
                    .Create()
                    .WithSuccessfulResponse("http://www.mock-url.com/mock.html", "[_MOCK_HTML_CONTENT_]");

                var mockStorageManager = MockBuilder
                    .CreateMock<IStorageManager>()
                    .WithEmptyCaching("[_MOCK_CACHING_NAME_]");

                var mockKeyCalculator = MockBuilder
                    .CreateMock<IKeyCalculator>()
                    .WithMapping("http://www.mock-url.com/mock.html", "[_MOCK_KEY_]");

                var cachingHandler = new CachingMessageHandler(
                    "[_MOCK_CACHING_NAME_]",
                    mockStorageManager.Object,
                    mockKeyCalculator.Object,
                    stubHandler);

                using (var client = new HttpClient(cachingHandler))
                {
                    // Act.

                    var responseMessage = await client.GetAsync("http://www.mock-url.com/mock.html");

                    // Assert.

                    responseMessage
                        .Should().NotBeNull();

                    responseMessage
                        .StatusCode
                        .Should().Be(HttpStatusCode.OK);

                    var content = await responseMessage.Content.ReadAsStringAsync();

                    content
                        .Should().Be("[_MOCK_HTML_CONTENT_]");

                    stubHandler.VerifyInvoked("http://www.mock-url.com/mock.html", 1);

                    mockStorageManager.Verify(
                        mock => mock.LoadEntry(Arg.DataSpec.IsKvasirCaching("[_MOCK_CACHING_NAME_]")),
                        Times.Once);

                    mockStorageManager.Verify(
                        mock => mock.SaveEntry(
                            Arg.DataSpec.IsKvasirCaching("[_MOCK_CACHING_NAME_]"),
                            Arg.IsAny<System.IO.Stream>(),
                            Arg.IsAny<bool>()),
                        Times.Never);
                }
            }

            [Fact]
            public async Task WhenGettingRequestWithCachedResponse_ShouldNotInvokeDelegatingHandler()
            {
                // Arrange.

                var stubHandler = StubHttpMessageHandler
                    .Create()
                    .WithSuccessfulResponse("http://www.mock-url.com/mock.html", "[_MOCK_HTML_CONTENT_]");

                var mockStorageManager = MockBuilder
                    .CreateMock<IStorageManager>()
                    .WithCaching("[_MOCK_CACHING_NAME_]", "mock.html", "[_MOCK_CACHED_HTML_CONTENT_]");

                var mockKeyCalculator = MockBuilder
                    .CreateMock<IKeyCalculator>()
                    .WithMapping("http://www.mock-url.com/mock.html", "mock.html");

                var cachingHandler = new CachingMessageHandler(
                    "[_MOCK_CACHING_NAME_]",
                    mockStorageManager.Object,
                    mockKeyCalculator.Object,
                    stubHandler);

                using (var client = new HttpClient(cachingHandler))
                {
                    // Act.

                    var responseMessage = await client.GetAsync("http://www.mock-url.com/mock.html");

                    // Assert.

                    responseMessage
                        .Should().NotBeNull();

                    responseMessage
                        .StatusCode
                        .Should().Be(HttpStatusCode.OK);

                    var content = await responseMessage.Content.ReadAsStringAsync();

                    content
                        .Should().Be("[_MOCK_CACHED_HTML_CONTENT_]");

                    stubHandler.VerifyInvoked("http://www.mock-url.com/mock.html", 0);

                    mockStorageManager.Verify(
                        mock => mock.LoadEntry(Arg.DataSpec.IsKvasirCaching("[_MOCK_CACHING_NAME_]")),
                        Times.Once);

                    mockStorageManager.Verify(
                        mock => mock.SaveEntry(
                            Arg.DataSpec.IsKvasirCaching("[_MOCK_CACHING_NAME_]"),
                            Arg.IsAny<System.IO.Stream>(),
                            Arg.IsAny<bool>()),
                        Times.Never);
                }
            }

            [Fact]
            public async Task WhenGettingRequestWithUnsuccessfulResponse_ShouldNotCacheAnything()
            {
                var stubHandler = StubHttpMessageHandler
                    .Create()
                    .WithResponse("http://www.mock-url.com/mock.html", HttpStatusCode.NotFound, "[_MOCK_ERROR_CONTENT_]");

                var mockStorageManager = MockBuilder
                    .CreateMock<IStorageManager>()
                    .WithEmptyCaching("[_MOCK_CACHING_NAME_]");

                var mockKeyCalculator = MockBuilder
                    .CreateMock<IKeyCalculator>()
                    .WithMapping("http://www.mock-url.com/mock.html", "[_MOCK_KEY_]");

                var cachingHandler = new CachingMessageHandler(
                    "[_MOCK_CACHING_NAME_]",
                    mockStorageManager.Object,
                    mockKeyCalculator.Object,
                    stubHandler);

                using (var client = new HttpClient(cachingHandler))
                {
                    // Act.

                    var responseMessage = await client.GetAsync("http://www.mock-url.com/mock.html");

                    // Assert.

                    responseMessage
                        .Should().NotBeNull();

                    responseMessage
                        .StatusCode
                        .Should().Be(HttpStatusCode.NotFound);

                    var content = await responseMessage.Content.ReadAsStringAsync();

                    content
                        .Should().Be("[_MOCK_ERROR_CONTENT_]");

                    stubHandler.VerifyInvoked("http://www.mock-url.com/mock.html", 1);

                    mockStorageManager.Verify(
                        mock => mock.LoadEntry(Arg.DataSpec.IsKvasirCaching("[_MOCK_CACHING_NAME_]")),
                        Times.Once);

                    mockStorageManager.Verify(
                        mock => mock.SaveEntry(
                            Arg.DataSpec.IsKvasirCaching("[_MOCK_CACHING_NAME_]"),
                            Arg.IsAny<System.IO.Stream>(),
                            Arg.IsAny<bool>()),
                        Times.Never);
                }
            }
        }
    }
}