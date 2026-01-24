using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using XYZ.Application.Features.Announcements.Commands.CreateAnnouncement;
using XYZ.Application.Features.Announcements.Commands.UpdateAnnouncement;
using XYZ.Domain.Enums;
using XYZ.Web.Models.Announcements;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize]
    public class AnnouncementsController : Controller
    {
        private readonly IApiClient _api;

        public AnnouncementsController(IApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,SuperAdmin,Student")]
        public async Task<IActionResult> Index(
            string? searchTerm,
            int? classId,
            AnnouncementType? type,
            bool onlyCurrent = true,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken ct = default)
        {
            pageSize = Math.Clamp(pageSize, 1, 200);

            var result = await _api.GetAnnouncementsAsync(
                searchTerm,
                classId,
                type,
                onlyCurrent,
                pageNumber,
                pageSize,
                ct);

            var vm = new AnnouncementsIndexViewModel
            {
                SearchTerm = searchTerm,
                ClassId = classId,
                Type = type,
                OnlyCurrent = onlyCurrent,

                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                Items = result.Items?.ToList() ?? new List<XYZ.Application.Features.Announcements.Queries.GetAllAnnouncements.AnnouncementListItemDto>(),

                CanWrite = User.IsInRole("Admin") || User.IsInRole("Coach") || User.IsInRole("SuperAdmin")
            };

            await FillClassesSelectList(ct, selectedClassId: classId);
            vm.TypeOptions = BuildTypeOptions(selected: type);

            return View(vm);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,SuperAdmin,Student")]
        public async Task<IActionResult> Details(int id, CancellationToken ct = default)
        {
            var dto = await _api.GetAnnouncementAsync(id, ct);
            if (dto is null)
                return NotFound();

            ViewBag.CanWrite = User.IsInRole("Admin") || User.IsInRole("Coach") || User.IsInRole("SuperAdmin");
            return View(dto);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        public async Task<IActionResult> Create(CancellationToken ct = default)
        {
            await FillClassesSelectList(ct, selectedClassId: null);

            var vm = new AnnouncementUpsertViewModel
            {
                PublishDate = DateTime.Today,
                Type = AnnouncementType.General
            };

            vm.TypeOptions = BuildTypeOptions(selected: vm.Type);
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AnnouncementUpsertViewModel model, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
            {
                await FillClassesSelectList(ct, model.ClassId);
                model.TypeOptions = BuildTypeOptions(model.Type);
                return View(model);
            }

            await _api.CreateAnnouncementAsync(new CreateAnnouncementCommand
            {
                ClassId = model.ClassId,
                Title = model.Title?.Trim() ?? string.Empty,
                Content = model.Content?.Trim() ?? string.Empty,
                PublishDate = model.PublishDate,
                ExpiryDate = model.ExpiryDate,
                Type = model.Type
            }, ct);

            TempData["SuccessMessage"] = "Duyuru oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
        {
            var dto = await _api.GetAnnouncementAsync(id, ct);
            if (dto is null)
                return NotFound();

            await FillClassesSelectList(ct, dto.ClassId);

            var vm = new AnnouncementUpsertViewModel
            {
                Id = dto.Id,
                ClassId = dto.ClassId,
                Title = dto.Title,
                Content = dto.Content,
                PublishDate = dto.PublishDate,
                ExpiryDate = dto.ExpiryDate,
                Type = dto.Type
            };

            vm.TypeOptions = BuildTypeOptions(selected: vm.Type);
            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AnnouncementUpsertViewModel model, CancellationToken ct = default)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                await FillClassesSelectList(ct, model.ClassId);
                model.TypeOptions = BuildTypeOptions(model.Type);
                return View(model);
            }

            await _api.UpdateAnnouncementAsync(id, new UpdateAnnouncementCommand
            {
                Id = id,
                ClassId = model.ClassId,
                Title = model.Title?.Trim() ?? string.Empty,
                Content = model.Content?.Trim() ?? string.Empty,
                PublishDate = model.PublishDate,
                ExpiryDate = model.ExpiryDate,
                Type = model.Type,
                IsActive = null
            }, ct);

            TempData["SuccessMessage"] = "Duyuru güncellendi.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        {
            await _api.DeleteAnnouncementAsync(id, ct);
            TempData["SuccessMessage"] = "Duyuru silindi.";

            return RedirectToAction(nameof(Index));
        }

        private async Task FillClassesSelectList(CancellationToken ct, int? selectedClassId)
        {
            var classes = await _api.GetClassesAsync(
                searchTerm: null,
                branchId: null,
                isActive: true,
                pageNumber: 1,
                pageSize: 200,
                cancellationToken: ct);

            var items = classes.Items
                .OrderBy(x => x.Name)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name,
                    Selected = selectedClassId.HasValue && x.Id == selectedClassId.Value
                })
                .ToList();

            ViewBag.ClassesSelectList = items;
        }

        private static List<SelectListItem> BuildTypeOptions(AnnouncementType? selected)
        {
            var list = Enum.GetValues(typeof(AnnouncementType))
                .Cast<AnnouncementType>()
                .Select(t => new SelectListItem
                {
                    Value = t.ToString(),
                    Text = t switch
                    {
                        AnnouncementType.General => "Genel",
                        AnnouncementType.Match => "Maç",
                        AnnouncementType.Event => "Etkinlik",
                        AnnouncementType.System => "Sistem",
                        _ => t.ToString()
                    },
                    Selected = selected.HasValue && t == selected.Value
                })
                .ToList();

            return list;
        }
    }
}
