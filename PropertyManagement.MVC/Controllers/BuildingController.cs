using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyManagement.API.Models;
using PropertyManagement.MVC.Services;
using System.Text;
using System.Text.Json;

namespace PropertyManagement.MVC.Controllers
{
    [Authorize(Roles = "PropertyManager")]
    public class BuildingController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly TokenService _tokenService;

        public BuildingController(IHttpClientFactory clientFactory, TokenService tokenService)
        {
            _clientFactory = clientFactory;
            _tokenService  = tokenService;
        }

        // Creates an API client with the current user's JWT attached as a Bearer token
        private HttpClient GetApiClient()
        {
            var client = _clientFactory.CreateClient("API");
            var token  = _tokenService.GetToken();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        // 1. GET: /Building (List all buildings)
        public async Task<IActionResult> Index()
        {
            var client = GetApiClient();

            // Send a GET request to: https://localhost:7166/api/buildings
            var response = await client.GetAsync("api/buildings");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON string array back into a C# List of Buildings
                var buildings = JsonSerializer.Deserialize<List<Building>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return View(buildings); 
            }

            TempData["Error"] = "Failed to retrieve buildings from the server.";
            return View(new List<Building>());
        }

        // 2. GET: /Building/Details/5 (View one specific building)
        public async Task<IActionResult> Details(int id)
        {
            var client = GetApiClient();
            var response = await client.GetAsync($"api/buildings/{id}");

            if (!response.IsSuccessStatusCode) return NotFound();

            var content = await response.Content.ReadAsStringAsync();
            var building = JsonSerializer.Deserialize<Building>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(building); 
        }

        // 3. GET
        public IActionResult Create()
        {
            return View();
        }

        // 4. POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Building building)
        {
            if (!ModelState.IsValid) return View(building);

            var client = GetApiClient();

            // Turn our C# building object into raw JSON text
            var json = JsonSerializer.Serialize(building);
            var bodyContent = new StringContent(json, Encoding.UTF8, "application/json");

            // POST it down to the API backend
            var response = await client.PostAsync("api/buildings", bodyContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Building created successfully!";
                return RedirectToAction(nameof(Index)); 
            }

            TempData["Error"] = "Could not save the new building.";
            return View(building);
        }

        // 5. GET
        public async Task<IActionResult> Edit(int id)
        {
            var client = GetApiClient();
            var response = await client.GetAsync($"api/buildings/{id}");

            if (!response.IsSuccessStatusCode) return NotFound();

            var content = await response.Content.ReadAsStringAsync();
            var building = JsonSerializer.Deserialize<Building>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            
            return View("EditBuilding", building);
        }

        // 6. POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Building building)
        {
            if (id != building.Id) return BadRequest();
            if (!ModelState.IsValid) return View("EditBuilding", building);

            var client = GetApiClient();
            var json = JsonSerializer.Serialize(building);
            var bodyContent = new StringContent(json, Encoding.UTF8, "application/json");

            // Send a PUT request to update the database record via API
            var response = await client.PutAsync($"api/buildings/{id}", bodyContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Building modifications saved successfully!";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Error trying to update building record.";
            return View("EditBuilding", building);
        }

        // 7. GET
        public async Task<IActionResult> Delete(int id)
        {
            var client = GetApiClient();
            var response = await client.GetAsync($"api/buildings/{id}");

            if (!response.IsSuccessStatusCode) return NotFound();

            var content = await response.Content.ReadAsStringAsync();
            var building = JsonSerializer.Deserialize<Building>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(building);
        }

        // 8. POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = GetApiClient();

            // Issue a HTTP DELETE command directly to the API endpoint
            var response = await client.DeleteAsync($"api/buildings/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Building dropped from database registry.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Failed to drop the building. It might still contain active rental units.";
            return RedirectToAction(nameof(Index));
        }
    }
}