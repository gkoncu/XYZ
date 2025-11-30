using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XYZ.Application.Common.Models;
using XYZ.Application.Features.Admins.Queries.GetAllAdmins;
using XYZ.Application.Features.Admins.Queries.GetAdminById;
using XYZ.Web.Models.Admins;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminsController : Controller
    {
        private readonly IApiClient _apiClient;

        public AdminsController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchTerm,
            bool? isActive,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _apiClient.GetAdminsAsync(
                searchTerm,
                isActive,
                pageNumber,
                pageSize,
                cancellationToken);

            ViewBag.SearchTerm = searchTerm;
            ViewBag.IsActive = isActive;

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var admin = await _apiClient.GetAdminAsync(id, cancellationToken);
            if (admin is null)
            {
                return NotFound();
            }

            return View(admin);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var dto = await _apiClient.GetAdminAsync(id, cancellationToken);
            if (dto is null)
            {
                return NotFound();
            }

            var fullName = dto.FullName?.Trim() ?? "";
            string first = fullName;
            string last = "";

            var lastSpace = fullName.LastIndexOf(' ');
            if (lastSpace > 0)
            {
                first = fullName[..lastSpace];
                last = fullName[(lastSpace + 1)..];
            }

            var vm = new AdminEditViewModel
            {
                Id = dto.Id,
                FirstName = first,
                LastName = last,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,

                TenantName = dto.TenantName,
                IdentityNumber = dto.IdentityNumber,
                CanManageUsers = dto.CanManageUsers,
                CanManageFinance = dto.CanManageFinance,
                CanManageSettings = dto.CanManageSettings,
                IsActive = dto.IsActive
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            AdminEditViewModel model,
            CancellationToken cancellationToken)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var payload = new
            {
                model.FirstName,
                model.LastName,
                model.Email,
                model.PhoneNumber,
                model.IdentityNumber,
                model.CanManageUsers,
                model.CanManageFinance,
                model.CanManageSettings
            };

            var response = await _apiClient.PutAsJsonAsync($"admins/{id}", payload, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Account");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                ModelState.AddModelError(string.Empty, "Bu admin kaydını güncelleme yetkiniz yok.");
                return View(model);
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);

                ModelState.AddModelError(string.Empty, $"API Hatası: {errorBody}");

                return View(model);
            }


            TempData["SuccessMessage"] = "Admin yetkileri güncellendi.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public IActionResult Create()
        {
            var vm = new AdminCreateViewModel();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            AdminCreateViewModel model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var payload = new
            {
                model.FirstName,
                model.LastName,
                model.Email,
                model.PhoneNumber,
                model.TenantId,
                model.IdentityNumber,
                model.CanManageUsers,
                model.CanManageFinance,
                model.CanManageSettings
            };

            var response = await _apiClient.PostAsJsonAsync("admins", payload, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Account");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                ModelState.AddModelError(string.Empty, "Admin oluşturma yetkiniz yok.");
                return View(model);
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);

                ModelState.AddModelError(string.Empty, $"API Hatası: {errorBody}");

                return View(model);
            }


            var id = await response.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);

            TempData["SuccessMessage"] = "Admin oluşturuldu.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
                int id,
                CancellationToken cancellationToken)
        {
            var response = await _apiClient.DeleteAsync($"admins/{id}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return RedirectToAction("Login", "Account");
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                TempData["ErrorMessage"] = "Bu admin kaydını silme yetkiniz yok.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Admin silinirken bir hata oluştu.";
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["SuccessMessage"] = "Admin silindi.";
            return RedirectToAction(nameof(Index));
        }

    }
}
