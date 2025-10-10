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

public class RegisterViewModel : ObservableObject
{
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
    }

    private void Back()
    {
        LoginPage backToLoginPage = new LoginPage();
        backToLoginPage.Show();
        
    }

    private async void RegisterUser()
    {
        _client.Name = LoginBox;
        _client.Password = PasswordBox;
        _client.Command = "CREATE_CLIENT";
        _client.RootPath = LoginBox + "-root";
        string json = JsonSerializer.Serialize(_client);

        try
        {
            await SendToServerClientAsync(json);
            MessageBox.Show("Регистрация прошла успешно!");
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
            throw;
        }
    }

    private async Task SendToServerClientAsync(string json)
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
                }
            }
        }
        catch (Exception e)
        {
            MessageBox.Show($"Ошибка при отправке данных на сервер: {e.Message}");
        }
    }
}