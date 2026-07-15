using System.Net;
using FluentAssertions;
using Semogly.Maestro.Abstractions.Activities;

namespace Semogly.Maestro.Activities.Http.Tests;

public class HttpRequestActivityHandlerTests
{
    private static ActivityExecutionContext Context(params (string Key, string? Value)[] parameters) =>
        new()
        {
            ActivityExecutionId = Guid.NewGuid(),
            ProcessActivityId = Guid.NewGuid(),
            Parameters = parameters.ToDictionary(p => p.Key, p => p.Value),
        };

    [Fact]
    public async Task SuccessfulResponse_ReturnsBodyAsOutput()
    {
        var fakeHandler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("pong") });
        var handler = new HttpRequestActivityHandler(new HttpClient(fakeHandler));

        var output = await handler.ExecuteAsync(Context(("Url", "https://example.test/ping")), CancellationToken.None);

        output.Should().Be("pong");
    }

    [Fact]
    public async Task DefaultsToGet_WhenMethodNotProvided()
    {
        var fakeHandler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var handler = new HttpRequestActivityHandler(new HttpClient(fakeHandler));

        await handler.ExecuteAsync(Context(("Url", "https://example.test/ping")), CancellationToken.None);

        fakeHandler.LastRequest!.Method.Should().Be(HttpMethod.Get);
    }

    [Fact]
    public async Task SendsBody_WithContentType()
    {
        var fakeHandler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var handler = new HttpRequestActivityHandler(new HttpClient(fakeHandler));

        await handler.ExecuteAsync(
            Context(("Url", "https://example.test/items"), ("Method", "POST"), ("Body", "{\"x\":1}"), ("ContentType", "application/json")),
            CancellationToken.None);

        fakeHandler.LastRequest!.Method.Should().Be(HttpMethod.Post);
        fakeHandler.LastRequestBody.Should().Be("{\"x\":1}");
        fakeHandler.LastRequest.Content!.Headers.ContentType!.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task ParsesHeaders_OnePerLine()
    {
        var fakeHandler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var handler = new HttpRequestActivityHandler(new HttpClient(fakeHandler));

        await handler.ExecuteAsync(
            Context(("Url", "https://example.test/ping"), ("Headers", "Authorization: Bearer token\nX-Custom: value")),
            CancellationToken.None);

        fakeHandler.LastRequest!.Headers.GetValues("Authorization").Should().ContainSingle().Which.Should().Be("Bearer token");
        fakeHandler.LastRequest.Headers.GetValues("X-Custom").Should().ContainSingle().Which.Should().Be("value");
    }

    [Fact]
    public async Task NonSuccessStatus_ThrowsWithStatusAndBody()
    {
        var fakeHandler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("not found here") });
        var handler = new HttpRequestActivityHandler(new HttpClient(fakeHandler));

        var act = () => handler.ExecuteAsync(Context(("Url", "https://example.test/missing")), CancellationToken.None);

        var assertion = await act.Should().ThrowAsync<HttpRequestException>();
        assertion.Which.Message.Should().Contain("404").And.Contain("not found here");
    }
}
