using System.Net.Sockets;
using System.Text.Json;
using CloudServer.Model;

namespace CloudServer.Services;

public static class ClientsServices
{
    private const string FileName = "clients.txt";
    public static List<Client> LoadClients()
    {

        if (!File.Exists(FileName))
        {
            File.WriteAllText(FileName, "[]");
            return new List<Client>();
        }
        
        return JsonSerializer.Deserialize<List<Client>>(File.ReadAllText(FileName)) ?? throw new InvalidOperationException();
    }

    public static void SaveClients(List<Client> clients)
    {

        if (!File.Exists(FileName))
        {
            File.WriteAllText(FileName, "[]");
        }
        
        string json = JsonSerializer.Serialize(clients) ?? throw new InvalidOperationException();;
        File.WriteAllText(FileName, json);
    }
}