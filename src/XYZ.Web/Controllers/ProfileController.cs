using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Web.Models.Profile;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IApiClient _apiClient;

        public ProfileController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var dto = await _apiClient.GetMyProfileAsync(cancellationToken);
            if (dto is null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new ProfileViewModel
            {
                Profile = dto,
                Initials = BuildInitials(dto.FullName)
            };

            ViewData["ProfilePictureUrl"] = dto.ProfilePictureUrl;

            return View(model);
        }

        [HttpGet]
        public IActionResult Edit()
        {
            return View("ComingSoon", new { Title = "Bilgilerimi Düzenle" });
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View("ComingSoon", new { Title = "Şifre Değiştir" });
        }

        private static string BuildInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "?";

            var parts = fullName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Take(2)
                .ToArray();

            if (parts.Length == 0) return "?";
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpperInvariant();

            return (parts[0].Substring(0, 1) + parts[1].Substring(0, 1)).ToUpperInvariant();
        }
    }
}
