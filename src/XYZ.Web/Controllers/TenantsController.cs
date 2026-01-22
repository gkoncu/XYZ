using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XYZ.Application.Features.Tenants.Commands.CreateTenant;
using XYZ.Application.Features.Tenants.Commands.UpdateTenant;
using XYZ.Web.Models.Tenants;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class TenantsController : Controller
    {
        private readonly IApiClient _apiClient;

        public TenantsController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchTerm,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _apiClient.GetTenantsAsync(searchTerm, pageNumber, pageSize, cancellationToken);
            ViewBag.SearchTerm = searchTerm;
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var tenant = await _apiClient.GetTenantAsync(id, cancellationToken);
            if (tenant is null) return NotFound();

            var vm = new TenantDetailsViewModel
            {
                Tenant = tenant,
                CreateAdmin = new CreateTenantAdminViewModel()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin(int id, TenantDetailsViewModel model, CancellationToken cancellationToken)
        {
            var tenant = await _apiClient.GetTenantAsync(id, cancellationToken);
            if (tenant is null) return NotFound();

            ModelState.Clear();
            TryValidateModel(model.CreateAdmin, nameof(TenantDetailsViewModel.CreateAdmin));
            if (!ModelState.IsValid)
            {
                return View("Details", new TenantDetailsViewModel
                {
                    Tenant = tenant,
                    CreateAdmin = model.CreateAdmin
                });
            }

            var payload = new
            {
                FirstName = model.CreateAdmin.FirstName.Trim(),
                LastName = model.CreateAdmin.LastName.Trim(),
                Email = model.CreateAdmin.Email.Trim(),
                PhoneNumber = string.IsNullOrWhiteSpace(model.CreateAdmin.PhoneNumber) ? null : model.CreateAdmin.PhoneNumber.Trim(),
                TenantId = id,
                IdentityNumber = string.IsNullOrWhiteSpace(model.CreateAdmin.IdentityNumber) ? null : model.CreateAdmin.IdentityNumber.Trim(),
                CanManageUsers = model.CreateAdmin.CanManageUsers,
                CanManageFinance = model.CreateAdmin.CanManageFinance,
                CanManageSettings = model.CreateAdmin.CanManageSettings
            };

            var response = await _apiClient.PostAsJsonAsync("admins", payload, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized) return RedirectToAction("Login", "Account");
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                ModelState.AddModelError(string.Empty, "Admin oluşturma yetkiniz yok.");
                return View("Details", new TenantDetailsViewModel { Tenant = tenant, CreateAdmin = model.CreateAdmin });
            }

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                ModelState.AddModelError(string.Empty, $"API Hatası: {body}");
                return View("Details", new TenantDetailsViewModel { Tenant = tenant, CreateAdmin = model.CreateAdmin });
            }

            TempData["SuccessMessage"] = "Admin oluşturuldu. İlk şifre: Admin123!";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public IActionResult Create()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            return View(new TenantCreateViewModel
            {
                SubscriptionStartDate = today,
                SubscriptionEndDate = today.AddDays(30),
                PrimaryColor = "#3B82F6",
                SecondaryColor = "#1E40AF",
                SubscriptionPlan = "Basic"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TenantCreateViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return View(model);

            var command = new CreateTenantCommand
            {
                Name = model.Name.Trim(),
                Subdomain = model.Subdomain.Trim(),
                Address = model.Address,
                Phone = model.Phone,
                Email = model.Email,
                LogoUrl = model.LogoUrl,
                PrimaryColor = model.PrimaryColor,
                SecondaryColor = model.SecondaryColor,
                SubscriptionPlan = model.SubscriptionPlan,
                SubscriptionStartDate = model.SubscriptionStartDate?.ToDateTime(TimeOnly.MinValue),
                SubscriptionEndDate = model.SubscriptionEndDate?.ToDateTime(TimeOnly.MinValue)
            };

            var response = await _apiClient.PostAsJsonAsync("tenants", command, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized) return RedirectToAction("Login", "Account");
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                ModelState.AddModelError(string.Empty, "Tenant oluşturma yetkiniz yok.");
                return View(model);
            }

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                ModelState.AddModelError(string.Empty, $"API Hatası: {body}");
                return View(model);
            }

            var id = await response.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
            TempData["SuccessMessage"] = "Tenant oluşturuldu.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var dto = await _apiClient.GetTenantAsync(id, cancellationToken);
            if (dto is null) return NotFound();

            var vm = new TenantEditViewModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Subdomain = dto.Subdomain,
                Address = dto.Address,
                Phone = dto.Phone,
                Email = dto.Email,
                LogoUrl = dto.LogoUrl,
                PrimaryColor = dto.PrimaryColor,
                SecondaryColor = dto.SecondaryColor,
                SubscriptionPlan = dto.SubscriptionPlan,
                SubscriptionStartDate = DateOnly.FromDateTime(dto.SubscriptionStartDate),
                SubscriptionEndDate = DateOnly.FromDateTime(dto.SubscriptionEndDate),
                IsActive = dto.IsActive
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TenantEditViewModel model, CancellationToken cancellationToken)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            var command = new UpdateTenantCommand
            {
                TenantId = id,
                Name = model.Name.Trim(),
                Subdomain = model.Subdomain.Trim(),
                Address = model.Address,
                Phone = model.Phone,
                Email = model.Email,
                LogoUrl = model.LogoUrl,
                PrimaryColor = model.PrimaryColor,
                SecondaryColor = model.SecondaryColor,
                SubscriptionPlan = model.SubscriptionPlan,
                SubscriptionStartDate = model.SubscriptionStartDate?.ToDateTime(TimeOnly.MinValue),
                SubscriptionEndDate = model.SubscriptionEndDate?.ToDateTime(TimeOnly.MinValue),
                IsActive = model.IsActive
            };

            var response = await _apiClient.PutAsJsonAsync($"tenants/{id}", command, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized) return RedirectToAction("Login", "Account");
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                ModelState.AddModelError(string.Empty, "Tenant güncelleme yetkiniz yok.");
                return View(model);
            }
            if (response.StatusCode == HttpStatusCode.NotFound) return NotFound();

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                ModelState.AddModelError(string.Empty, $"API Hatası: {body}");
                return View(model);
            }

            TempData["SuccessMessage"] = "Tenant güncellendi.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var response = await _apiClient.DeleteAsync($"tenants/{id}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized) return RedirectToAction("Login", "Account");
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                TempData["ErrorMessage"] = "Tenant silme yetkiniz yok.";
                return RedirectToAction(nameof(Details), new { id });
            }
            if (response.StatusCode == HttpStatusCode.NotFound) return NotFound();

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                TempData["ErrorMessage"] = $"Tenant silinemedi: {body}";
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["SuccessMessage"] = "Tenant silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
