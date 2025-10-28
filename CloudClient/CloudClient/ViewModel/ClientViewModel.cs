using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using CloudClient.Model;
using CloudClient.Services;
using CloudClient.View;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace CloudClient.ViewModel;

public class ClientViewModel : ObservableObject
{
    AuthService _authService;
    private Client? _currentClient;
    private FileDirectory? _rootDirectory;
    
    private Stack<string> _directoryStack = new();
    
    private ObservableCollection<FileDirectory> _currentItems;
    public ObservableCollection<FileDirectory> CurrentItems
    {
        get => _currentItems;
        set => SetProperty(ref _currentItems, value);
    }
    private FileDirectory _currentSelectedItem = new();

    public FileDirectory CurrentSelectedItem
    {
        get => _currentSelectedItem;
        set => SetProperty(ref _currentSelectedItem, value);
    }
    
    
    public ClientViewModel(Client loggedInClient)
    {
        _authService = new AuthService("192.168.0.100", 9999);
        _currentClient = loggedInClient;
        _currentItems = new ObservableCollection<FileDirectory>();
        CurrentSelectedItem = new FileDirectory();
        
        LogoutCommand = new RelayCommand(Logout);
        OpenSelectedDirCommand = new AsyncRelayCommand(OpenSelectedDir);
        GoBackCommand = new RelayCommand(GoBack);
        CreateDirectoryCommand = new AsyncRelayCommand(CreateDirectory);
        DeleteDirectoryCommand = new AsyncRelayCommand(DeleteDirectory);
        RenameDirectoryCommand = new AsyncRelayCommand(RenameDirectory);
        UploadFileCommand = new AsyncRelayCommand(UploadFile);
        DownloadFileCommand = new AsyncRelayCommand(DownloadFile);
        
        _ = LoadDirs();
    }

