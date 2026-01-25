using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Features.Tenants.Commands.UpdateCurrentTenantTheme;
using XYZ.Web.Models.Settings;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers;

[Authorize]
public sealed class SettingsController : Controller
{
    private readonly IApiClient _api;

    public SettingsController(IApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var profile = await _api.GetMyProfileAsync(ct);
        if (profile is null) return RedirectToAction("Login", "Account");

        ViewData["ProfilePictureUrl"] = profile.ProfilePictureUrl;

        var model = new SettingsViewModel
        {
            FullName = $"{profile.FirstName} {profile.LastName}".Trim(),
            Email = profile.Email,
            Role = profile.Role,
            TenantName = profile.TenantName
        };

        return View(model);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet]
    public async Task<IActionResult> Club(CancellationToken ct)
    {
        var theme = await _api.GetCurrentTenantThemeRawAsync(ct);
        if (theme is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var model = new ClubSettingsViewModel
        {
            TenantName = theme.Name,
            PrimaryColor = theme.PrimaryColor,
            SecondaryColor = theme.SecondaryColor,
            LogoUrl = theme.LogoUrl
        };

        return View(model);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Club(ClubSettingsViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);

        var ok = await _api.UpdateCurrentTenantThemeAsync(new UpdateCurrentTenantThemeCommand
        {
            PrimaryColor = model.PrimaryColor,
            SecondaryColor = model.SecondaryColor,
            LogoUrl = model.LogoUrl
        }, ct);

        if (!ok)
        {
            ModelState.AddModelError("", "Tema güncellenemedi. Lütfen tekrar deneyin.");
            return View(model);
        }

        Response.Cookies.Delete("TenantTheme");

        TempData["SuccessMessage"] = "Kulüp teması güncellendi.";
        return RedirectToAction(nameof(Club));
    }

}
