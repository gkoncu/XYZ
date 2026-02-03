using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XYZ.Application.Features.ProgressMetricDefinitions.Commands.CreateProgressMetricDefinition;
using XYZ.Application.Features.ProgressMetricDefinitions.Commands.DeleteProgressMetricDefinition;
using XYZ.Application.Features.ProgressMetricDefinitions.Commands.UpdateProgressMetricDefinition;
using XYZ.Application.Features.ProgressMetricDefinitions.Queries;
using XYZ.Application.Features.ProgressMetricDefinitions.Queries.GetProgressMetricDefinitionById;
using XYZ.Application.Features.ProgressMetricDefinitions.Queries.GetProgressMetricDefinitions;

namespace XYZ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Coach,SuperAdmin")]
    public class ProgressMetricDefinitionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProgressMetricDefinitionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IList<ProgressMetricDefinitionListItemDto>), 200)]
        public async Task<ActionResult<IList<ProgressMetricDefinitionListItemDto>>> GetAll(
            [FromQuery] int branchId,
            [FromQuery] bool includeInactive = false,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetProgressMetricDefinitionsQuery
            {
                BranchId = branchId,
                IncludeInactive = includeInactive
            }, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProgressMetricDefinitionDetailDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ProgressMetricDefinitionDetailDto>> GetById(int id, CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(new GetProgressMetricDefinitionByIdQuery { Id = id }, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), 201)]
        public async Task<ActionResult<int>> Create([FromBody] CreateProgressMetricDefinitionCommand command, CancellationToken cancellationToken = default)
        {
            var id = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<ActionResult<int>> Update(int id, [FromBody] UpdateProgressMetricDefinitionCommand command, CancellationToken cancellationToken = default)
        {
            command.Id = id;
            var updatedId = await _mediator.Send(command, cancellationToken);
            return Ok(updatedId);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<ActionResult<int>> Delete(int id, CancellationToken cancellationToken = default)
        {
            var deletedId = await _mediator.Send(new DeleteProgressMetricDefinitionCommand { Id = id }, cancellationToken);
            return Ok(deletedId);
        }
    }
}
