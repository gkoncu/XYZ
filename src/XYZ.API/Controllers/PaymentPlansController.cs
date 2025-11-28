using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Features.PaymentPlans.Commands.CreatePaymentPlan;
using XYZ.Application.Features.PaymentPlans.Queries.GetStudentPaymentPlan;

namespace XYZ.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentPlansController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUser;

        public PaymentPlansController(IMediator mediator, ICurrentUserService currentUser)
        {
            _mediator = mediator;
            _currentUser = currentUser;
        }

        [HttpGet("by-student/{studentId:int}")]
        [Authorize(Roles = "Admin,Coach,Student,SuperAdmin")]
        [ProducesResponseType(typeof(StudentPaymentPlanDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StudentPaymentPlanDto>> GetByStudent(
            int studentId,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetStudentPaymentPlanQuery { StudentId = studentId },
                cancellationToken);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        public async Task<ActionResult<int>> Create(
            [FromBody] CreatePaymentPlanCommand command,
            CancellationToken cancellationToken)
        {
            var id = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetByStudent),
                new { studentId = command.StudentId },
                id);
        }

        [HttpGet("my")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(StudentPaymentPlanDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StudentPaymentPlanDto>> GetMyPlan(
            CancellationToken cancellationToken)
        {
            if (!_currentUser.StudentId.HasValue)
            {
                return Forbid();
            }

            var studentId = _currentUser.StudentId.Value;

            var result = await _mediator.Send(
                new GetStudentPaymentPlanQuery { StudentId = studentId },
                cancellationToken);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}
