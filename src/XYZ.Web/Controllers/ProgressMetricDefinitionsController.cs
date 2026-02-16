using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using XYZ.Application.Features.ProgressMetricDefinitions.Commands.CreateProgressMetricDefinition;
using XYZ.Application.Features.ProgressMetricDefinitions.Commands.UpdateProgressMetricDefinition;
using XYZ.Domain.Constants;
using XYZ.Domain.Enums;
using XYZ.Web.Models.ProgressMetricDefinitions;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize(Roles = RoleNames.AdminCoachOrSuperAdmin)]
    public class ProgressMetricDefinitionsController : Controller
    {
        private readonly IApiClient _api;

        public ProgressMetricDefinitionsController(IApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int branchId, bool includeInactive = false, CancellationToken ct = default)
        {
            if (branchId <= 0) return BadRequest("branchId zorunludur.");

            var branch = await _api.GetBranchAsync(branchId, ct);
            if (branch is null) return NotFound();

            var items = await _api.GetProgressMetricDefinitionsAsync(branchId, includeInactive, ct);

            var vm = new ProgressMetricDefinitionsIndexViewModel
            {
                BranchId = branchId,
                BranchName = branch.Name,
                IncludeInactive = includeInactive,
                Items = items.OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToList()
            };

            return View(vm);
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.AdminOrSuperAdmin)]
        public async Task<IActionResult> Create(int branchId, CancellationToken ct = default)
        {
            var branch = await _api.GetBranchAsync(branchId, ct);
            if (branch is null) return NotFound();

            var vm = new ProgressMetricDefinitionEditViewModel
            {
                BranchId = branchId,
                BranchName = branch.Name,
                IsActive = true,
                IsRequired = false,
                SortOrder = 1,
                DataType = ProgressMetricDataType.Decimal
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.AdminOrSuperAdmin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProgressMetricDefinitionEditViewModel model, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var cmd = new CreateProgressMetricDefinitionCommand
                {
                    BranchId = model.BranchId,
                    Name = model.Name.Trim(),
                    DataType = model.DataType,
                    Unit = string.IsNullOrWhiteSpace(model.Unit) ? null : model.Unit.Trim(),
                    IsRequired = model.IsRequired,
                    SortOrder = model.SortOrder,
                    MinValue = model.MinValue,
                    MaxValue = model.MaxValue,
                    IsActive = model.IsActive
                };

                await _api.CreateProgressMetricDefinitionAsync(cmd, ct);

                TempData["SuccessMessage"] = "Metrik eklendi.";
                return RedirectToAction(nameof(Index), new { branchId = model.BranchId, includeInactive = true });
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = ex.StatusCode == HttpStatusCode.Conflict
                    ? "Bu şubede aynı isimde metrik var."
                    : "Metrik eklenemedi. Lütfen tekrar deneyin.";
                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.AdminOrSuperAdmin)]
        public async Task<IActionResult> Edit(int id, int branchId, CancellationToken ct = default)
        {
            var branch = await _api.GetBranchAsync(branchId, ct);
            if (branch is null) return NotFound();

            var items = await _api.GetProgressMetricDefinitionsAsync(branchId, includeInactive: true, ct);
            var item = items.FirstOrDefault(x => x.Id == id);
            if (item is null) return NotFound();

            var vm = new ProgressMetricDefinitionEditViewModel
            {
                Id = item.Id,
                BranchId = item.BranchId,
                BranchName = branch.Name,
                Name = item.Name,
                DataType = item.DataType,
                Unit = item.Unit,
                IsRequired = item.IsRequired,
                SortOrder = item.SortOrder,
                MinValue = item.MinValue,
                MaxValue = item.MaxValue,
                IsActive = item.IsActive
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.AdminOrSuperAdmin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProgressMetricDefinitionEditViewModel model, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var cmd = new UpdateProgressMetricDefinitionCommand
                {
                    Id = model.Id,
                    BranchId = model.BranchId,
                    Name = model.Name.Trim(),
                    DataType = model.DataType,
                    Unit = string.IsNullOrWhiteSpace(model.Unit) ? null : model.Unit.Trim(),
                    IsRequired = model.IsRequired,
                    SortOrder = model.SortOrder,
                    MinValue = model.MinValue,
                    MaxValue = model.MaxValue,
                    IsActive = model.IsActive
                };

                await _api.UpdateProgressMetricDefinitionAsync(model.Id, cmd, ct);

                TempData["SuccessMessage"] = "Metrik güncellendi.";
                return RedirectToAction(nameof(Index), new { branchId = model.BranchId, includeInactive = true });
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] = "Metrik güncellenemedi. Lütfen tekrar deneyin.";
                return View(model);
            }
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.AdminOrSuperAdmin)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int branchId, CancellationToken ct = default)
        {
            try
            {
                await _api.DeleteProgressMetricDefinitionAsync(id, ct);
                TempData["SuccessMessage"] = "Metrik silindi.";
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = ex.StatusCode == HttpStatusCode.BadRequest
                    ? "Bu metrik kullanıldığı için silinemedi. Pasif yapabilirsiniz."
                    : "Metrik silinemedi. Lütfen tekrar deneyin.";
            }

            return RedirectToAction(nameof(Index), new { branchId, includeInactive = true });
        }
    }
}
