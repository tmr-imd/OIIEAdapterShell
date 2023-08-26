namespace AdapterServer.Shared;

/// <summary>
/// This interface can be used to set the Nav Menu via dependency injection.
/// </summary>
public interface INavigationConfiguration
{
    public Type NavComponentType { get; }
}