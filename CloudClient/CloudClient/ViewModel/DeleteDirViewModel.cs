using System.Collections.ObjectModel;
using System.Windows;
using CloudClient.Model;
using CloudClient.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CloudClient.ViewModel;

public class DeleteDirViewModel : ObservableObject
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
    public DeleteDirViewModel(ObservableCollection<string> roots, Client currentClient)
    {
        Roots = roots;
        _currentClient = currentClient;
        DeleteDirCommand = new AsyncRelayCommand(DeleteDir);
        _authService = new AuthService("192.168.0.100", 9999);
    }

    private async Task DeleteDir()
    {
        var cmd = new Command
        {
            CommandName = "DELETE_DIRECTORY",
            Args = new Dictionary<string, string>
            {
                { "username", _currentClient.Name },
                { "password", _currentClient.Password },
                { "selectedRoot", SelectedRoot },
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

    public AsyncRelayCommand DeleteDirCommand { get; }
}