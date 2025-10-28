using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using CloudClient.Model;

namespace CloudClient.Services;

public class AuthService 
{
    private readonly TcpClient _client = new TcpClient();
    NetworkStream _stream;
    StreamReader _reader;
    StreamWriter _writer;
    
    public AuthService(string ip, int port)
    {
        _client.Connect(ip, port);
        _stream = _client.GetStream();
        _reader = new StreamReader(_stream,  Encoding.UTF8);
        _writer = new StreamWriter(_stream, Encoding.UTF8);
    }

    public async Task SendMessageAsync(Command cmd)
    {
        string json = JsonSerializer.Serialize(cmd);
        await _writer.WriteLineAsync(json);
        await _writer.FlushAsync();
    }

    public async Task<Response<T>?> GetMessageAsync<T>() where T : class
    {
        string? response = await _reader.ReadLineAsync();

        if (response != null)
        {
            return JsonSerializer.Deserialize<Response<T>>(response);
        }
        return null;
    }
    
    public async Task<bool> SendFileAsync(string filePath)
    {
        try
        {
            using (FileStream fileStream = File.OpenRead(filePath))
            {
                byte[] fileSizeBytes = BitConverter.GetBytes(fileStream.Length);
                await _stream.WriteAsync(fileSizeBytes, 0, fileSizeBytes.Length);
                
                byte[] buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await _stream.WriteAsync(buffer, 0, bytesRead);
                }
                return true;
            }
        }
        catch (Exception ex)
        {
            return false;
        }
    }
    
    public async Task<long> ReadFileSizeAsync()
    {
        byte[] sizeBuffer = new byte[8];
        int bytesRead = await _stream.ReadAsync(sizeBuffer, 0, 8);
    
        if (bytesRead != 8)
            throw new Exception("Не удалось прочитать размер файла");
    
        return BitConverter.ToInt64(sizeBuffer, 0);
    }
    
    public async Task<bool> ReceiveFileAsync(string savePath, long fileSize)
    {
        try
        {
            string directory = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(directory) && !string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (FileStream fileStream = File.Create(savePath))
            {
                byte[] buffer = new byte[1024]; 
                long totalBytesReceived = 0;
                int bytesRead;

                while (totalBytesReceived < fileSize &&
                       (bytesRead = await _stream.ReadAsync(buffer, 0, 
                           (int)Math.Min(buffer.Length, fileSize - totalBytesReceived))) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    totalBytesReceived += bytesRead;
                }
                
                if (totalBytesReceived != fileSize)
                {
                    throw new Exception($"Неполная загрузка. Получено {totalBytesReceived} из {fileSize} байт");
                }

                return true;
            }
        }
        catch (Exception ex)
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }
            return false;
        }
    }
    
}

