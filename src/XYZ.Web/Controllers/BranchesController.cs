using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using XYZ.Application.Features.Branches.Commands.CreateBranch;
using XYZ.Application.Features.Branches.Commands.UpdateBranch;
using XYZ.Domain.Constants;
using XYZ.Web.Models.Branches;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize(Roles = RoleNames.AdminCoachOrSuperAdmin)]
    public class BranchesController : Controller
    {
        private readonly IApiClient _apiClient;

        public BranchesController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        {
            var result = await _apiClient.GetBranchesAsync(pageNumber, pageSize, cancellationToken);
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            var dto = await _apiClient.GetBranchAsync(id, cancellationToken);
            if (dto is null) return NotFound();

            return View(dto);
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.AdminOrSuperAdmin)]
        public IActionResult Create()
        {
            return View(new BranchCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleNames.AdminOrSuperAdmin)]
        public async Task<IActionResult> Create(BranchCreateViewModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var id = await _apiClient.CreateBranchAsync(new CreateBranchCommand
                {
                    Name = model.Name.Trim()
                }, cancellationToken);

                TempData["SuccessMessage"] = "Şube oluşturuldu.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "Şube oluşturulamadı. Lütfen tekrar deneyin.");
                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.AdminOrSuperAdmin)]
        public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
        {
            var dto = await _apiClient.GetBranchAsync(id, cancellationToken);
            if (dto is null) return NotFound();

            return View(new BranchEditViewModel
            {
                Id = dto.Id,
                Name = dto.Name
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleNames.AdminOrSuperAdmin)]
        public async Task<IActionResult> Edit(int id, BranchEditViewModel model, CancellationToken cancellationToken)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            try
            {
                await _apiClient.UpdateBranchAsync(new UpdateBranchCommand
                {
                    BranchId = id,
                    Name = model.Name.Trim()
                }, cancellationToken);

                TempData["SuccessMessage"] = "Şube güncellendi.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "Şube güncellenemedi. Lütfen tekrar deneyin.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleNames.AdminOrSuperAdmin)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            try
            {
                await _apiClient.DeleteBranchAsync(id, cancellationToken);
                TempData["SuccessMessage"] = "Şube silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = ex.StatusCode == HttpStatusCode.NotFound
                    ? "Şube bulunamadı."
                    : "Şube silinemedi. Lütfen tekrar deneyin.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}
