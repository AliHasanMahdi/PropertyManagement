using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PropertyManagement.MVC.Services
{
    public class ApiClientService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TokenService _tokenService;
        private readonly IConfiguration _configuration;

        public ApiClientService(
            IHttpClientFactory httpClientFactory,
            TokenService tokenService,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient("API");
            var token = _tokenService.GetToken();

            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        public async Task<(bool Success, string? Token, IList<string>? Roles, string? Error)>
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
                    return (false, null, null, "Invalid email or password");

                using var doc = JsonDocument.Parse(responseString);
                var root = doc.RootElement;

                var token = root.GetProperty("token").GetString()!;
                var rolesArray = root.GetProperty("roles");
                var roles = new List<string>();

                foreach (var role in rolesArray.EnumerateArray())
                    roles.Add(role.GetString()!);

                return (true, token, roles, null);
            }
            catch (Exception ex)
            {
                return (false, null, null, ex.Message);
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
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                return default;
            }
        }

        public async Task<(bool Success, string? Error)> PostAsync<T>(
            string endpoint, T data)
        {
            try
            {
                var client = CreateClient();
                var content = new StringContent(
                    JsonSerializer.Serialize(data),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(endpoint, content);
                if (response.IsSuccessStatusCode) return (true, null);

                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? Error)> PutAsync<T>(
            string endpoint, T data)
        {
            try
            {
                var client = CreateClient();
                var content = new StringContent(
                    JsonSerializer.Serialize(data),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PutAsync(endpoint, content);
                if (response.IsSuccessStatusCode) return (true, null);

                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(string endpoint)
        {
            try
            {
                var client = CreateClient();
                var response = await client.DeleteAsync(endpoint);
                if (response.IsSuccessStatusCode) return (true, null);

                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}