using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using CloudClient.Model;
using CloudClient.Services;
using CloudClient.View;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace CloudClient.ViewModel;
public class LoginViewModel : ObservableObject
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
    
    public ICommand LoginCommand {get;}
    public ICommand RegisterCommand {get;}

    public LoginViewModel()
    {
        LoginCommand = new RelayCommand(Login);
        RegisterCommand = new RelayCommand(Register);
        _authService = new AuthService("192.168.0.100", 9999);
    }

    private void Register()
    {
        RegisterPage registerWindow = new RegisterPage();
        registerWindow.Show();

        Application.Current.MainWindow = registerWindow;
        foreach (Window window in Application.Current.Windows)
        {
            if (window != registerWindow)
            {
                window.Close();
                break;
            }
        }
    }

    private async void Login()
    {
        string loginBox =  LoginBox;
        string passwordBox = PasswordBox;
        var cmd = new Command
        {
            CommandName = "LOGIN_USER",
            Args = new Dictionary<string, string>
            {
                { "username", loginBox },
                { "password", passwordBox }
            }
        };
        
        await _authService.SendMessageAsync(cmd);
        
        var resp = await _authService.GetMessageAsync<Client>();
        
        if (resp != null && resp.Status == "LOGGED")
        {
            MessageBox.Show(resp.Message);
            var client = new ClientMainPaige(resp.Data);
            client.Show();
            
            Application.Current.MainWindow = client;
            foreach (Window window in Application.Current.Windows)
            {
                if (window != client)
                {
                    window.Close();
                    break;
                }
            }
        }

        if (resp != null  && resp.Status == "ERROR")
        {
            MessageBox.Show(resp.Message);
        }
    }
    
}