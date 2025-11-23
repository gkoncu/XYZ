using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Features.Attendances.Commands.UpdateSessionAttendance;
using XYZ.Application.Features.Attendances.Queries.GetSessionAttendance;
using XYZ.Application.Features.Attendances.Queries.GetStudentAttendanceHistory;
using XYZ.Application.Features.ClassSessions.Queries.GetMyTodaySessions;

namespace XYZ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AttendancesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _current;

        public AttendancesController(
            IMediator mediator,
            ICurrentUserService currentUser)
        {
            _mediator = mediator;
            _current = currentUser;
        }

        [HttpGet("today-sessions")]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        public async Task<ActionResult<IList<MyTodaySessionListItemDto>>> GetTodaySessions(
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetMyTodaySessionsQuery(),
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("session/{sessionId:int}")]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
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
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
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

            var updatedSessionId = await _mediator.Send(
                command,
                cancellationToken);

            return Ok(updatedSessionId);
        }

        [HttpGet("my-history")]
        [Authorize(Roles = "Student")]
        public async Task<ActionResult<IList<StudentAttendanceHistoryItemDto>>> GetMyAttendanceHistory(
            CancellationToken cancellationToken)
        {
            if (!_current.StudentId.HasValue)
                return Forbid();

            var result = await _mediator.Send(
                new GetStudentAttendanceHistoryQuery
                {
                    StudentId = _current.StudentId.Value
                },
                cancellationToken);

            return Ok(result);
        }
    }
}
