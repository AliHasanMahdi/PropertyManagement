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

        public IndexModel(
            IHttpClientFactory httpClientFactory,
            ILogger<IndexModel> logger,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger            = logger;
            _configuration     = configuration;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            try
            {
                var client  = _httpClientFactory.CreateClient("API");
                var baseUrl = client.BaseAddress?.ToString()
                              ?? _configuration["ApiSettings:BaseUrl"]
                              ?? string.Empty;

                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    Input.ErrorMessage = "API base URL is not configured.";
                    return Page();
                }

                if (!baseUrl.EndsWith("/")) baseUrl += "/";

                // Call the dedicated public lookup endpoint that validates ticket + phone
                var url = new Uri(new Uri(baseUrl),
                    $"api/maintenance/lookup?ticketNumber={Uri.EscapeDataString(Input.TicketNumber)}" +
                    $"&phone={Uri.EscapeDataString(Input.Phone)}");

                HttpResponseMessage response;
                try
                {
                    response = await client.GetAsync(url);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "Failed to reach API at {Url}", url);
                    Input.ErrorMessage = "Could not reach the server. Please try again later.";
                    return Page();
                }

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    try
                    {
                        using var doc = JsonDocument.Parse(content);
                        Input.JsonResult = JsonSerializer.Serialize(
                            doc.RootElement,
                            new JsonSerializerOptions { WriteIndented = true });
                    }
                    catch
                    {
                        Input.RawResult = content;
                    }
                    Input.Found = true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Input.ErrorMessage = "No maintenance request found for the provided ticket number and phone number.";
                }
                else
                {
                    Input.ErrorMessage = $"Unexpected error: {(int)response.StatusCode} {response.ReasonPhrase}";
                    _logger.LogWarning("Tracking API returned {Status}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in tracking lookup");
                Input.ErrorMessage = "An unexpected error occurred. Please try again.";
            }

            return Page();
        }
    }
}
