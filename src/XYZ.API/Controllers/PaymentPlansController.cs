using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using XYZ.Application.Common.Interfaces;
using XYZ.Application.Features.PaymentPlans.Commands.ArchivePaymentPlan;
using XYZ.Application.Features.PaymentPlans.Commands.CancelPaymentPlan;
using XYZ.Application.Features.PaymentPlans.Commands.CreatePaymentPlan;
using XYZ.Application.Features.PaymentPlans.Queries.GetPaymentPlanDetails;
using XYZ.Application.Features.PaymentPlans.Queries.GetStudentPaymentPlan;
using XYZ.Application.Features.PaymentPlans.Queries.GetStudentPaymentPlanHistory;

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
        public async Task<ActionResult<StudentPaymentPlanDto>> GetByStudent(int studentId, CancellationToken cancellationToken)
        {
            if (User.IsInRole("Student") && _currentUser.StudentId.HasValue && _currentUser.StudentId.Value != studentId)
                return Forbid();

            var result = await _mediator.Send(new GetStudentPaymentPlanQuery { StudentId = studentId }, cancellationToken);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("by-student/{studentId:int}/history")]
        [Authorize(Roles = "Admin,Coach,Student,SuperAdmin")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHistoryByStudent(int studentId, CancellationToken cancellationToken)
        {
            if (User.IsInRole("Student") && _currentUser.StudentId.HasValue && _currentUser.StudentId.Value != studentId)
                return Forbid();

            var result = await _mediator.Send(new GetStudentPaymentPlanHistoryQuery { StudentId = studentId }, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{planId:int}")]
        [Authorize(Roles = "Admin,Coach,Student,SuperAdmin")]
        [ProducesResponseType(typeof(StudentPaymentPlanDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StudentPaymentPlanDto>> GetDetails(int planId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetPaymentPlanDetailsQuery { PlanId = planId }, cancellationToken);

            if (result == null)
                return NotFound();

            if (User.IsInRole("Student") && _currentUser.StudentId.HasValue && _currentUser.StudentId.Value != result.StudentId)
                return Forbid();

            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        public async Task<ActionResult<int>> Create([FromBody] CreatePaymentPlanCommand command, CancellationToken cancellationToken)
        {
            var id = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetByStudent), new { studentId = command.StudentId }, id);
        }

        [HttpPost("{planId:int}/cancel")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> Cancel(int planId, CancellationToken cancellationToken)
        {
            var id = await _mediator.Send(new CancelPaymentPlanCommand { PlanId = planId }, cancellationToken);
            return Ok(id);
        }

        [HttpPost("{planId:int}/archive")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> Archive(int planId, CancellationToken cancellationToken)
        {
            var id = await _mediator.Send(new ArchivePaymentPlanCommand { PlanId = planId }, cancellationToken);
            return Ok(id);
        }

        [HttpGet("my")]
        [Authorize(Roles = "Student")]
        [ProducesResponseType(typeof(StudentPaymentPlanDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StudentPaymentPlanDto>> GetMyPlan(CancellationToken cancellationToken)
        {
            if (!_currentUser.StudentId.HasValue)
                return Forbid();

            var studentId = _currentUser.StudentId.Value;

            var result = await _mediator.Send(new GetStudentPaymentPlanQuery { StudentId = studentId }, cancellationToken);

            if (result == null)
                return NotFound();

            return Ok(result);
        }
    }
}
