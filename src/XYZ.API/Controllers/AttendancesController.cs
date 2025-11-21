using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Features.Attendances.Queries.GetSessionAttendance;
using XYZ.Application.Features.Attendances.Queries.GetStudentAttendanceHistory;

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

        [HttpGet("session/{sessionId:int}")]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        [ProducesResponseType(typeof(SessionAttendanceDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<SessionAttendanceDto>> GetSessionAttendance(
            int sessionId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetSessionAttendanceQuery { ClassSessionId = sessionId },
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("student/{studentId:int}")]
        [Authorize(Roles = "Admin,Coach,SuperAdmin,Student")]
        [ProducesResponseType(typeof(IList<StudentAttendanceHistoryItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IList<StudentAttendanceHistoryItemDto>>> GetStudentAttendanceHistory(
            int studentId,
            CancellationToken cancellationToken)
        {
            var query = new GetStudentAttendanceHistoryQuery
            {
                StudentId = studentId
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
}
