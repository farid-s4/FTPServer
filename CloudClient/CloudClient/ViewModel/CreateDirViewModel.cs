using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using CloudClient.Model;
using CloudClient.Services;
using CloudClient.View;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CloudClient.ViewModel;

public class CreateDirViewModel : ObservableObject
{
    public static event Action? OnFolderCreated;
    public event Action? RequestClose;
    private AuthService _authService;
    private Client _currentClient;
    private string _dirName = null!;
    
    private ObservableCollection<string> _roots = null!;
    public ObservableCollection<string> Roots
    {
        get => _roots;
        set => SetProperty(ref _roots, value);
    }
    private string _selectedRoot = null!;

    public string SelectedRoot
    {
        get => _selectedRoot;
        set => SetProperty(ref _selectedRoot, value);
    }

    public string DirName
    {
        get => _dirName;
        set => SetProperty(ref _dirName, value);
    }
    private string _dirPath = null!;
    public CreateDirViewModel(ObservableCollection<string> roots, Client currentClient)
    {
        Roots = roots;
        _currentClient = currentClient;
        CreateDirCommand = new AsyncRelayCommand(CreateDir);
        _authService = new AuthService("192.168.0.100", 9999);
    }
    
    public IAsyncRelayCommand CreateDirCommand { get; }

    private async Task CreateDir()
    {
        var cmd = new Command
        {
            CommandName = "CREATE_DIRECTORY",
            Args = new Dictionary<string, string>
            {
                { "username", _currentClient.Name },
                { "password", _currentClient.Password },
                { "selectedRoot", SelectedRoot },
                { "newRootName", DirName }
            }
        };

        await _authService.SendMessageAsync(cmd);
        var resp = await _authService.GetMessageAsync<string>();
        
        if (resp != null && resp.Status == "OK")
        {
            MessageBox.Show(resp.Message);
            OnFolderCreated?.Invoke();
            RequestClose?.Invoke();
        }

        if (resp != null  && resp.Status == "ERROR")
        {
            MessageBox.Show(resp.Message);
        }
    }
}