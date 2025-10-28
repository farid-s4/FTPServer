using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CloudClient.Model;
using CloudClient.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace CloudClient.ViewModel;

public class UploadFileViewModel : ObservableObject
{
    private AuthService _authService;
    private Client _currentClient;
    private string _dirName = null!;
    private FileInfo? _fileInfo = null;
    
    
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
    
    private string _dirPath = null!;
    public UploadFileViewModel(ObservableCollection<string> roots, Client currentClient, AuthService authService)
    {
        Roots = roots;
        _currentClient = currentClient;
        UploadFileCommand = new AsyncRelayCommand(UploadFile);
        _authService = authService;
        OpenFileDialog ofd = new()
        {
            Title = "Выберите файл"
        };

        if (ofd.ShowDialog() == true)
        {
            _fileInfo = new FileInfo(ofd.FileName);
        }
    }

    private async Task UploadFile()
    {
        if (_fileInfo != null)
        {
            var cmd = new Command()
            {
                CommandName = "UPLOAD_FILE",
                Args = new Dictionary<string, string>
                {
                    { "username", _currentClient.Name },
                    { "password", _currentClient.Password },
                    { "fileName", _fileInfo.Name },
                    { "selectedRoot", SelectedRoot },
                    { "fileSize", _fileInfo.Length.ToString() }
                }
            };
            
            await _authService.SendMessageAsync(cmd);

            var b = await _authService.SendFileAsync(_fileInfo.FullName);
                
            var recv = await _authService.GetMessageAsync<string>();
            
            if (recv != null && b && recv.Status == "OK")
            {
                MessageBox.Show(recv.Message);
            }
            
        }
    }
    
    public IAsyncRelayCommand UploadFileCommand { get; }
}