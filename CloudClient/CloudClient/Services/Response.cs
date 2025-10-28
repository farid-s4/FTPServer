namespace CloudClient.Services;

public class Response<T>
{
    public string Status { get; set; } = "OK";
    public string? Message { get; set; }
    public T? Data { get; set; }
}
