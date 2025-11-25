using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Features.Attendances.Queries.GetAttendanceOverview;
using XYZ.Application.Features.Attendances.Queries.GetStudentAttendanceHistory;

namespace XYZ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Coach,SuperAdmin")]
    public class ReportsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IExcelExporter _excelExporter;

        public ReportsController(
            IMediator mediator,
            IExcelExporter excelExporter)
        {
            _mediator = mediator;
            _excelExporter = excelExporter;
        }

        [HttpGet("attendance/student-history")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportStudentAttendanceHistory(
            [FromQuery] int studentId,
            [FromQuery] DateOnly? from,
            [FromQuery] DateOnly? to,
            CancellationToken ct)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var effectiveTo = to ?? today;
            var effectiveFrom = from ?? effectiveTo.AddDays(-30);

            var query = new GetStudentAttendanceHistoryQuery
            {
                StudentId = studentId,
                From = effectiveFrom,
                To = effectiveTo
            };

            var rows = await _mediator.Send(query, ct);

            var bytes = _excelExporter.Export(rows, "StudentAttendance");

            var fromPart = effectiveFrom.ToString("yyyyMMdd");
            var toPart = effectiveTo.ToString("yyyyMMdd");
            var fileName = $"student-{studentId}-attendance-{fromPart}-{toPart}.xlsx";

            const string contentType =
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(bytes, contentType, fileName);
        }

        [HttpGet("attendance/class-overview")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportClassAttendanceOverview(
            [FromQuery] int classId,
            [FromQuery] DateOnly from,
            [FromQuery] DateOnly to,
            CancellationToken ct)
        {
            var overview = await _mediator.Send(
                new GetAttendanceOverviewQuery
                {
                    ClassId = classId,
                    From = from,
                    To = to
                }, ct);

            var rows = new List<AttendanceOverviewDto> { overview };

            var bytes = _excelExporter.Export(rows, "ClassOverview");

            var fromPart = from.ToString("yyyyMMdd");
            var toPart = to.ToString("yyyyMMdd");
            var fileName = $"class-{classId}-overview-{fromPart}-{toPart}.xlsx";

            const string contentType =
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(bytes, contentType, fileName);
        }
    }
}
