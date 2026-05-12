using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PropertyManagement.Reporting.Services
{
    public class ApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient("API");
            var token = _httpContextAccessor.HttpContext?
                .Session.GetString("JwtToken");

            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        public async Task<(bool Success, string? Token, string? Error)>
            LoginAsync(string email, string password)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("API");
                var content = new StringContent(
                    JsonSerializer.Serialize(new { email, password }),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync("api/auth/login", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return (false, null, "Invalid credentials");

                using var doc = JsonDocument.Parse(responseString);
                var token = doc.RootElement.GetProperty("token").GetString();

                return (true, token, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                var client = CreateClient();
                var response = await client.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode) return default;

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch
            {
                return default;
            }
        }
    }
}