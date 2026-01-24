using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Models;
using XYZ.Application.Features.Announcements.Commands.CreateAnnouncement;
using XYZ.Application.Features.Announcements.Commands.CreateSystemAnnouncementForAllTenants;
using XYZ.Application.Features.Announcements.Commands.DeleteAnnouncement;
using XYZ.Application.Features.Announcements.Commands.UpdateAnnouncement;
using XYZ.Application.Features.Announcements.Queries.GetAllAnnouncements;
using XYZ.Application.Features.Announcements.Queries.GetAnnouncementById;

namespace XYZ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnnouncementsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AnnouncementsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,SuperAdmin,Student")]
        [ProducesResponseType(typeof(PaginationResult<AnnouncementListItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginationResult<AnnouncementListItemDto>>> GetAll(
            [FromQuery] GetAllAnnouncementsQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Coach,SuperAdmin,Student")]
        [ProducesResponseType(typeof(AnnouncementDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AnnouncementDetailDto>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetAnnouncementByIdQuery { Id = id },
                cancellationToken);

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreateAnnouncementCommand command,
            CancellationToken cancellationToken)
        {
            var id = await _mediator.Send(command, cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> Update(
            int id,
            [FromBody] UpdateAnnouncementCommand command,
            CancellationToken cancellationToken)
        {
            command.Id = id;

            var updatedId = await _mediator.Send(command, cancellationToken);
            return Ok(updatedId);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            var deletedId = await _mediator.Send(
                new DeleteAnnouncementCommand { Id = id },
                cancellationToken);

            return Ok(deletedId);
        }

        [HttpPost("system/broadcast")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> BroadcastSystem([FromBody] CreateSystemAnnouncementForAllTenantsCommand command,CancellationToken cancellationToken)
        {
            var count = await _mediator.Send(command, cancellationToken);
            return Ok(count);
        }

    }
}
