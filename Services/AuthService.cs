using System.Net.Http.Json;

namespace SH_Entry_Portal.Services;

public class AuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public AuthService(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    // Verifies credentials against Supabase Auth; the actual session is tracked via an auth cookie, not here
    public async Task<bool> VerifyCredentialsAsync(string email, string password)
    {
        var supabaseUrl = _config["Supabase:Url"];
        var anonKey = _config["Supabase:AnonKey"];

        var client = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, $"{supabaseUrl}/auth/v1/token?grant_type=password");
        request.Headers.Add("apikey", anonKey);
        request.Content = JsonContent.Create(new { email, password });

        var response = await client.SendAsync(request);
        return response.IsSuccessStatusCode;
    }
}

