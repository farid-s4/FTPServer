using CloudServer.Model;

namespace CloudServer.Services;

public static class ClientGetDirServices
{
    public static async Task<FileDirectory> GetDirAsync(string rootPath)
    {
        if (!Directory.Exists(rootPath))
        {
            throw new Exception("Root path error.");
        }

        return await GetDirectoryRecursiveAsync(rootPath);
    }
    
    private static async Task<FileDirectory> GetDirectoryRecursiveAsync(string rootPath)
    {
        var isDirectory = Directory.Exists(rootPath);

        var child = new FileDirectory
        {
            Name = Path.GetFileName(rootPath),
            FullPath = Path.GetFullPath(rootPath),
            IsDirectory = isDirectory,
            Children = new List<FileDirectory>()
        };
        
        if (isDirectory)
        {
            foreach (var dir in Directory.GetDirectories(rootPath))
            {
                child.Children.Add(await GetDirectoryRecursiveAsync(dir));
            }

            foreach (var file in Directory.GetFiles(rootPath))
            {
                child.Children.Add(new FileDirectory
                {
                    Name = Path.GetFileName(file),
                    FullPath = Path.GetFullPath(file),
                    IsDirectory = false,
                });
            }
        }
        return child;
    }
}



