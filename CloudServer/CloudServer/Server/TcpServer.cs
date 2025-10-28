using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using CloudServer.Model;
using CloudServer.Services;
using static CloudServer.Services.ClientsDataServices;

namespace CloudServer.Server;

public class TcpServer
{
    private readonly TcpListener _listener;
    private List<Client> _clients;
    private readonly CancellationTokenSource _cts  = new CancellationTokenSource();
    public event Action? OnStartedListening;

    public TcpServer(string ip, int port)
    {
        _clients  = new List<Client>();
        _listener = new TcpListener(IPAddress.Parse(ip), port);
        _ = InitClients();
    }

    private async Task InitClients()
    {
        _clients = await LoadClientsAsync();
    }
    
    public async Task Start()
    {
        _listener.Start();
        OnStartedListening?.Invoke();
        
        while (!_cts.IsCancellationRequested)
        {
            var client = await _listener.AcceptTcpClientAsync();
            _ = HandleClientAsync(client); 
        }
    }
    
    private async Task HandleClientAsync(TcpClient client)
    {
        var stream = client.GetStream();
        var _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
        var _reader = new StreamReader(stream, Encoding.UTF8);
        Client? sessionClient = null;

        while (!_cts.IsCancellationRequested && client.Connected)
        {
            string? json = await _reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(json)) continue;

            Command? cmd;
            
            cmd = JsonSerializer.Deserialize<Command>(json);
            
            if (cmd == null) continue;
            
            sessionClient = await ProcessCommandAsync(cmd, sessionClient, stream);
        }
    }
    
    private async Task<Client> ProcessCommandAsync(Command cmd, Client? receivedClient, Stream stream)
    {
        
        switch (cmd.CommandName)
        {
            case "REGISTER_USER":
            {
                var stringResponse = new Response<string>();
                
                if (cmd.Args == null || !cmd.Args.ContainsKey("username") || !cmd.Args.ContainsKey("password"))
                {
                    stringResponse.Status = "ERROR";
                    stringResponse.Message = "Отсутствуют данные для регистрации";
                    await SendResponse(stream, stringResponse);
                    break;
                }
                
                string username = cmd.Args["username"];
                string password = cmd.Args["password"];

                foreach (var c in _clients)
                {
                    if (c.Name == username)
                    {
                        stringResponse.Status = "ERROR";
                        stringResponse.Message = "Такой клиент уже есть";
                        await SendResponse(stream, stringResponse);
                        break;
                    }
                }

                var newClient = new Client
                {
                    Name = username,
                    Password = password,
                    RootPath = username + "root"
                };
                
                Directory.CreateDirectory(newClient.RootPath);
                
                _clients.Add(newClient);
                await ClientsDataServices.SaveClientsAsync(_clients);
                
                stringResponse.Status = "OK";
                stringResponse.Message = $"Пользовател {newClient.Name} успешно зарегистрирован";
                await SendResponse(stream, stringResponse);
                break;
            }
            case "LOGIN_USER":
            {
                var stringResponse = new Response<string>();
                if (cmd.Args == null || !cmd.Args.ContainsKey("username") || !cmd.Args.ContainsKey("password"))
                {
                    stringResponse.Status = "ERROR";
                    stringResponse.Message = "Отсутствуют данные для регистрации";
                    await SendResponse(stream, stringResponse);
                    break;
                }
                string username = cmd.Args["username"];
                string password = cmd.Args["password"];

                var clientResponse = new Response<Client>();
                
                var foundClient = _clients.FirstOrDefault(c => c.Name == username && c.Password == password);
                if (foundClient != null)
                {
                    receivedClient = foundClient;
                    clientResponse.Status = "LOGGED";
                    clientResponse.Message = $"Здравствуйте {foundClient.Name}";
                    clientResponse.Data = foundClient;
                    await SendResponse<Client>(stream, clientResponse);
                    
                    return receivedClient;
                }
                else
                {
                    stringResponse.Status = "ERROR";
                    stringResponse.Message = "Такой пользователь не найден";
                    await SendResponse(stream, stringResponse);
                }
                break;
            }
            case "LOGOUT_USER":
            {
                /*receivedClient = null;
                response.Status = "OK";
                response.Message = "Вы успешно вышли с системы";
                await SendResponse(stream, response);
                return receivedClient;*/
                break;
            }
            case "GET_DIRS":
            {
                var stringResponse = new Response<string>();
                if (cmd.Args == null || !cmd.Args.ContainsKey("username") || !cmd.Args.ContainsKey("password"))
                {
                    stringResponse.Status = "ERROR";
                    stringResponse.Message = "Отсутствуют данные для регистрации";
                    await SendResponse(stream, stringResponse);
                    break;
                }
                string username = cmd.Args["username"];
                string password = cmd.Args["password"];
                
                var foundClient = _clients.FirstOrDefault(c => c.Name == username && c.Password == password);
                var getDirResponse = new Response<FileDirectory>();
                if (foundClient != null)
                {
                    getDirResponse.Status = "OK";
                    getDirResponse.Data = await ClientGetDirServices.GetDirAsync(foundClient.RootPath);
                    await SendResponse(stream, getDirResponse);
                }
                break;
            }
            case "CREATE_DIRECTORY":
            {
                var stringResponse = new Response<string>();
                if (cmd.Args == null || !cmd.Args.ContainsKey("username") || !cmd.Args.ContainsKey("password"))
                {
                    stringResponse.Status = "ERROR";
                    stringResponse.Message = "Отсутствуют данные для создании директории";
                    await SendResponse(stream, stringResponse);
                    break;
                }
                string username = cmd.Args["username"];
                string password = cmd.Args["password"];
                string selectedRootPath = cmd.Args["selectedRoot"];
                string newDirName = cmd.Args["newRootName"];
                var foundClient = _clients.FirstOrDefault(c => c.Name == username && c.Password == password);

                if (foundClient != null)
                {
                    var dirs = await ClientGetDirServices.GetDirAsync(foundClient.RootPath);
                    dirs.Children.Add(dirs);
                    foreach (var dir in dirs.Children)
                    {
                        if (dir.Name == selectedRootPath && dir.IsDirectory)
                        {
                            string path = Path.Combine(dir.FullPath, newDirName);
                            
                            if (Directory.Exists(path))
                            {
                                stringResponse.Status = "ERROR";
                                stringResponse.Message = "Директория с таким именем уже существует";
                                await SendResponse(stream, stringResponse);
                                break;
                            }
                            
                            Directory.CreateDirectory(path);
                            stringResponse.Status = "OK";
                            stringResponse.Message = "Создана новая директория";
                            await SendResponse(stream, stringResponse);
                        }
                        else
                        {
                            stringResponse.Status = "ERROR";
                            await SendResponse(stream, stringResponse);
                        }
                    }
                }
                break;
            }
            case "DELETE_DIRECTORY":
            {
                var stringResponse = new Response<string>();
                if (cmd.Args == null || !cmd.Args.ContainsKey("username") || !cmd.Args.ContainsKey("password"))
                {
                    stringResponse.Status = "ERROR";
                    stringResponse.Message = "Отсутствуют данные для удаления директории";
                    await SendResponse(stream, stringResponse);
                    break;
                }
                string username = cmd.Args["username"];
                string password = cmd.Args["password"];
                string selectedRootPath = cmd.Args["selectedRoot"];
                var foundClient = _clients.FirstOrDefault(c => c.Name == username && c.Password == password);

                if (foundClient != null)
                {
                    var dirs = await ClientGetDirServices.GetDirAsync(foundClient.RootPath);
                    foreach (var dir in dirs.Children)
                    {
                        if (dir.Name == selectedRootPath && dir.IsDirectory)
                        {
                            Directory.Delete(dir.FullPath, true);
                            stringResponse.Status = "OK";
                            stringResponse.Message = "Директория удалена";
                            await SendResponse(stream, stringResponse);
                        }
                        else
                        {
                            stringResponse.Status = "ERROR";
                            await SendResponse(stream, stringResponse);
                        }
                    }
                }
                
                break;
            }
            case "RENAME_DIRECTORY":
            {
                var stringResponse = new Response<string>();
                if (cmd.Args == null || !cmd.Args.ContainsKey("username") || !cmd.Args.ContainsKey("password"))
                {
                    stringResponse.Status = "ERROR";
                    stringResponse.Message = "Отсутствуют данные для переименовывания директории";
                    await SendResponse(stream, stringResponse);
                    break;
                }
                string username = cmd.Args["username"];
                string password = cmd.Args["password"];
                string selectedRootPath = cmd.Args["selectedRoot"];
                string newNameDir = cmd.Args["newRootName"];
                var foundClient = _clients.FirstOrDefault(c => c.Name == username && c.Password == password);

                if (foundClient != null)
                {
                    var dirs = await ClientGetDirServices.GetDirAsync(foundClient.RootPath);
                    foreach (var dir in dirs.Children)
                    {
                        if (dir.Name == selectedRootPath && dir.IsDirectory)
                        {
                            string parentDir = Path.GetDirectoryName(dir.FullPath) ?? throw new InvalidOperationException();
                            string newFullPath = Path.Combine(parentDir, newNameDir);
                            
                            if (Directory.Exists(newFullPath))
                            {
                                stringResponse.Status = "ERROR";
                                stringResponse.Message = "Директория с таким именем уже существует";
                                await SendResponse(stream, stringResponse);
                                break;
                            }
                            
                            Directory.Move(dir.FullPath, newFullPath);
                            
                            stringResponse.Status = "OK";
                            stringResponse.Message = "Директория переименовывано";
                            await SendResponse(stream, stringResponse);
                        }
                    }
                }
                else
                {
                    stringResponse.Status = "ERROR";
                    await SendResponse(stream, stringResponse);
                }
                break;
            }
            case "UPLOAD_FILE":
            {
                var stringResponse = new Response<string>();
                if (cmd.Args == null || !cmd.Args.ContainsKey("username") || !cmd.Args.ContainsKey("password"))
                {
                    stringResponse.Status = "ERROR";
                    stringResponse.Message = "Отсутствуют данные для загрузки файла";
                    await SendResponse(stream, stringResponse);
                    break;
                }
                string username = cmd.Args["username"];
                string password = cmd.Args["password"];
                string selectedRootPath = cmd.Args["selectedRoot"];
                string fileName = cmd.Args["fileName"];
                long fileSize =  Convert.ToInt64(cmd.Args["fileSize"]);
                var foundClient = _clients.FirstOrDefault(c => c.Name == username && c.Password == password);

                if (foundClient != null)
                {
                    var dirs = await ClientGetDirServices.GetDirAsync(foundClient.RootPath);
                    var targetDir = FindDirectory(dirs, selectedRootPath);

                    if (targetDir == null)
                    {
                        stringResponse.Status = "ERROR";
                        stringResponse.Message = "Директория не найдена";
                        await SendResponse(stream, stringResponse);
                        break;
                    }
                    string fullFilePath = Path.Combine(targetDir.FullPath, fileName);
                    bool success = await ReceiveFile(stream, fullFilePath, fileSize);

                    if (success)
                    {
                        stringResponse.Status = "OK";
                        stringResponse.Message = "Файл успешно загружен";
                    }
                    else
                    {
                        stringResponse.Status = "ERROR";
                        stringResponse.Message = "Ошибка при сохранении файла";
                    }
                }
                await SendResponse(stream, stringResponse);
                break;
            }
            case "DOWNLOAD_FILE":
            {
                var stringResponse = new Response<string>();
                if (cmd.Args == null || !cmd.Args.ContainsKey("username") || !cmd.Args.ContainsKey("password"))
                {
                    stringResponse.Status = "ERROR";
                    stringResponse.Message = "Отсутствуют данные для загрузки файла";
                    await SendResponse(stream, stringResponse);
                    break;
                }
                string username = cmd.Args["username"];
                string password = cmd.Args["password"];
                string fileName = cmd.Args["fileName"];
                var foundClient = _clients.FirstOrDefault(c => c.Name == username && c.Password == password);
                bool success = false;
                if (foundClient != null)
                {
                    var dirs = await ClientGetDirServices.GetDirAsync(foundClient.RootPath);
                    FileDirectory? file = FindFile(dirs, fileName);
                    if (file == null)
                    {
                        stringResponse.Status = "ERROR";
                        stringResponse.Message = "Файл не был найден";
                    }
                    else
                    {
                        success = await SendFile(stream,  file.FullPath);
                    }
                    
                    
                    if (success)
                    {
                        stringResponse.Status = "OK";
                        stringResponse.Message = "Файл успешно загружен";
                    }
                    else
                    {
                        stringResponse.Status = "ERROR";
                        stringResponse.Message = "Ошибка при сохранении файла";
                    }
                }
                break;
            }
        }
        return receivedClient;
    }
    
    
    private async Task SendResponse<T>(Stream stream, Response<T> response)
    {
        string json = JsonSerializer.Serialize(response);
        byte[] data = Encoding.UTF8.GetBytes(json + "\n");
        await stream.WriteAsync(data, 0, data.Length);
    }
    
    private FileDirectory? FindFile(FileDirectory directory, string fileName)
    {
        if (directory.Name == fileName && !directory.IsDirectory)
        {
            return directory;
        }

        foreach (var child in directory.Children)
        {
            var found = FindFile(child, fileName);
            if (found != null)
                return found;
        }

        return null;
    }
    
    private FileDirectory? FindDirectory(FileDirectory directory, string targetName)
    {
        if (directory.Name == targetName && directory.IsDirectory)
        {
            return directory;
        }
        
        foreach (var child in directory.Children)
        {
            if (child.IsDirectory)
            {
                var found = FindDirectory(child, targetName);
                if (found != null)
                    return found;
            }
        }

        return null;
    }
    
    private async Task<bool> SendFile(Stream stream, string filePath)
    {
        const int bufferSize = 1024;
        byte[] buffer = new byte[bufferSize];
        int bytesRead;

        try
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] fileSizeBytes = BitConverter.GetBytes(fileStream.Length);
                await stream.WriteAsync(fileSizeBytes, 0, fileSizeBytes.Length);

                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, bufferSize)) > 0)
                {
                    await stream.WriteAsync(buffer, 0, bytesRead);
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
    
    private async Task<bool> ReceiveFile(Stream stream, string filePath, long fileSize)
    {
        const int bufferSize = 1024; 
        byte[] buffer = new byte[bufferSize];
        long totalBytesRead = 0;
        int bytesRead;

        try
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                while (totalBytesRead < fileSize)
                {
                    int bytesToRead = (int)Math.Min(bufferSize, fileSize - totalBytesRead);
                    bytesRead = await stream.ReadAsync(buffer, 0, bytesToRead);
                
                    if (bytesRead == 0)
                        break; 

                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    totalBytesRead += bytesRead;
                }
            }
            
            bool success = totalBytesRead == fileSize;
            return success;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
    
    public void Stop()
    {
        _cts.Cancel();
        _listener.Stop();
    }

    public void Dispose()
    {
        _listener.Dispose();
        _cts.Dispose();
    }
}


