using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
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

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (ok, errorCode) = await _api.ChangeMyPasswordAsync(
                model.CurrentPassword,
                model.NewPassword,
                ct);

            if (ok)
            {
                TempData["SuccessMessage"] = "Şifreniz güncellendi.";
                return RedirectToAction("Index", "Settings");
            }

            var msg = errorCode switch
            {
                "invalid_current_password" => "Mevcut şifre hatalı.",
                "password_policy_failed" => "Yeni şifre kurallara uymuyor. Daha güçlü bir şifre deneyin.",
                _ => "Şifre değiştirilemedi. Lütfen tekrar deneyin."
            };

            ModelState.AddModelError("", msg);
            return View(model);
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
            var url = await _api.UploadMyProfilePictureAsync(stream, file.FileName, file.ContentType, ct);

            if (string.IsNullOrWhiteSpace(url))
            {
                TempData["ErrorMessage"] = "Fotoğraf yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Edit));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Response.Cookies.Append(
                $"xyz_pp_{userId}",
                url,
                new CookieOptions
                {
                    Path = "/",
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax
                });

            TempData["SuccessMessage"] = "Profil fotoğrafı güncellendi.";
            return RedirectToAction(nameof(Edit));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePicture(CancellationToken ct)
        {
            try
            {
                var me = await _api.GetMyProfileAsync(ct);
                if (string.IsNullOrWhiteSpace(me?.ProfilePictureUrl))
                {
                    TempData["SuccessMessage"] = "Profil fotoğrafı bulunamadı.";
                    return RedirectToAction(nameof(Edit));
                }

                await _api.DeleteMyProfilePictureAsync(ct);

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                Response.Cookies.Delete($"xyz_pp_{userId}", new CookieOptions { Path = "/" });

                TempData["SuccessMessage"] = "Profil fotoğrafı silindi.";
                return RedirectToAction(nameof(Edit));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Profil fotoğrafı silinirken bir hata oluştu.";
                return RedirectToAction(nameof(Edit));
            }
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
