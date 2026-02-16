using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XYZ.Application.Common.Models;
using XYZ.Application.Features.Classes.Commands.AssignCoachToClass;
using XYZ.Application.Features.Classes.Commands.AssignStudentToClass;
using XYZ.Application.Features.Classes.Commands.CreateClass;
using XYZ.Application.Features.Classes.Commands.DeleteClass;
using XYZ.Application.Features.Classes.Commands.UnassignCoachToClass;
using XYZ.Application.Features.Classes.Commands.UnassignStudentFromClass;
using XYZ.Application.Features.Classes.Commands.UpdateClass;
using XYZ.Application.Features.Classes.Queries.GetAllClasses;
using XYZ.Application.Features.Classes.Queries.GetClassById;

namespace XYZ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClassesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClassesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<PaginationResult<ClassListItemDto>>> GetAll(
            [FromQuery] GetAllClassesQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ClassDetailDto>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetClassByIdQuery { ClassId = id },
                cancellationToken);

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreateClassCommand command,
            CancellationToken cancellationToken)
        {
            var id = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<int>> Update(
            int id,
            [FromBody] UpdateClassCommand command,
            CancellationToken cancellationToken)
        {
            command.ClassId = id;

            var updatedId = await _mediator.Send(command, cancellationToken);
            return Ok(updatedId);
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<int>> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            var deletedId = await _mediator.Send(
                new DeleteClassCommand { ClassId = id },
                cancellationToken);

            return Ok(deletedId);
        }

        [HttpPost("{id:int}/assign-student")]
        public async Task<ActionResult<int>> AssignStudent(
            int id,
            [FromBody] AssignStudentToClassCommand command,
            CancellationToken cancellationToken)
        {
            command.ClassId = id;

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{id:int}/unassign-student")]
        public async Task<ActionResult<int>> UnassignStudent(
            int id,
            [FromBody] UnassignStudentFromClassCommand command,
            CancellationToken cancellationToken)
        {
            command.ClassId = id;

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{id:int}/assign-coach")]
        public async Task<ActionResult<int>> AssignCoach(
            int id,
            [FromBody] AssignCoachToClassCommand command,
            CancellationToken cancellationToken)
        {
            command.ClassId = id;

            try
            {
                var result = await _mediator.Send(command, cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id:int}/unassign-coach")]
        public async Task<ActionResult<int>> UnassignCoach(
            int id,
            [FromBody] UnassignCoachFromClassCommand command,
            CancellationToken cancellationToken)
        {
            command.ClassId = id;

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
    }
}
