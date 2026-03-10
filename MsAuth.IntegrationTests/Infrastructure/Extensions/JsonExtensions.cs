namespace MsAuth.IntegrationTests.Infrastructure.Extensions;

public static class JsonExtensions
{
    public static string PrettifyJson(this string json)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
        return JsonSerializer.Serialize(jsonElement, options);
    }
}
