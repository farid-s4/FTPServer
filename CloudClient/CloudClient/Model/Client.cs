using System.Net.Sockets;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CloudClient.Model;

public class Client :  ObservableObject
{
    private string _name;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    private string _password;

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }
    private string _rootPath;
    public string RootPath
    {
        get => _rootPath;
        set => SetProperty(ref _rootPath, value);
    }
    
    public string Command { get; set; }

}