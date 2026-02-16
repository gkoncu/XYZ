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
using XYZ.Application.Features.ClassSessions.Queries.GetMyClassSessions;

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
        [ProducesResponseType(typeof(PaginationResult<ClassSessionListItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginationResult<ClassSessionListItemDto>>> GetAll(
            [FromQuery] GetClassSessionsQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("my")]
        [ProducesResponseType(typeof(PaginationResult<ClassSessionListItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginationResult<ClassSessionListItemDto>>> GetMy(
            [FromQuery] GetMyClassSessionsQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ClassSessionDetailDto), StatusCodes.Status200OK)]
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
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreateClassSessionCommand command,
            CancellationToken cancellationToken)
        {
            var id = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
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
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
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
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
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
