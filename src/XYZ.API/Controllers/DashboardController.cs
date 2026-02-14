using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using XYZ.Application.Features.Dashboard.Queries.GetAdminCoachDashboard;
using XYZ.Application.Features.Dashboard.Queries.GetStudentDashboard;
using XYZ.Application.Features.Dashboard.Queries.GetSuperAdminDashboard;
using XYZ.Application.Features.Email.Options;

namespace XYZ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _env;
    private readonly IOptions<EmailOptions> _emailOptions;

    public DashboardController(
        IMediator mediator,
        IWebHostEnvironment env,
        IOptions<EmailOptions> emailOptions)
    {
        _mediator = mediator;
        _env = env;
        _emailOptions = emailOptions;
    }

    [HttpGet("admin-coach")]
    public async Task<ActionResult<AdminCoachDashboardDto>> GetAdminCoachDashboard(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAdminCoachDashboardQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("student")]
    public async Task<ActionResult<StudentDashboardDto>> GetStudentDashboard(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStudentDashboardQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("super-admin")]
    public async Task<ActionResult<SuperAdminDashboardDto>> GetSuperAdminDashboard(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSuperAdminDashboardQuery(), cancellationToken);

        result.SystemHealth.Environment = _env.EnvironmentName;
        result.SystemHealth.EmailEnabled = _emailOptions.Value.Enabled;
        result.SystemHealth.AppVersion =
            typeof(DashboardController).Assembly.GetName().Version?.ToString() ?? string.Empty;

        return Ok(result);
    }
}
