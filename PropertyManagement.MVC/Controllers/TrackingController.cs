using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PropertyManagement.MVC.Models;
using Microsoft.Extensions.Configuration;

namespace PropertyManagement.MVC.Controllers
{
    public class TrackingController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TrackingController> _logger;
        private readonly IConfiguration _configuration;

        public TrackingController(IHttpClientFactory httpClientFactory, ILogger<TrackingController> logger, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
        }

        // Redirect to the Razor Page implementation so there is a single canonical tracking page.
        [HttpGet]
        public IActionResult Index()
        {
            return Redirect("/Tracking");
        }
    }
}
