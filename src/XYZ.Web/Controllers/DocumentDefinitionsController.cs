using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XYZ.Application.Features.DocumentDefinitions.Commands.CreateDocumentDefinition;
using XYZ.Application.Features.DocumentDefinitions.Commands.UpdateDocumentDefinition;
using XYZ.Domain.Enums;
using XYZ.Web.Models.DocumentDefinitions;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class DocumentDefinitionsController : Controller
    {
        private readonly IApiClient _api;

        public DocumentDefinitionsController(IApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int target = 1, bool includeInactive = false, CancellationToken ct = default)
        {
            var items = await _api.GetDocumentDefinitionsAsync(target, includeInactive, ct);

            var vm = new DocumentDefinitionsIndexViewModel
            {
                Target = target,
                IncludeInactive = false ,
                Items = items
                    .OrderBy(x => x.SortOrder)
                    .ThenBy(x => x.Name)
                    .ToList()
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult Create(int target = 1)
        {
            var vm = new DocumentDefinitionEditViewModel
            {
                Target = target,
                IsActive = true,
                IsRequired = true,
                SortOrder = 100
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DocumentDefinitionEditViewModel model, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
                return View(model);

            var cmd = new CreateDocumentDefinitionCommand
            {
                Target = (DocumentTarget)model.Target,
                Name = model.Name.Trim(),
                IsRequired = model.IsRequired,
                IsActive = model.IsActive,
                SortOrder = model.SortOrder
            };

            await _api.CreateDocumentDefinitionAsync(cmd, ct);

            TempData["SuccessMessage"] = "Belge türü eklendi.";
            return RedirectToAction(nameof(Index), new { target = model.Target });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id, int target, CancellationToken ct = default)
        {
            var items = await _api.GetDocumentDefinitionsAsync(target, includeInactive: true, ct);
            var item = items.FirstOrDefault(x => x.Id == id);
            if (item is null) return NotFound();

            var vm = new DocumentDefinitionEditViewModel
            {
                Id = item.Id,
                Target = target,
                Name = item.Name,
                IsRequired = item.IsRequired,
                IsActive = item.IsActive,
                SortOrder = item.SortOrder
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DocumentDefinitionEditViewModel model, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
                return View(model);

            var cmd = new UpdateDocumentDefinitionCommand
            {
                Id = model.Id,
                Target = (DocumentTarget)model.Target,
                Name = model.Name.Trim(),
                IsRequired = model.IsRequired,
                IsActive = model.IsActive,
                SortOrder = model.SortOrder
            };

            await _api.UpdateDocumentDefinitionAsync(model.Id, cmd, ct);

            TempData["SuccessMessage"] = "Belge türü güncellendi.";
            return RedirectToAction(nameof(Index), new { target = model.Target, includeInactive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int target, CancellationToken ct = default)
        {
            await _api.DeleteDocumentDefinitionAsync(id, ct);

            TempData["SuccessMessage"] = "Belge türü silindi.";
            return RedirectToAction(nameof(Index), new { target, includeInactive = true });
        }
    }
}
