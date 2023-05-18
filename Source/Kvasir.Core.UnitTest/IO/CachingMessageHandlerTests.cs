// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CachingMessageHandlerTests.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, 17 November 2018 10:08:47 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.UnitTest;

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.AI.Kvasir;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Framework;
using nGratis.Cop.Olympus.Contract;
using nGratis.Cop.Olympus.Framework;
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

            using var client = new HttpClient(cachingHandler);

            // Act.

            var response = await client.GetAsync("http://www.mock-url.com/mock.html");

            // Assert.

            response
                .Should().NotBeNull();

            response
                .StatusCode
                .Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();

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

            using var client = new HttpClient(cachingHandler);

            // Act.

            var response = await client.GetAsync("http://www.mock-url.com/mock.html");

            // Assert.

            response
                .Should().NotBeNull();

            response
                .StatusCode
                .Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();

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

        [Fact]
        public async Task WhenGettingRequestWithCachedResponse_ShouldNotInvokeDelegatingHandler()
        {
            // Arrange.

            var stubHandler = StubHttpMessageHandler
                .Create()
                .WithSuccessfulResponse("http://www.mock-url.com/mock.html", "[_MOCK_HTML_CONTENT_]");

            var mockStorageManager = MockBuilder
                .CreateMock<IStorageManager>()
                .WithCompressedEntry(
                    new DataSpec("[_MOCK_CACHING_NAME_]", KvasirMime.Cache),
                    (new DataSpec("mock", Mime.Html), "[_MOCK_CACHED_HTML_CONTENT_]"));

            var mockKeyCalculator = MockBuilder
                .CreateMock<IKeyCalculator>()
                .WithMapping("http://www.mock-url.com/mock.html", "mock.html");

            var cachingHandler = new CachingMessageHandler(
                "[_MOCK_CACHING_NAME_]",
                mockStorageManager.Object,
                mockKeyCalculator.Object,
                stubHandler);

            using var client = new HttpClient(cachingHandler);

            // Act.

            var response = await client.GetAsync("http://www.mock-url.com/mock.html");

            // Assert.

            response
                .Should().NotBeNull();

            response
                .StatusCode
                .Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();

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

            using var client = new HttpClient(cachingHandler);

            // Act.

            var response = await client.GetAsync("http://www.mock-url.com/mock.html");

            // Assert.

            response
                .Should().NotBeNull();

            response
                .StatusCode
                .Should().Be(HttpStatusCode.NotFound);

            var content = await response.Content.ReadAsStringAsync();

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