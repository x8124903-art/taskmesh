using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace WebApp.Models;

[ExcludeFromCodeCoverage]
public class User
{
    [JsonPropertyName("idUser")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}
