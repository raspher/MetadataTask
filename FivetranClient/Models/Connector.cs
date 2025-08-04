namespace FivetranClient.Models;

public class Connector
{
    public required string Id { get; set; }
    public string Service { get; set; } = string.Empty;
    public string Schema { get; set; } = string.Empty;
    public bool? Paused { get; set; }
}