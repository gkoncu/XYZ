using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        [ProducesResponseType(typeof(StudentPaymentPlanDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StudentPaymentPlanDto>> GetByStudent(int studentId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetStudentPaymentPlanQuery { StudentId = studentId }, cancellationToken);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("by-student/{studentId:int}/history")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHistoryByStudent(int studentId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetStudentPaymentPlanHistoryQuery { StudentId = studentId }, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{planId:int}")]
        [ProducesResponseType(typeof(StudentPaymentPlanDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StudentPaymentPlanDto>> GetDetails(int planId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetPaymentPlanDetailsQuery { PlanId = planId }, cancellationToken);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        public async Task<ActionResult<int>> Create([FromBody] CreatePaymentPlanCommand command, CancellationToken cancellationToken)
        {
            var id = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetByStudent), new { studentId = command.StudentId }, id);
        }

        [HttpPost("{planId:int}/cancel")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> Cancel(int planId, CancellationToken cancellationToken)
        {
            var id = await _mediator.Send(new CancelPaymentPlanCommand { PlanId = planId }, cancellationToken);
            return Ok(id);
        }

        [HttpPost("{planId:int}/archive")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> Archive(int planId, CancellationToken cancellationToken)
        {
            var id = await _mediator.Send(new ArchivePaymentPlanCommand { PlanId = planId }, cancellationToken);
            return Ok(id);
        }

        [HttpGet("my")]
        [ProducesResponseType(typeof(StudentPaymentPlanDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StudentPaymentPlanDto>> GetMyPlan(CancellationToken cancellationToken)
        {
            if (!_currentUser.StudentId.HasValue)
                return Forbid();

            var result = await _mediator.Send(new GetStudentPaymentPlanQuery { StudentId = _currentUser.StudentId.Value }, cancellationToken);

            if (result == null)
                return NotFound();

            return Ok(result);
        }
    }
}
