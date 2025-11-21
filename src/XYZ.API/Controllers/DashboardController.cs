using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("super-admin")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(SuperAdminDashboardDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<SuperAdminDashboardDto>> GetSuperAdminDashboard(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetSuperAdminDashboardQuery(), cancellationToken);
            return Ok(result);
        }

        [HttpGet("admin-coach")]
        [Authorize(Roles = "Admin,Coach")]
        [ProducesResponseType(typeof(AdminCoachDashboardDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<AdminCoachDashboardDto>> GetAdminCoachDashboard(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAdminCoachDashboardQuery(), cancellationToken);
            return Ok(result);
        }

        [HttpGet("student")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(StudentDashboardDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<StudentDashboardDto>> GetStudentDashboard(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetStudentDashboardQuery(), cancellationToken);
            return Ok(result);
        }
    }
}
