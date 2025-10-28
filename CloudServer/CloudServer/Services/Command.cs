namespace CloudServer.Services;

public class Command
{
    public string CommandName { get; set; } = "";
    public Dictionary<string, string>? Args { get; set; }
}