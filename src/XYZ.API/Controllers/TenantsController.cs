using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XYZ.Application.Common.Models;
using XYZ.Application.Features.Tenants.Commands.CreateTenant;
using XYZ.Application.Features.Tenants.Commands.DeleteTenant;
using XYZ.Application.Features.Tenants.Commands.UpdateCurrentTenantTheme;
using XYZ.Application.Features.Tenants.Commands.UpdateTenant;
using XYZ.Application.Features.Tenants.Queries.GetAllTenants;
using XYZ.Application.Features.Tenants.Queries.GetCurrentTenantTheme;
using XYZ.Application.Features.Tenants.Queries.GetTenantsById;
using XYZ.Domain.Constants;

namespace XYZ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TenantsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TenantsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<PaginationResult<TenantListItemDto>>> GetAll(
            [FromQuery] GetAllTenantsQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TenantDetailDto>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetTenantByIdQuery { TenantId = id },
                cancellationToken);

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreateTenantCommand command,
            CancellationToken cancellationToken)
        {
            var id = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<int>> Update(
            int id,
            [FromBody] UpdateTenantCommand command,
            CancellationToken cancellationToken)
        {
            command.TenantId = id;

            var updatedId = await _mediator.Send(command, cancellationToken);
            return Ok(updatedId);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<int>> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            var deletedId = await _mediator.Send(
                new DeleteTenantCommand { TenantId = id },
                cancellationToken);

            return Ok(deletedId);
        }

        [HttpGet("current-theme")]
        public async Task<ActionResult<TenantThemeDto>> GetCurrentTheme(CancellationToken ct)
        => Ok(await _mediator.Send(new GetCurrentTenantThemeQuery(), ct));

        [HttpPut("current-theme")]
        public async Task<IActionResult> UpdateCurrentTheme([FromBody] UpdateCurrentTenantThemeCommand command, CancellationToken ct)
        {
            await _mediator.Send(command, ct);
            return NoContent();
        }

    }
}
