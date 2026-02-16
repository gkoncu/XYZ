using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XYZ.Application.Features.ProgressRecords.Commands.CreateProgressRecord;
using XYZ.Application.Features.ProgressRecords.Commands.DeleteProgressRecord;
using XYZ.Application.Features.ProgressRecords.Commands.UpdateProgressRecord;
using XYZ.Application.Features.ProgressRecords.Queries.GetProgressRecordById;
using XYZ.Application.Features.ProgressRecords.Queries.GetStudentProgressRecords;

namespace XYZ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProgressRecordsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProgressRecordsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProgressRecordDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProgressRecordDetailDto>> GetById(int id, CancellationToken ct)
        {
            try
            {
                var result = await _mediator.Send(new GetProgressRecordByIdQuery { Id = id }, ct);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("student/{studentId:int}")]
        [ProducesResponseType(typeof(IList<ProgressRecordListItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IList<ProgressRecordListItemDto>>> GetByStudent(
            int studentId,
            [FromQuery] DateOnly? from,
            [FromQuery] DateOnly? to,
            [FromQuery] int? branchId,
            CancellationToken ct)
        {
            try
            {
                var query = new GetStudentProgressRecordsQuery
                {
                    StudentId = studentId,
                    From = from,
                    To = to,
                    BranchId = branchId
                };

                var result = await _mediator.Send(query, ct);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        public async Task<ActionResult<int>> Create([FromBody] CreateProgressRecordCommand command, CancellationToken ct)
        {
            try
            {
                var id = await _mediator.Send(command, ct);
                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> Update(int id, [FromBody] UpdateProgressRecordCommand command, CancellationToken ct)
        {
            try
            {
                command.Id = id;
                var updatedId = await _mediator.Send(command, ct);
                return Ok(updatedId);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> Delete(int id, CancellationToken ct)
        {
            try
            {
                var deletedId = await _mediator.Send(new DeleteProgressRecordCommand { Id = id }, ct);
                return Ok(deletedId);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }
    }
}
