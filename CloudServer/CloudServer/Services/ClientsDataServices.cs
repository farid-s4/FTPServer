using System.Text.Json;
using CloudServer.Model;

namespace CloudServer.Services;

public static class ClientsDataServices
{
    private const string FileName = "clients.json";
    public static async Task<List<Client>> LoadClientsAsync()
    {
        try
        {
            if (!File.Exists(FileName))
            {
                await File.WriteAllTextAsync(FileName, "[]");
            }
        
            return JsonSerializer.Deserialize<List<Client>>(await File.ReadAllTextAsync(FileName)) ?? throw new InvalidOperationException();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    public static async Task SaveClientsAsync(List<Client> clients)
    {
        try
        {
            if (!File.Exists(FileName))
            {
                await File.WriteAllTextAsync(FileName, "[]");
            }
        
            string json = JsonSerializer.Serialize(clients) ?? throw new InvalidOperationException();
            await File.WriteAllTextAsync(FileName, json);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}