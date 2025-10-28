namespace CloudServer.Model;

public class FileDirectory
{
    public string Name { get; set; }
    public string FullPath { get; set; }
    public bool IsDirectory { get; set; }
    public List<FileDirectory> Children { get; set; }
}