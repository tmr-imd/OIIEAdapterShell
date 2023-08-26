namespace AdapterServer.Shared;

public class NavigationConfiguration : INavigationConfiguration
{
    public virtual Type NavComponentType => typeof(NavMenu);
}