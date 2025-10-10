using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using static CloudServer.Services.ClientsServices;
using CloudServer.Model;
namespace CloudServer;

public class TcpServer
{
    private TcpListener _listener;
    private List<Client> _clients; 
    private CancellationTokenSource _cts  = new CancellationTokenSource();
    
    public event Action? OnClientConnected;
    public event Action? OnClientDisconnected;
    public event Action? OnStartedListening;
    public event Action? OnClientRegistered;
    public event Action? OnClientLoggedIn;

    public TcpServer(string ip, int port)
    {
        _clients = LoadClients();
        _listener = new TcpListener(IPAddress.Parse(ip), port);
    }

    public Task Start()
    {
        _listener.Start();

        return Task.Factory.StartNew(() =>
        {
            OnStartedListening?.Invoke();

            while (!_cts.IsCancellationRequested)
            {
                var client = _listener.AcceptTcpClient();
                using var stream = client.GetStream();
                
                byte[] buffer = new byte[client.ReceiveBufferSize];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                
                Client? receivedClient = JsonSerializer.Deserialize<Client>(json);

                if (receivedClient != null)
                {
                    _clients.Add(receivedClient);
                    OnClientConnected?.Invoke();

                    switch (receivedClient.Command)
                    {
                        case "CREATE_CLIENT":
                        {
                            Directory.CreateDirectory(receivedClient.RootPath);
                            SaveClients(_clients);
                            OnClientRegistered?.Invoke();
                            break;
                        }
                        case "CHECK_LOGIN":
                        {
                            
                            foreach (var c in _clients)
                            {
                                if (c.Name == receivedClient.Name && c.Password == receivedClient.Password)
                                {
                                    string currentClientJson = JsonSerializer.Serialize(c);
                                    
                                    _ = SendResponse(stream, currentClientJson);
                                    OnClientLoggedIn?.Invoke();
                                }
                                else
                                {
                                    _ = SendResponse(stream, "Invalid login or password");
                                }
                            }
                            break;
                        }
                    }
                }
            }
            
        });
    }

    private async Task SendResponse(NetworkStream stream, string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        await stream.WriteAsync(data, 0, data.Length);
        await stream.FlushAsync();
    }

    public void Stop()
    {
        _cts.Cancel();
    }

    public void Dispose()
    {
        _listener.Dispose();
        _cts.Dispose();
    }
}


