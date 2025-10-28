using System.Net.Sockets;
using System.Text.Json.Serialization;

namespace CloudServer.Model;

public class Client
{
    public string Name { get; set; } = "";
    public string Password { get; set; } = "";
    public string RootPath { get; set; } = "";
}