    private async Task DownloadFile()
    {
        try
        {
            if (_currentClient != null)
            {
                string? savePath = null;
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Title = "Сохранить файл как:",
                    FileName = CurrentSelectedItem.Name,
                    DefaultExt = Path.GetExtension(CurrentSelectedItem.Name), 
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) 
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    savePath = saveFileDialog.FileName;
                }
                else
                {
                    return;
                }

                long fileSize = 0;
                var cmd = new Command
                {
                    CommandName = "DOWNLOAD_FILE",
                    Args = new Dictionary<string, string>
                    {
                        { "username", _currentClient.Name },
                        { "password", _currentClient.Password },
                        { "fileName", CurrentSelectedItem.Name },
                    }
                };

                await _authService.SendMessageAsync(cmd);

                fileSize = await _authService.ReadFileSizeAsync();

                var b = savePath != null && await _authService.ReceiveFileAsync(savePath, fileSize);

                var recv = await _authService.GetMessageAsync<string>();
                if (recv != null && b && recv.Status == "OK")
                {
                    MessageBox.Show(recv.Message);
                }
            }
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
        }

    }

    private async Task UploadFile()
    {
        var data = await RefreshedData();
        var result = new ObservableCollection<string>();
        var dirs = CollectAllDirectories(data, result);
        if (_currentClient != null && dirs != null)
        {
            var dirPage = new UploadFilePage(dirs, _currentClient, _authService);
            dirPage.Closed += async (sender, args) => await LoadDirs();
            dirPage.Show();
        }
        
    }

    private async Task RenameDirectory()
    {
        var data = await RefreshedData();
        var result = new ObservableCollection<string>();
        var dirs = CollectAllDirectoriesWithOutRoot(data, result);
        if (_currentClient != null && dirs != null)
        {
            var dirPage = new RenameDirPage(dirs, _currentClient);
            dirPage.Closed += async (sender, args) => await LoadDirs();
            dirPage.Show();
        }
    }

    private async Task DeleteDirectory()
    {
        var data = await RefreshedData();
        var result = new ObservableCollection<string>();
        var dirs = CollectAllDirectoriesWithOutRoot(data, result);
        
        if (_currentClient != null && dirs != null)
        {
            var dirPage = new DeleteDirPage(dirs, _currentClient);
            dirPage.Closed += async (sender, args) => await LoadDirs();
            dirPage.Show();
        }
    }

    private async Task CreateDirectory()
    {
        var data = await RefreshedData();
        var result = new ObservableCollection<string>();
        var dirs = CollectAllDirectories(data, result);

        if (_currentClient != null && dirs != null)
        {
            var dirPage = new CreateDirPage(dirs, _currentClient);
            dirPage.Closed += async (sender, args) => await LoadDirs();
            dirPage.Show();
        }
    }

    private ObservableCollection<string>? CollectAllDirectories(FileDirectory directory, ObservableCollection<string> result)
    {

        if (directory.IsDirectory)
        {
            result.Add(directory.Name);
        }

        if (directory.Children.Count != 0)
        {
            foreach (var child in directory.Children)
            {
                if (child.IsDirectory)
                {
                    CollectAllDirectories(child,  result);
                }
            }
        }
        return result;
    }
    private ObservableCollection<string>? CollectAllDirectoriesWithOutRoot(FileDirectory directory, ObservableCollection<string> result)
    {
        

        if (directory.Children.Count != 0)
        {
            foreach (var child in directory.Children)
            {
                if (child.IsDirectory)
                {
                    CollectAllDirectories(child,  result);
                }
            }
        }
        return result;
    }

    private void GoBack()
    {
        if (_directoryStack.Count <= 1)
        {
            return;
        }
        
        _directoryStack.Pop();
        
        string previousPath = _directoryStack.Peek();
        
        FileDirectory? dirNode = FindNode(_rootDirectory!, previousPath);
        if (dirNode == null)
        {
            MessageBox.Show("Директория не найдена.");
            return;
        }
        
        _currentItems.Clear();
        if (dirNode.Children != null)
        {
            foreach (var child in dirNode.Children)
            {
                _currentItems.Add(child);
            }
        }
    }

    private async Task LoadDirs()
    {
        CurrentItems.Clear();
        _rootDirectory = null;
        _directoryStack.Clear(); 
        
        var f = await RefreshedData();

        _rootDirectory = f;
        _directoryStack.Push(f.FullPath); 
        
        if (f.Children.Count > 0)
        {
            foreach (var child in f.Children)
            {
                CurrentItems.Add(child);
            }
        }
        else
        {
            CurrentItems.Add(f);
        }
    }

    private Task OpenSelectedDir()
    {
        FileDirectory? dirNode = FindNode(_rootDirectory!, CurrentSelectedItem.FullPath);
        if (dirNode == null)
        {
            MessageBox.Show("Директория не найдена.");
            return Task.CompletedTask;
        }

        if (dirNode.IsDirectory == false)
        {
            MessageBox.Show("Это не директория");
            return Task.CompletedTask;
        }
        
        _directoryStack.Push(dirNode.FullPath);
        
        _currentItems.Clear();
        foreach (var child in dirNode.Children)
        {
            _currentItems.Add(child);
        }

        return Task.CompletedTask;
    }

    private FileDirectory? FindNode(FileDirectory node, string fullPath)
    {
        if (node.FullPath == fullPath)
            return node;

        if (node.Children != null)
        {
            foreach (var child in node.Children)
            {
                var found = FindNode(child, fullPath);
                if (found != null)
                    return found;
            }
        }

        return null;
    }


    private void Logout()
    {
        var loginWindow = new LoginPage();
        loginWindow.Show();

        foreach (Window window in Application.Current.Windows)
        {
            if (window.DataContext == this)
            {
                window.Close();
                break;
            }
        }
    }

    private async Task<FileDirectory> RefreshedData()
    {
        if (_currentClient != null)
        {
            var cmd = new Command
            {
                CommandName = "GET_DIRS",
                Args = new Dictionary<string, string>
                {
                    { "username", _currentClient.Name },
                    { "password", _currentClient.Password }
                }
            };

            await _authService.SendMessageAsync(cmd);

            var response = await _authService.GetMessageAsync<FileDirectory>();

            if (response != null && response.Status == "OK" && response.Data != null)
            {
                return response.Data;
            }
        }
        return null!;
    }

    public ICommand LogoutCommand { get; }
    public IAsyncRelayCommand OpenSelectedDirCommand { get; }
    public ICommand GoBackCommand { get; }
    public IAsyncRelayCommand CreateDirectoryCommand { get; }
    public IAsyncRelayCommand DeleteDirectoryCommand { get; }
    public IAsyncRelayCommand RenameDirectoryCommand { get; }
    public IAsyncRelayCommand UploadFileCommand { get; }
    public IAsyncRelayCommand DownloadFileCommand { get; }
    
}