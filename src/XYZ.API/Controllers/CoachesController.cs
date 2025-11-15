using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XYZ.Application.Common.Models;
using XYZ.Application.Features.Coaches.Commands.CreateCoach;
using XYZ.Application.Features.Coaches.Commands.DeleteCoach;
using XYZ.Application.Features.Coaches.Commands.UpdateCoach;
using XYZ.Application.Features.Coaches.Queries.GetAllCoaches;
using XYZ.Application.Features.Coaches.Queries.GetCoachById;

namespace XYZ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CoachesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CoachesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        public async Task<ActionResult<PaginationResult<CoachListItemDto>>> GetAll(
            [FromQuery] GetAllCoachesQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        public async Task<ActionResult<CoachDetailDto>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetCoachByIdQuery { CoachId = id },
                cancellationToken);

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreateCoachCommand command,
            CancellationToken cancellationToken)
        {
            var id = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<int>> Update(
            int id,
            [FromBody] UpdateCoachCommand command,
            CancellationToken cancellationToken)
        {
            command.CoachId = id;

            var updatedId = await _mediator.Send(command, cancellationToken);
            return Ok(updatedId);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<int>> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            var deletedId = await _mediator.Send(
                new DeleteCoachCommand { CoachId = id },
                cancellationToken);

            return Ok(deletedId);
        }
    }
}
