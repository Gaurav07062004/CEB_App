using System.Text.Json;

namespace MyEmploylog.Services;

/// <summary>
/// Calls the CEB PMS utility REST service (AppConfig.ApiBaseUrl).
/// </summary>
public class CebApiService
{
    readonly HttpClient _http = new()
    {
        BaseAddress = new Uri(AppConfig.ApiBaseUrl),
        Timeout = TimeSpan.FromSeconds(30),
    };

    /// <summary>
    /// The service's user name for a signed-in Microsoft account:
    /// sam.hotel@ceb.mu  ->  SAM.HOTEL
    /// </summary>
    public static string ToServiceUser(string email) =>
        email.Split('@')[0].ToUpperInvariant();

    /// <summary>GET api/getuser2?puser=... and return the raw JSON.</summary>
    public async Task<string> GetUserJsonAsync(string puser)
    {
        var path = $"api/getuser2?puser={Uri.EscapeDataString(puser)}";
        using var response = await _http.GetAsync(path);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"{(int)response.StatusCode} {response.ReasonPhrase}: {body}",
                null, response.StatusCode);

        return body;
    }

    /// <summary>
    /// Turns the JSON reply into "Field: value" lines for display, whatever its shape.
    /// Once the real fields are known, replace this with a typed record.
    /// </summary>
    public static string ToDisplayText(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                root = root[0];

            if (root.ValueKind != JsonValueKind.Object)
                return root.ToString();

            return string.Join(Environment.NewLine,
                root.EnumerateObject().Select(p => $"{p.Name}: {p.Value}"));
        }
        catch (JsonException)
        {
            return json;   // not JSON: show it as it came
        }
    }
}
