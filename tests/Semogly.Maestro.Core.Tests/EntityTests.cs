using FluentAssertions;
using Semogly.Maestro.Core.Entities;

namespace Semogly.Maestro.Core.Tests;

public class EntityTests
{
    [Fact]
    public void NewEntity_HasNonEmptyId()
    {
        var processFlow = new ProcessFlow { Name = "Nightly" };

        processFlow.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void TwoNewEntities_HaveDifferentIds()
    {
        var first = new ProcessFlow { Name = "Nightly" };
        var second = new ProcessFlow { Name = "Nightly" };

        first.Id.Should().NotBe(second.Id);
    }
}
