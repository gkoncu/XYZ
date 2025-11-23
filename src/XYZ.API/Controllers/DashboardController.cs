using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Features.Dashboard.Queries.GetAdminCoachDashboard;
using XYZ.Application.Features.Dashboard.Queries.GetStudentDashboard;
using XYZ.Application.Features.Dashboard.Queries.GetSuperAdminDashboard;

namespace XYZ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("admin-coach")]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        public async Task<ActionResult<AdminCoachDashboardDto>> GetAdminCoachDashboard(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAdminCoachDashboardQuery(), cancellationToken);
            return Ok(result);
        }

        [HttpGet("student")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<StudentDashboardDto>> GetStudentDashboard(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetStudentDashboardQuery(), cancellationToken);
            return Ok(result);
        }

        [HttpGet("super-admin")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<SuperAdminDashboardDto>> GetSuperAdminDashboard(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetSuperAdminDashboardQuery(), cancellationToken);
            return Ok(result);
        }
    }
}
