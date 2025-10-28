using CommunityToolkit.Mvvm.ComponentModel;

namespace CloudClient.Model;

public class ChangeRoot :  ObservableObject
{
    private string _name;

    public string Name
    {
        get => _name; 
        set => SetProperty(ref _name, value);
    }
}