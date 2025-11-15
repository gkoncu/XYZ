using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XYZ.Application.Common.Models;
using XYZ.Application.Features.Branches.Commands.CreateBranch;
using XYZ.Application.Features.Branches.Commands.DeleteBranch;
using XYZ.Application.Features.Branches.Commands.UpdateBranch;
using XYZ.Application.Features.Branches.Queries.GetAllBranches;
using XYZ.Application.Features.Branches.Queries.GetBranchById;

namespace XYZ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BranchesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BranchesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        public async Task<ActionResult<PaginationResult<BranchListItemDto>>> GetAll(
            [FromQuery] GetAllBranchesQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Coach,SuperAdmin")]
        public async Task<ActionResult<BranchDetailDto>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetBranchByIdQuery { BranchId = id },
                cancellationToken);

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreateBranchCommand command,
            CancellationToken cancellationToken)
        {
            var id = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<int>> Update(
            int id,
            [FromBody] UpdateBranchCommand command,
            CancellationToken cancellationToken)
        {
            command.BranchId = id;

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
                new DeleteBranchCommand { BranchId = id },
                cancellationToken);

            return Ok(deletedId);
        }
    }
}
