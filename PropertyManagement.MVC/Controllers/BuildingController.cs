using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using PropertyManagement.API.Models; // Grabs your shared Building model structure

namespace PropertyManagement.MVC.Controllers
{
    public class BuildingController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        // Constructor: Inject HttpClientFactory so we can spin up HTTP clients to talk to our API
        public BuildingController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        // Helper Method: Keeps our code DRY (Don't Repeat Yourself) by creating a configured HTTP client
        private HttpClient GetApiClient()
        {
            // Creates a client named "API" which reads the BaseUrl from appsettings.json
            return _clientFactory.CreateClient("API");
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

                return View(buildings); // Sends the list to Index.cshtml
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

            return View(building); // Sends a single building object to Details.cshtml
        }

        // 3. GET: /Building/Create (Just loads the blank creation form screen)
        public IActionResult Create()
        {
            return View();
        }

        // 4. POST: /Building/Create (Takes form submission data and posts it to the API)
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
                return RedirectToAction(nameof(Index)); // Send them back to the list
            }

            TempData["Error"] = "Could not save the new building.";
            return View(building);
        }

        // 5. GET: /Building/Edit/5 (Loads the edit page with data filled in)
        public async Task<IActionResult> Edit(int id)
        {
            var client = GetApiClient();
            var response = await client.GetAsync($"api/buildings/{id}");

            if (!response.IsSuccessStatusCode) return NotFound();

            var content = await response.Content.ReadAsStringAsync();
            var building = JsonSerializer.Deserialize<Building>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Explicitly loads your custom named view file we built earlier!
            return View("EditBuilding", building);
        }

        // 6. POST: /Building/Edit/5 (Submits form changes to the API)
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

        // 7. GET: /Building/Delete/5 (Loads a confirmation page before erasing data)
        public async Task<IActionResult> Delete(int id)
        {
            var client = GetApiClient();
            var response = await client.GetAsync($"api/buildings/{id}");

            if (!response.IsSuccessStatusCode) return NotFound();

            var content = await response.Content.ReadAsStringAsync();
            var building = JsonSerializer.Deserialize<Building>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(building);
        }

        // 8. POST: /Building/Delete/5 (The actual final "Confirm Delete" click)
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