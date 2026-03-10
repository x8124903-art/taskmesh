using WebApp.Services;

namespace WebApp.Tests.Services;

public sealed class ProjectStateServiceShould
{
    [Fact]
    public void NotifyProjectsChanged_InvokesSubscribers()
    {
        var service = new ProjectStateService();
        var invoked = false;
        service.OnProjectsChanged += () => invoked = true;

        service.NotifyProjectsChanged();

        invoked.Should().BeTrue();
    }

    [Fact]
    public void NotifyProjectsChanged_DoesNotThrow_WhenNoSubscribers()
    {
        var service = new ProjectStateService();

        var act = () => service.NotifyProjectsChanged();

        act.Should().NotThrow();
    }

    [Fact]
    public void NotifyProjectsChanged_InvokesMultipleSubscribers()
    {
        var service = new ProjectStateService();
        var count = 0;
        service.OnProjectsChanged += () => count++;
        service.OnProjectsChanged += () => count++;

        service.NotifyProjectsChanged();

        count.Should().Be(2);
    }

    [Fact]
    public void OnProjectsChanged_IsNullByDefault()
    {
        var service = new ProjectStateService();

        service.NotifyProjectsChanged();
    }
}
