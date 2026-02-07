using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XYZ.Application.Features.Auth.DTOs;
using XYZ.Application.Features.Tenants.Commands.CreateTenant;
using XYZ.Application.Features.Tenants.Commands.UpdateTenant;
using XYZ.Web.Models.Tenants;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class TenantsController : Controller
    {
        private const string RefreshTokenCookieName = "xyz_rt";

        private readonly IApiClient _apiClient;
        private readonly IHttpClientFactory _httpClientFactory;

        public TenantsController(IApiClient apiClient, IHttpClientFactory httpClientFactory)
        {
            _apiClient = apiClient;
            _httpClientFactory = httpClientFactory;
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
                Tenant = tenant
            };

            return View(vm);
        }

        // === SWITCH CONTEXT: "Work with this tenant" ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SwitchContext(int id, CancellationToken cancellationToken)
        {
            var switchResponse = await _apiClient.PostAsJsonAsync("profile/me/tenant", new { tenantId = id }, cancellationToken);

            if (switchResponse.StatusCode == HttpStatusCode.Unauthorized) return RedirectToAction("Login", "Account");
            if (switchResponse.StatusCode == HttpStatusCode.Forbidden)
            {
                TempData["ErrorMessage"] = "Tenant değiştirme yetkiniz yok.";
                return RedirectToAction(nameof(Details), new { id });
            }
            if (switchResponse.StatusCode == HttpStatusCode.NotFound)
            {
                TempData["ErrorMessage"] = "Tenant bulunamadı.";
                return RedirectToAction(nameof(Index));
            }
            if (!switchResponse.IsSuccessStatusCode)
            {
                var body = await switchResponse.Content.ReadAsStringAsync(cancellationToken);
                TempData["ErrorMessage"] = $"Tenant değiştirilemedi: {body}";
                return RedirectToAction(nameof(Details), new { id });
            }

            var ok = await RefreshAndUpdateCookieAsync(cancellationToken);
            if (!ok)
            {
                TempData["ErrorMessage"] = "Kulüp değişti ancak oturum yenilenemedi. Lütfen çıkış yapıp tekrar giriş yapın.";
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["SuccessMessage"] = "Aktif kulüp değiştirildi (token yenilendi).";
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task<bool> RefreshAndUpdateCookieAsync(CancellationToken ct)
        {
            if (!Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
                return false;

            var client = _httpClientFactory.CreateClient("ApiNoAuth");

            HttpResponseMessage resp;
            try
            {
                resp = await client.PostAsJsonAsync("auth/refresh", new { RefreshToken = refreshToken }, ct);
            }
            catch
            {
                return false;
            }

            if (!resp.IsSuccessStatusCode)
                return false;

            var result = await resp.Content.ReadFromJsonAsync<LoginResultDto>(cancellationToken: ct);
            if (result is null || string.IsNullOrWhiteSpace(result.AccessToken) || string.IsNullOrWhiteSpace(result.RefreshToken))
                return false;

            Response.Cookies.Append(RefreshTokenCookieName, result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });

            var auth = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!auth.Succeeded || auth.Principal is null)
                return true;

            var identity = auth.Principal.Identities
                .FirstOrDefault(i => i.AuthenticationType == CookieAuthenticationDefaults.AuthenticationScheme)
                ?? (auth.Principal.Identity as ClaimsIdentity);

            if (identity is null)
                return true;

            ReplaceClaim(identity, "access_token", result.AccessToken);

            if (!string.IsNullOrWhiteSpace(result.TenantId))
                ReplaceClaim(identity, "tenant_id", result.TenantId);

            var props = auth.Properties ?? new AuthenticationProperties();
            props.ExpiresUtc = result.ExpiresAtUtc;

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                props);

            return true;
        }

        private static void ReplaceClaim(ClaimsIdentity identity, string claimType, string newValue)
        {
            var existing = identity.FindFirst(claimType);
            if (existing is not null)
                identity.RemoveClaim(existing);

            identity.AddClaim(new Claim(claimType, newValue));
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
