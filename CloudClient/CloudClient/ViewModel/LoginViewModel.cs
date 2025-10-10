using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using CloudClient.Model;
using CloudClient.View;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CloudClient.ViewModel;
public class LoginViewModel : ObservableObject
{
    private Client _client = new Client(); 
    
    private string _loginBox;

    public string LoginBox
    {
        get { return _loginBox; }
        set { SetProperty(ref _loginBox, value); }
    }
    private string _passwordBox;
    public string PasswordBox
    {
        get { return _passwordBox; }
        set { SetProperty(ref _passwordBox, value); }
    }
    
    public ICommand LoginCommand {get;}
    public ICommand RegisterCommand {get;}

    public LoginViewModel()
    {
        LoginCommand = new RelayCommand(Login);
        RegisterCommand = new RelayCommand(Register);
    }

    private void Register()
    {
        RegisterPage registerWindow = new RegisterPage();
        registerWindow.Show();

        if (Application.Current.MainWindow != null) Application.Current.MainWindow.Close();
    }

    private async void Login()
    {
        _client.Name = LoginBox;
        _client.Password = PasswordBox;
        _client.Command = "CHECK_LOGIN";
        
        string json = JsonSerializer.Serialize(_client);
        
        try
        {
            string response = await SendRequestForLogin(json);

            if (response == "Invalid login or password")
            {
                MessageBox.Show("Invalid login or password");
            }
            else
            {
                Client? loggedInClient = JsonSerializer.Deserialize<Client>(response);
                if (loggedInClient != null)
                {
                    MessageBox.Show($"Добро пожаловать, {loggedInClient.Name}!");

                    var clientPage = new ClientMainPaige();
                    clientPage.Show();
                    if (Application.Current.MainWindow != null) Application.Current.MainWindow.Close();
                }
            }
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
            throw;
        }
        
    }

    private async Task<string> SendRequestForLogin(string json)
    {
        try
        {
            using (TcpClient client = new TcpClient())
            {
                client.Connect("192.168.0.101", 9999);

                using (NetworkStream stream = client.GetStream())
                {
                    byte[] data = Encoding.UTF8.GetBytes(json);
                    await stream.WriteAsync(data, 0, data.Length);
                    await stream.FlushAsync();
                    
                    byte[] buffer = new byte[4096];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    return response;
                }
            }
        }
        catch (Exception e)
        {
            MessageBox.Show($"Ошибка при отправке данных на сервер: {e.Message}");
            return "ERROR";
        }
    }
}