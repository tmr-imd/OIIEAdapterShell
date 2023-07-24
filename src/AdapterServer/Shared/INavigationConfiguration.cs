namespace AdapterServer.Shared;
public interface INavigationConfiguration
{
    /*
        This interface can be used to set the Nav Menu from different submodules.
    */
    public static Type? selectedComponent {get; set;}

}