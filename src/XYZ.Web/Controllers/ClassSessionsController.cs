using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using XYZ.Application.Common.Models;
using XYZ.Application.Features.ClassSessions.Queries.GetClassSessionById;
using XYZ.Application.Features.ClassSessions.Queries.GetClassSessions;
using XYZ.Web.Services;

namespace XYZ.Web.Controllers
{
    [Authorize(Roles = "Admin,Coach,SuperAdmin")]
    public class ClassSessionsController : Controller
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger<ClassSessionsController> _logger;

        public ClassSessionsController(
            IApiClient apiClient,
            ILogger<ClassSessionsController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            DateOnly? from,
            DateOnly? to,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken ct = default)
        {
            var effectiveFrom = from ?? DateOnly.FromDateTime(DateTime.Today);
            var effectiveTo = to ?? effectiveFrom;

            ViewBag.From = effectiveFrom.ToString("yyyy-MM-dd");
            ViewBag.To = effectiveTo.ToString("yyyy-MM-dd");

            var emptyModel = new PaginationResult<ClassSessionListItemDto>
            {
                Items = new List<ClassSessionListItemDto>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = 0
            };

            try
            {
                var path =
                    $"classsessions?from={effectiveFrom:yyyy-MM-dd}" +
                    $"&to={effectiveTo:yyyy-MM-dd}" +
                    $"&pageNumber={pageNumber}" +
                    $"&pageSize={pageSize}";

                var response = await _apiClient.GetAsync(path, ct);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "classsessions isteği başarısız. StatusCode: {StatusCode}",
                        response.StatusCode);

                    TempData["ErrorMessage"] = "Seanslar yüklenirken bir hata oluştu.";
                    return View(emptyModel);
                }

                var result = await response.Content
                    .ReadFromJsonAsync<PaginationResult<ClassSessionListItemDto>>(cancellationToken: ct);

                if (result is null)
                {
                    TempData["ErrorMessage"] = "Seans verileri çözümlenemedi.";
                    return View(emptyModel);
                }

                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Seanslar alınırken beklenmeyen hata oluştu.");
                TempData["ErrorMessage"] = "Seanslar yüklenirken beklenmeyen bir hata oluştu.";
                return View(emptyModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken ct = default)
        {
            try
            {
                var response = await _apiClient.GetAsync($"classsessions/{id}", ct);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return NotFound();
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "classsessions/{Id} isteği başarısız. StatusCode: {StatusCode}",
                        id,
                        response.StatusCode);

                    TempData["ErrorMessage"] = "Seans detayı yüklenirken bir hata oluştu.";
                    return RedirectToAction(nameof(Index));
                }

                var dto = await response.Content
                    .ReadFromJsonAsync<ClassSessionDetailDto>(cancellationToken: ct);

                if (dto is null)
                {
                    return NotFound();
                }

                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Seans detayı alınırken beklenmeyen hata oluştu. SessionId: {SessionId}",
                    id);

                TempData["ErrorMessage"] = "Seans detayı yüklenirken beklenmeyen bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
