using FluentAssertions;

namespace Semogly.Maestro.Activities.Http.Tests;

public class HttpActivityDefinitionsTests
{
    [Fact]
    public void Request_HasExpectedParams()
    {
        var activity = HttpActivityDefinitions.Request();

        activity.Type.Should().Be(HttpActivityTypes.Request);
        activity.Params.Select(p => p.Key).Should().BeEquivalentTo(["Url", "Method", "Body", "ContentType", "Headers"]);
        activity.Params.Single(p => p.Key == "Url").Required.Should().BeTrue();
        activity.Params.Single(p => p.Key == "Method").DefaultValue.Should().Be("GET");
    }
}
