using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XYZ.Application.Common.Models;
using XYZ.Application.Features.Audit.Queries.GetAuditEventById;
using XYZ.Application.Features.Audit.Queries.GetAuditEvents;

namespace XYZ.API.Controllers
{
    [ApiController]
    [Route("api/audit-events")]
    [Authorize]
    public sealed class AuditEventsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuditEventsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginationResult<AuditEventListItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginationResult<AuditEventListItemDto>>> GetAll(
            [FromQuery] GetAuditEventsQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(AuditEventDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AuditEventDetailDto>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAuditEventByIdQuery { Id = id }, cancellationToken);
            if (result is null) return NotFound();
            return Ok(result);
        }
    }
}
