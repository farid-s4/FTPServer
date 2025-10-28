using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using CloudClient.Model;
using CloudClient.Services;
using CloudClient.View;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CloudClient.ViewModel;

public class RegisterViewModel : ObservableObject
{
    private AuthService _authService;
    private Client _client = new Client(); 
    
    private string _loginBox;
    public string LoginBox
    {
        get => _loginBox;
        set => SetProperty(ref _loginBox, value);
    }
    private string _passwordBox;
    public string PasswordBox
    {
        get => _passwordBox;
        set => SetProperty(ref _passwordBox, value);
    }
    
    public ICommand RegisterCommand { get; set; }
    public ICommand BackCommand { get; set; }
    public RegisterViewModel()
    {
        RegisterCommand = new RelayCommand(RegisterUser);
        BackCommand = new RelayCommand(Back);
        _authService = new AuthService("192.168.0.100", 9999);
    }

    private void Back()
    {
        LoginPage backToLoginPage = new LoginPage();
        backToLoginPage.Show();
        Application.Current.MainWindow = backToLoginPage;
        foreach (Window window in Application.Current.Windows)
        {
            if (window != backToLoginPage)
            {
                window.Close();
                break;
            }
        }
    }

    private async void RegisterUser()
    {
        string loginBox = LoginBox;
        string passwordBox = PasswordBox;
        var cmd = new Command
        {
            CommandName = "REGISTER_USER",
            Args = new Dictionary<string, string>
            {
                { "username", loginBox },
                { "password", passwordBox }
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
    
}