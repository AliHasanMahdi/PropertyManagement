using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PropertyManagement.Reporting.Models;
using PropertyManagement.Reporting.Services;

namespace PropertyManagement.Reporting.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ApiService _apiService;

        public ReportController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var occupancy = await _apiService
                .GetAsync<OccupancyReport>("api/reporting/occupancy");

            var maintenanceStats = await _apiService
                .GetAsync<MaintenanceStats>("api/reporting/maintenance-stats");

            var leases = await _apiService
                .GetAsync<List<LeaseModel>>("api/leases") ?? new();

            var maintenance = await _apiService
                .GetAsync<List<MaintenanceModel>>("api/maintenance") ?? new();

            var buildings = await _apiService
                .GetAsync<List<BuildingModel>>("api/buildings") ?? new();

            var model = new DashboardViewModel
            {
                Occupancy = occupancy,
                MaintenanceStats = maintenanceStats,
                OverdueLeases = leases
                    .Where(l => l.HasOverduePayments).ToList(),
                PendingRequests = maintenance
                    .Where(m => m.Status == "Submitted").ToList(),
                Buildings = buildings
            };

            return View(model);
        }

        public async Task<IActionResult> OccupancyReport()
        {
            var buildings = await _apiService
                .GetAsync<List<BuildingModel>>("api/buildings") ?? new();

            return View(buildings);
        }

        public async Task<IActionResult> MaintenanceReport()
        {
            var requests = await _apiService
                .GetAsync<List<MaintenanceModel>>("api/maintenance") ?? new();

            return View(requests);
        }

        public async Task<IActionResult> LeaseReport()
        {
            var leases = await _apiService
                .GetAsync<List<LeaseModel>>("api/leases") ?? new();

            return View(leases);
        }
    }
}