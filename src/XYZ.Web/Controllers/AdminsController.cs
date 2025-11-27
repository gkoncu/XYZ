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

            var vm = new AdminEditViewModel
            {
                Id = dto.Id,
                UserId = dto.UserId,
                FullName = dto.FullName,
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
                ModelState.AddModelError(string.Empty, "Admin güncellenirken bir hata oluştu.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Admin yetkileri güncellendi.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
