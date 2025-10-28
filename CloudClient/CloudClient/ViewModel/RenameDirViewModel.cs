using System.Collections.ObjectModel;
using System.Windows;
using CloudClient.Model;
using CloudClient.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CloudClient.ViewModel;

public class RenameDirViewModel : ObservableObject
{
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
    public RenameDirViewModel(ObservableCollection<string> roots, Client currentClient)
    {
        Roots = roots;
        _currentClient = currentClient;
        RenameDirCommand = new AsyncRelayCommand(RenameDir);
        _authService = new AuthService("192.168.0.100", 9999);
    }

    private async Task RenameDir()
    {
        var cmd = new Command
        {
            CommandName = "RENAME_DIRECTORY",
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
        }

        if (resp != null  && resp.Status == "ERROR")
        {
            MessageBox.Show(resp.Message);
        }
    }

    public IAsyncRelayCommand RenameDirCommand { get; }
    
}