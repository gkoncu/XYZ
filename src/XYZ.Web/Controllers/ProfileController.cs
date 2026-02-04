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
        private readonly IConfiguration _config;

        public ProfileController(IApiClient api, IConfiguration config)
        {
            _api = api;
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var dto = await _api.GetMyProfileAsync(ct);
            if (dto is null) return RedirectToAction("Login", "Account");

            ViewData["ProfilePictureUrl"] = NormalizeAssetUrl(dto.ProfilePictureUrl);

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

            ViewData["ProfilePictureUrl"] = NormalizeAssetUrl(dto.ProfilePictureUrl);

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPicture(IFormFile file, CancellationToken ct)
        {
            if (file is null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Lütfen bir dosya seçin.";
                return RedirectToAction(nameof(Edit));
            }

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext) || !allowed.Contains(ext, StringComparer.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Sadece JPG, PNG veya WEBP yükleyebilirsiniz.";
                return RedirectToAction(nameof(Edit));
            }

            await using var stream = file.OpenReadStream();
            var url = await _api.UploadMyProfilePictureAsync(stream, file.FileName, ct);

            if (string.IsNullOrWhiteSpace(url))
            {
                TempData["ErrorMessage"] = "Fotoğraf yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Edit));
            }

            var normalized = NormalizeAssetUrl(url);

            if (!string.IsNullOrWhiteSpace(normalized))
            {
                Response.Cookies.Append(
                    XYZ.Web.Common.WebCookieNames.ProfilePictureUrl,
                    normalized,
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddDays(7),
                        HttpOnly = false,
                        Secure = true,
                        SameSite = SameSiteMode.Lax,
                        IsEssential = true
                    });
            }

            TempData["SuccessMessage"] = "Profil fotoğrafı güncellendi.";
            return RedirectToAction(nameof(Edit));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePicture(CancellationToken ct)
        {
            var ok = await _api.DeleteMyProfilePictureAsync(ct);
            if (!ok)
            {
                TempData["ErrorMessage"] = "Profil fotoğrafı silinirken bir hata oluştu.";
                return RedirectToAction(nameof(Edit));
            }

            Response.Cookies.Delete(XYZ.Web.Common.WebCookieNames.ProfilePictureUrl);

            TempData["SuccessMessage"] = "Profil fotoğrafı silindi.";
            return RedirectToAction(nameof(Edit));
        }

        private string? NormalizeAssetUrl(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            if (raw.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                raw.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return raw;

            var baseUrl = _config["Api:BaseUrl"] ?? string.Empty;

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var apiUri))
                return raw;

            var origin = apiUri.GetLeftPart(UriPartial.Authority);

            if (!raw.StartsWith("/"))
                raw = "/" + raw;

            return $"{origin}{raw}?v={DateTime.UtcNow.Ticks}";
        }

        private static string BuildInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var initials = string.Concat(parts.Take(2).Select(p => char.ToUpperInvariant(p[0])));
            return string.IsNullOrWhiteSpace(initials) ? "?" : initials;
        }
    }
}
