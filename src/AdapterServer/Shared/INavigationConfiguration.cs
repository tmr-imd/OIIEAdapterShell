namespace AdapterServer.Shared;
public interface INavigationConfiguration
{
    /*
        This interface can be used to set the Nav Menu from different submodules.
    */
    public void setSelectedComponent(Type selectedTypeComponent);
    public Type? getSelectedComponent();

}