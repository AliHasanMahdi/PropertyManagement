using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace PropertyManagement.MVC.Controllers
{
    public class LookupController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public LookupController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Track()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Track(string ticketNumber, string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(ticketNumber) || string.IsNullOrWhiteSpace(phoneNumber))
            {
                ViewBag.Error = "Please fill in both the ticket number and registered phone number.";
                return View();
            }

            var client = _httpClientFactory.CreateClient();

            // Adjust the port number (7001) if your API runs on a different port locally
            var response = await client.GetAsync($"https://localhost:7001/api/maintenancerequest/track/{ticketNumber.Trim()}/{phoneNumber.Trim()}");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "❌ Mismatch: No tracking records found for that ticket and phone number combo.";
                return View();
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Unpacks as a deserialized object to pass directly to our HTML elements
            var trackingData = JsonSerializer.Deserialize<dynamic>(jsonString, options);

            return View(trackingData);
        }
    }
}