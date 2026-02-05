using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Features.Auth.DTOs;
using XYZ.Web.Models.Account;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IApiClient _apiClient;

        public AccountController(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                if (User.IsInRole("SuperAdmin"))
                {
                    return RedirectToAction("Index", "SuperAdminDashboard");
                }
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "AdminDashboard");
                }
                if (User.IsInRole("Coach"))
                {
                    return RedirectToAction("Index", "CoachDashboard");

                }
                if (User.IsInRole("Student"))
                {
                    return RedirectToAction("Index", "StudentDashboard");
                }
            }

            var model = new LoginRequest
            {
                RememberMe = true,
                ReturnUrl = returnUrl
            };

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest model, string? returnUrl = null, CancellationToken ct = default)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var loginResult = await _apiClient.LoginAsync(model.Email, model.Password, ct);

            if (loginResult == null)
            {
                ModelState.AddModelError(string.Empty, "Email veya şifre hatalı.");
                return View(model);
            }

            // === Claims ===
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, loginResult.UserId),
                new Claim(ClaimTypes.Name, loginResult.FullName),
                new Claim(ClaimTypes.Email, loginResult.Email),
                new Claim("access_token", loginResult.AccessToken)
            };

            if (loginResult.Roles is not null)
            {
                foreach (var role in loginResult.Roles)
                {
                    if (!string.IsNullOrWhiteSpace(role))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(loginResult.TenantId))
            {
                claims.Add(new Claim("tenant_id", loginResult.TenantId));
            }

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            var authProps = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = loginResult.ExpiresAtUtc
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProps);

            var me = await _apiClient.GetMyProfileAsync(ct);

            if (!string.IsNullOrWhiteSpace(me?.ProfilePictureUrl))
            {
                Response.Cookies.Append(
                    $"xyz_pp_{me.UserId}",
                    me.ProfilePictureUrl,
                    new CookieOptions
                    {
                        Path = "/",
                        Secure = true,
                        SameSite = SameSiteMode.Lax
                    });
            }


            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (principal.IsInRole("SuperAdmin"))
            {
                return RedirectToAction("Index", "SuperAdminDashboard");
            }

            if (principal.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "AdminDashboard");
            }

            if (principal.IsInRole("Coach"))
            {
                return RedirectToAction("Index", "CoachDashboard");
            }

            if (principal.IsInRole("Student"))
            {
                return RedirectToAction("Index", "StudentDashboard");
            }

            return RedirectToAction("Index", "AdminDashboard");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                Response.Cookies.Delete($"xyz_pp_{userId}", new CookieOptions { Path = "/" });
            }

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(model);

            var ok = await _apiClient.ForgotPasswordAsync(model.Email.Trim(), ct);

            model.InfoMessage = "Eğer hesap mevcutsa şifre belirleme bağlantısı gönderilecektir.";

            return View(model);
        }

        [HttpGet]
        public IActionResult SetPassword(string? uid, string? token)
        {
            if (string.IsNullOrWhiteSpace(uid) || string.IsNullOrWhiteSpace(token))
            {
                return View(new SetPasswordViewModel
                {
                    ErrorMessage = "Bağlantı geçersiz. Lütfen tekrar deneyin."
                });
            }

            return View(new SetPasswordViewModel
            {
                UserId = uid,
                Token = token
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.Token = model.Token.Replace(' ', '+');

            var (ok, error) = await _apiClient.SetPasswordAsync(model.UserId, model.Token, model.NewPassword, ct);

            if (!ok)
            {
                model.ErrorMessage = error ?? "Şifre güncellenemedi.";
                return View(model);
            }

            TempData["SuccessMessage"] = "Şifre başarıyla güncellendi. Şimdi giriş yapabilirsiniz.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
