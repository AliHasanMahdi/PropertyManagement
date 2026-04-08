using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using PropertyManagement.MVC.Models;

namespace PropertyManagement.MVC.Pages.Tracking
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;

        [BindProperty]
        public TrackingViewModel Input { get; set; } = new TrackingViewModel();

        public IndexModel(IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var client = _httpClientFactory.CreateClient("API");

                var baseUrl = client.BaseAddress?.ToString() ?? _configuration["ApiSettings:BaseUrl"];
                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    Input.ErrorMessage = "API base URL is not configured. Set ApiSettings:BaseUrl in appsettings.json.";
                    _logger.LogWarning("ApiSettings:BaseUrl is missing.");
                    return Page();
                }

                if (!baseUrl.EndsWith("/")) baseUrl += "/";

                var candidatePaths = new[]
                {
                    $"api/tracking/{Uri.EscapeDataString(Input.Reference)}",
                    $"api/public/tracking/{Uri.EscapeDataString(Input.Reference)}",
                    $"api/lookup/{Uri.EscapeDataString(Input.Reference)}",
                    $"api/public/lookup/{Uri.EscapeDataString(Input.Reference)}"
                };

                HttpResponseMessage? response = null;

                foreach (var path in candidatePaths)
                {
                    try
                    {
                        var requestUri = new Uri(new Uri(baseUrl), path);
                        response = await client.GetAsync(requestUri);
                    }
                    catch (HttpRequestException ex)
                    {
                        _logger.LogWarning(ex, "HTTP request failed for path {Path}", path);
                        response = null;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error building request URI for path {Path}", path);
                        response = null;
                    }

                    if (response == null)
                        continue;

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        try
                        {
                            using var doc = JsonDocument.Parse(content);
                            Input.JsonResult = JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
                        }
                        catch
                        {
                            Input.RawResult = content;
                        }

                        Input.Found = true;
                        break;
                    }

                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        continue;
                    }

                    Input.ErrorMessage = $"API returned {(int)response.StatusCode} {response.ReasonPhrase} for {response.RequestMessage?.RequestUri}";
                    _logger.LogWarning("API returned {Status} for reference {Reference}", response.StatusCode, Input.Reference);
                    break;
                }

                if (!Input.Found && Input.ErrorMessage == null && Input.JsonResult == null && Input.RawResult == null)
                {
                    Input.ErrorMessage = "No data found for the provided reference. Confirm the API endpoint and reference value.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while calling tracking API");
                Input.ErrorMessage = "Unexpected error while calling the tracking API. See logs for details.";
            }

            return Page();
        }
    }
}