namespace FivetranClient.Models;

public class Schema
{
    public string NameInDestination { get; set; } = string.Empty;
    public bool? Enabled { get; set; }
    public Dictionary<string, Table> Tables { get; set; } = [];
}