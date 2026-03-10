namespace WebApp.Services;

public class ProjectStateService
{
    public event Action? OnProjectsChanged;

    public void NotifyProjectsChanged()
    {
        OnProjectsChanged?.Invoke();
    }
}
