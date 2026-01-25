using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Features.Profile.Commands.UpdateMyProfile;
using XYZ.Web.Models.Profile;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IApiClient _api;

        public ProfileController(IApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var dto = await _api.GetMyProfileAsync(ct);
            if (dto is null) return RedirectToAction("Login", "Account");

            ViewData["ProfilePictureUrl"] = dto.ProfilePictureUrl;

            var initials = BuildInitials($"{dto.FirstName} {dto.LastName}".Trim());

            var model = new ProfileViewModel
            {
                Profile = dto,
                Initials = initials
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(CancellationToken ct)
        {
            var dto = await _api.GetMyProfileAsync(ct);
            if (dto is null) return RedirectToAction("Login", "Account");

            ViewData["ProfilePictureUrl"] = dto.ProfilePictureUrl;

            var vm = new ProfileEditViewModel
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                Gender = dto.Gender,
                BloodType = dto.BloodType,
                BirthDate = dto.BirthDate
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileEditViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid) return View(model);

            var ok = await _api.UpdateMyProfileAsync(new UpdateMyProfileCommand
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                Gender = model.Gender,
                BloodType = model.BloodType,
                BirthDate = model.BirthDate
            }, ct);

            if (!ok)
            {
                ModelState.AddModelError("", "Profil güncellenemedi. Lütfen tekrar deneyin.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Profil bilgileri güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View("ComingSoon");
        }

        private static string BuildInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "?";
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Take(2).ToArray();
            if (parts.Length == 0) return "?";
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpperInvariant();
            return (parts[0].Substring(0, 1) + parts[1].Substring(0, 1)).ToUpperInvariant();
        }
    }
}
