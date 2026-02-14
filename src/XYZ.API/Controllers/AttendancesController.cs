using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Models;
using XYZ.Application.Features.Attendances.Commands.UpdateSessionAttendance;
using XYZ.Application.Features.Attendances.Queries.GetAttendanceList;
using XYZ.Application.Features.Attendances.Queries.GetSessionAttendance;
using XYZ.Application.Features.ClassSessions.Queries.GetMyTodaySessions;
using XYZ.Domain.Constants;

namespace XYZ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AttendancesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AttendancesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("today-sessions")]
        [Authorize(Roles = RoleNames.AdminCoachOrSuperAdmin)]
        public async Task<ActionResult<IList<MyTodaySessionListItemDto>>> GetTodaySessions(
            [FromQuery] DateOnly? date,
            CancellationToken cancellationToken)
        {
            var query = new GetMyTodaySessionsQuery
            {
                Date = date
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("session/{sessionId:int}")]
        public async Task<ActionResult<SessionAttendanceDto>> GetSessionAttendance(
            int sessionId,
            CancellationToken cancellationToken)
        {
            var dto = await _mediator.Send(
                new GetSessionAttendanceQuery
                {
                    ClassSessionId = sessionId
                },
                cancellationToken);

            return Ok(dto);
        }

        [HttpPut("session/{sessionId:int}")]
        public async Task<ActionResult<int>> UpdateSessionAttendance(
            int sessionId,
            [FromBody] UpdateSessionAttendanceCommand command,
            CancellationToken cancellationToken)
        {
            if (command.SessionId == 0)
            {
                command.SessionId = sessionId;
            }
            else if (command.SessionId != sessionId)
            {
                return BadRequest("Route ve gövde SessionId değerleri uyuşmuyor.");
            }

            var updatedSessionId = await _mediator.Send(command, cancellationToken);
            return Ok(updatedSessionId);
        }

        [HttpGet("list")]
        [ProducesResponseType(typeof(PaginationResult<AttendanceListItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginationResult<AttendanceListItemDto>>> GetList(
            [FromQuery] GetAttendanceListQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
}
