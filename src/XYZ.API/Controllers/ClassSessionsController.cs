using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Models;
using XYZ.Application.Features.ClassSessions.Commands.ChangeSessionStatus;
using XYZ.Application.Features.ClassSessions.Commands.CreateClassSession;
using XYZ.Application.Features.ClassSessions.Commands.DeleteClassSession;
using XYZ.Application.Features.ClassSessions.Commands.UpdateClassSession;
using XYZ.Application.Features.ClassSessions.Queries.GetClassSessionById;
using XYZ.Application.Features.ClassSessions.Queries.GetClassSessions;

namespace XYZ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClassSessionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClassSessionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        [ProducesResponseType(typeof(PaginationResult<ClassSessionListItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginationResult<ClassSessionListItemDto>>> GetAll(
            [FromQuery] GetClassSessionsQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Coach,SuperAdmin,Student")]
        [ProducesResponseType(typeof(ClassSessionDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ClassSessionDetailDto>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetClassSessionByIdQuery { Id = id },
                cancellationToken);

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreateClassSessionCommand command,
            CancellationToken cancellationToken)
        {
            var id = await _mediator.Send(command, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<int>> Update(
            int id,
            [FromBody] UpdateClassSessionCommand command,
            CancellationToken cancellationToken)
        {
            if (command.SessionId == 0)
            {
                command.SessionId = id;
            }
            else if (command.SessionId != id)
            {
                return BadRequest("Route id ile gövdedeki SessionId uyuşmuyor.");
            }

            var updatedId = await _mediator.Send(command, cancellationToken);
            return Ok(updatedId);
        }

        [HttpPost("{id:int}/status")]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<int>> ChangeStatus(
            int id,
            [FromBody] ChangeSessionStatusCommand command,
            CancellationToken cancellationToken)
        {
            if (command.SessionId == 0)
            {
                command.SessionId = id;
            }
            else if (command.SessionId != id)
            {
                return BadRequest("Route id ile gövdedeki SessionId uyuşmuyor.");
            }

            var updatedId = await _mediator.Send(command, cancellationToken);
            return Ok(updatedId);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<int>> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            var deletedId = await _mediator.Send(
                new DeleteClassSessionCommand { Id = id },
                cancellationToken);

            return Ok(deletedId);
        }
    }
}
