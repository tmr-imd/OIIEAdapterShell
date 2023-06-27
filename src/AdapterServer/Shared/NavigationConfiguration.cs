namespace AdapterServer.Shared;

public class NavigationConfiguration : INavigationConfiguration
{
    public static Type? selectedComponent {get; set;}
    public Type? getSelectedComponent()
    {
        if (selectedComponent == null)
        {
            selectedComponent = typeof(NavMenu);
        }
        return selectedComponent;
    }

    public void setSelectedComponent(Type selectedTypeComponent)
    {
        selectedComponent = selectedTypeComponent;
    }
}