using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XYZ.Application.Features.DocumentDefinitions.Commands.CreateDocumentDefinition;
using XYZ.Application.Features.DocumentDefinitions.Commands.DeleteDocumentDefinition;
using XYZ.Application.Features.DocumentDefinitions.Commands.UpdateDocumentDefinition;
using XYZ.Application.Features.DocumentDefinitions.Queries;
using XYZ.Application.Features.DocumentDefinitions.Queries.GetDocumentDefinitionById;
using XYZ.Application.Features.DocumentDefinitions.Queries.GetDocumentDefinitions;
using XYZ.Domain.Enums;

namespace XYZ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentDefinitionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DocumentDefinitionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IList<DocumentDefinitionListItemDto>), 200)]
        public async Task<ActionResult<IList<DocumentDefinitionListItemDto>>> GetAll(
            [FromQuery] DocumentTarget? target,
            [FromQuery] bool includeInactive,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetDocumentDefinitionsQuery
            {
                Target = target,
                IncludeInactive = includeInactive
            }, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(DocumentDefinitionDetailDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<DocumentDefinitionDetailDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetDocumentDefinitionByIdQuery { Id = id }, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), 201)]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreateDocumentDefinitionCommand command,
            CancellationToken cancellationToken)
        {
            var id = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<ActionResult<int>> Update(
            int id,
            [FromBody] UpdateDocumentDefinitionCommand command,
            CancellationToken cancellationToken)
        {
            command.Id = id;
            var updatedId = await _mediator.Send(command, cancellationToken);
            return Ok(updatedId);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<ActionResult<int>> Delete(int id, CancellationToken cancellationToken)
        {
            var deletedId = await _mediator.Send(new DeleteDocumentDefinitionCommand { Id = id }, cancellationToken);
            return Ok(deletedId);
        }
    }
}
